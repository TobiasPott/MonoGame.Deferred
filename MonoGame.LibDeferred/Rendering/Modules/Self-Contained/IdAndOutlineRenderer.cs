using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.Editor;
using DeferredEngine.Renderer.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Ext;
using static DeferredEngine.Renderer.Helper.DynamicMeshBatcher;

namespace DeferredEngine.Renderer.RenderModules
{
    public partial class IdAndOutlineRenderer
    {
        private readonly Vector4 _hoveredColor = new Vector4(1, 1, 1, 0.1f);
        private readonly Vector4 _selectedColor = new Vector4(1, 1, 0, 0.1f);


        private GraphicsDevice _graphicsDevice;

        private RenderTarget2D _idRenderTarget2D;

        public int HoveredId;

        private BillboardBuffer _billboardBuffer;

        public void Initialize(GraphicsDevice graphicsDevice, BillboardBuffer billboardBuffer)
        {
            _graphicsDevice = graphicsDevice;
            _billboardBuffer = billboardBuffer;
        }

        public void Draw(DynamicMeshBatcher meshMat, EntitySceneGroup scene, EnvironmentProbe envSample,
            PipelineMatrices matrices, GizmoDrawContext drawContext, bool mouseMoved)
        {
            if (drawContext.GizmoTransformationMode)
            {
                _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
                _graphicsDevice.Clear(Color.Black);
                return;
            }

            if (mouseMoved)
                DrawIds(meshMat, scene, matrices, envSample, drawContext);

            if (RenderingSettings.e_DrawOutlines)
                DrawOutlines(meshMat, matrices, mouseMoved, HoveredId, drawContext, mouseMoved);
        }

        public void DrawIds(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, PipelineMatrices matrices, EnvironmentProbe envSample, GizmoDrawContext gizmoContext)
        {

            _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdRender, matrices);

            //Now onto the billboards
            DrawBillboards(scene, envSample, matrices);

            //Now onto the gizmos
            DrawGizmos(matrices.ViewProjection, gizmoContext);

            Rectangle sourceRectangle =
            new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);

            Color[] retrievedColor = new Color[1];

            try
            {
                if (sourceRectangle.X >= 0 && sourceRectangle.Y >= 0 && sourceRectangle.X < _idRenderTarget2D.Width - 2 && sourceRectangle.Y < _idRenderTarget2D.Height - 2)
                    _idRenderTarget2D.GetData(0, sourceRectangle, retrievedColor, 0, 1);
            }
            catch
            {
                //nothing
            }

            HoveredId = IdGenerator.GetIdFromColor(retrievedColor[0]);
        }

        private void DrawBillboard(Matrix world, Matrix view, Matrix staticViewProjection, int id)
        {
            Shaders.Billboard.Param_WorldViewProj.SetValue(world * staticViewProjection);
            Shaders.Billboard.Param_WorldView.SetValue(world * view);
            Shaders.Billboard.Param_IdColor.SetValue(IdGenerator.GetColorFromId(id).ToVector3());
            Shaders.Billboard.Effect.CurrentTechnique.Passes[0].Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        public void DrawBillboards(EntitySceneGroup scene, EnvironmentProbe envSample, PipelineMatrices matrices)
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            Shaders.Billboard.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            Shaders.Billboard.Effect.CurrentTechnique = Shaders.Billboard.Technique_Id;

            Matrix staticViewProjection = matrices.StaticViewProjection;
            Matrix view = matrices.View;

            List<Decal> decals = scene.Decals;
            List<DeferredPointLight> pointLights = scene.PointLights;
            List<DeferredDirectionalLight> dirLights = scene.DirectionalLights;

            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawBillboard(decal.World, view, staticViewProjection, decal.Id);
            }

            for (int index = 0; index < pointLights.Count; index++)
            {
                var light = pointLights[index];
                DrawBillboard(light.World, view, staticViewProjection, light.Id);
            }

            for (int index = 0; index < dirLights.Count; index++)
            {
                var light = dirLights[index];
                DrawBillboard(light.World, view, staticViewProjection, light.Id);
            }

            Shaders.Billboard.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
            DrawBillboard(envSample.World, view, staticViewProjection, envSample.Id);

        }

        public void DrawGizmos(Matrix staticViewProjection, GizmoDrawContext gizmoContext)
        {
            if (gizmoContext.SelectedObjectId == 0) return;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.Opaque;

            Vector3 position = gizmoContext.SelectedObjectPosition;

            Matrix rotation = (RenderingStats.e_LocalTransformation || gizmoContext.GizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            //Z
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, 0, 0), 0.5f, new Color(1, 0, 0), staticViewProjection);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)-Math.PI / 2.0f, 0, 0), 0.5f, new Color(2, 0, 0), staticViewProjection);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, (float)Math.PI / 2.0f, 0), 0.5f, new Color(3, 0, 0), staticViewProjection);

            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)Math.PI, 0, 0), 0.5f, new Color(1, 0, 0), staticViewProjection);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)Math.PI / 2.0f, 0, 0), 0.5f, new Color(2, 0, 0), staticViewProjection);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, (float)-Math.PI / 2.0f, 0), 0.5f, new Color(3, 0, 0), staticViewProjection);

        }

        // ToDo: @tpott: Extract IdRender and Bilboard Shaders members
        public static void DrawArrow(GraphicsDevice graphicsDevice, Vector3 position, Matrix rotationObject, Vector3 angles, float scale, Color color, Matrix staticViewProjection, GizmoModes gizmoMode = GizmoModes.Translation)
        {
            Matrix rotation = angles.ToMatrixRotationXYZ();
            Matrix scaleMatrix = Matrix.CreateScale(0.75f, 0.75f, scale * 1.5f);
            Matrix worldViewProj = scaleMatrix * rotation * rotationObject * Matrix.CreateTranslation(position) * staticViewProjection;

            IdAndOutlineEffectSetup.Instance.Param_WorldViewProj.SetValue(worldViewProj);
            IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(color.ToVector4());
            IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();

            ModelMeshPart meshpart = gizmoMode == GizmoModes.Translation ? StaticAssets.Instance.EditorArrow3DMeshPart : StaticAssets.Instance.EditorArrow3DRoundMeshPart;
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            graphicsDevice.Indices = (meshpart.IndexBuffer);
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
        }

        public void DrawOutlines(DynamicMeshBatcher meshMat, PipelineMatrices matrices, bool drawAll, int hoveredId, GizmoDrawContext gizmoContext, bool mouseMoved)
        {
            _graphicsDevice.SetRenderTarget(_idRenderTarget2D);

            if (!mouseMoved)
                _graphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);
            else
            {
                _graphicsDevice.Clear(Color.Black);
            }
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


            int selectedId = gizmoContext.SelectedObjectId;
            //Selected entity
            if (selectedId != 0)
            {
                //UPdate the size of our outlines!

                if (!drawAll) meshMat.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, false, selectedId);

                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(_selectedColor);
                meshMat.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, outlined: true, outlineId: selectedId);
            }

            if (selectedId != hoveredId && hoveredId != 0 && mouseMoved)
            {
                if (!drawAll) meshMat.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, false, hoveredId);

                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(_hoveredColor);
                meshMat.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, outlined: true, outlineId: hoveredId);
            }
        }

        public RenderTarget2D GetRenderTarget2D()
        {
            return _idRenderTarget2D;
        }


        public void SetUpRenderTarget(int width, int height)
        {
            if (_idRenderTarget2D != null) _idRenderTarget2D.Dispose();

            _idRenderTarget2D = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }

        public bool ApplyShaders(RenderType renderType, Matrix localToWorldMatrix, Matrix viewProjection, MeshBatch meshLib, int index, int outlineId, bool outlined)
        {
            if (renderType == RenderType.IdRender || renderType == RenderType.IdOutline)
            {
                // ToDo: @tpott: Extract IdRender and Bilboard Shaders members
                IdAndOutlineEffectSetup.Instance.Param_WorldViewProj.SetValue(localToWorldMatrix * viewProjection);

                int id = meshLib.GetTransforms()[index].Id;

                if (renderType == RenderType.IdRender)
                {
                    IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(IdGenerator.GetColorFromId(id).ToVector4());
                    IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
                }
                if (renderType == RenderType.IdOutline)
                {
                    //Is this the Id we want to outline?
                    if (id == outlineId)
                    {
                        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                        IdAndOutlineEffectSetup.Instance.Param_World.SetValue(localToWorldMatrix);

                        if (outlined)
                            IdAndOutlineEffectSetup.Instance.Pass_Outline.Apply();
                        else
                            IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
                    }
                    else
                        return false;
                }

            }

            return true;
        }


    }
}
