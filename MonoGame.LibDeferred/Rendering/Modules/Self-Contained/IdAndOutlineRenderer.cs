using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Ext;

namespace DeferredEngine.Renderer.RenderModules
{
    public partial class IdAndOutlineRenderer
    {
        private readonly Vector4 _hoveredColor = new Vector4(1, 1, 1, 0.1f);
        private readonly Vector4 _selectedColor = new Vector4(1, 1, 0, 0.1f);


        private Color[] _readbackIdColor = new Color[1];
        private GraphicsDevice _graphicsDevice;

        private RenderTarget2D _idAndOutlineRenderTarget2D;

        public int HoveredId;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Draw(DynamicMeshBatcher meshMat, EntitySceneGroup scene, EnvironmentProbe envSample,
            PipelineMatrices matrices, GizmoDrawContext drawContext, bool mouseMoved)
        {
            if (drawContext.GizmoTransformationMode)
            {
                _graphicsDevice.SetRenderTarget(_idAndOutlineRenderTarget2D);
                _graphicsDevice.Clear(Color.Black);
                return;
            }

            if (mouseMoved)
                DrawIds(meshMat, scene, matrices, envSample, drawContext);

            if (RenderingSettings.e_DrawOutlines)
                DrawOutlines(meshMat, matrices, mouseMoved, HoveredId, drawContext, mouseMoved);
        }

        public BillboardRenderer BillboardRenderer;

        public void DrawIds(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, PipelineMatrices matrices, EnvironmentProbe envSample, GizmoDrawContext gizmoContext)
        {

            _graphicsDevice.SetRenderTarget(_idAndOutlineRenderTarget2D);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdRender, matrices);

            //Now onto the billboards
            BillboardRenderer?.DrawSceneBillboards(scene, envSample, matrices);

            //Now onto the gizmos
            DrawGizmos(matrices.ViewProjection, gizmoContext);

            Rectangle sourceRectangle =
            new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);

            try
            {
                if (sourceRectangle.X >= 0 && sourceRectangle.Y >= 0 && sourceRectangle.X < _idAndOutlineRenderTarget2D.Width - 2 && sourceRectangle.Y < _idAndOutlineRenderTarget2D.Height - 2)
                    _idAndOutlineRenderTarget2D.GetData(0, sourceRectangle, _readbackIdColor, 0, 1);
            }
            catch
            {
                //nothing
            }

            HoveredId = IdGenerator.GetIdFromColor(_readbackIdColor[0]);
        }


        public void DrawGizmos(PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            if (gizmoContext.SelectedObjectId == 0) return;

            Vector3 position = gizmoContext.SelectedObjectPosition;
            GizmoModes gizmoMode = gizmoContext.GizmoMode;
            Matrix rotation = (RenderingStats.e_LocalTransformation || gizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, 0, 0), this.HoveredId == 1 ? 1 : 0.5f, Color.Blue, matrices.StaticViewProjection, gizmoMode); //z 1
            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)-Math.PI / 2, 0, 0), this.HoveredId == 2 ? 1 : 0.5f, Color.Green, matrices.StaticViewProjection, gizmoMode); //y 2
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, (float)Math.PI / 2, 0), this.HoveredId == 3 ? 1 : 0.5f, Color.Red, matrices.StaticViewProjection, gizmoMode); //x 3

            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)Math.PI, 0, 0), this.HoveredId == 1 ? 1 : 0.5f, Color.Blue, matrices.StaticViewProjection, gizmoMode); //z 1
            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)Math.PI / 2, 0, 0), this.HoveredId == 2 ? 1 : 0.5f, Color.Green, matrices.StaticViewProjection, gizmoMode); //y 2
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, (float)-Math.PI / 2, 0), this.HoveredId == 3 ? 1 : 0.5f, Color.Red, matrices.StaticViewProjection, gizmoMode); //x 3
        }
        public void DrawGizmos(Matrix staticViewProjection, GizmoDrawContext gizmoContext)
        {
            if (gizmoContext.SelectedObjectId == 0) return;

            Vector3 position = gizmoContext.SelectedObjectPosition;
            GizmoModes gizmoMode = gizmoContext.GizmoMode;
            Matrix rotation = (RenderingStats.e_LocalTransformation || gizmoContext.GizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            //Z
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, 0, 0), this.HoveredId == 1 ? 1 : 0.5f, new Color(1, 0, 0), staticViewProjection, gizmoMode);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)-Math.PI / 2.0f, 0, 0), this.HoveredId == 2 ? 1 : 0.5f, new Color(2, 0, 0), staticViewProjection, gizmoMode);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, (float)Math.PI / 2.0f, 0), this.HoveredId == 3 ? 1 : 0.5f, new Color(3, 0, 0), staticViewProjection, gizmoMode);

            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)Math.PI, 0, 0), this.HoveredId == 1 ? 1 : 0.5f, new Color(1, 0, 0), staticViewProjection, gizmoMode);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3((float)Math.PI / 2.0f, 0, 0), this.HoveredId == 2 ? 1 : 0.5f, new Color(2, 0, 0), staticViewProjection, gizmoMode);
            DrawArrow(_graphicsDevice, position, rotation, new Vector3(0, (float)-Math.PI / 2.0f, 0), this.HoveredId == 3 ? 1 : 0.5f, new Color(3, 0, 0), staticViewProjection, gizmoMode);

        }
        public static void DrawArrow(GraphicsDevice graphicsDevice, Vector3 position, Matrix rotationObject, Vector3 angles, float scale,
            Color color, Matrix staticViewProjection, GizmoModes gizmoMode = GizmoModes.Translation, Vector3? direction = null)
        {
            Matrix rotation = (direction != null) ? Matrix.CreateLookAt(Vector3.Zero, (Vector3)direction, Vector3.UnitX) : angles.ToMatrixRotationXYZ();
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
            _graphicsDevice.SetRenderTarget(_idAndOutlineRenderTarget2D);

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
            return _idAndOutlineRenderTarget2D;
        }


        public void SetUpRenderTarget(int width, int height)
        {
            if (_idAndOutlineRenderTarget2D != null) _idAndOutlineRenderTarget2D.Dispose();

            _idAndOutlineRenderTarget2D = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }

        public bool ApplyShaders(DynamicMeshBatcher.RenderType renderType, Matrix localToWorldMatrix, Matrix viewProjection, MeshBatch meshLib, int index, int outlineId, bool outlined)
        {
            if (renderType == DynamicMeshBatcher.RenderType.IdRender || renderType == DynamicMeshBatcher.RenderType.IdOutline)
            {
                // ToDo: @tpott: Extract IdRender and Bilboard Shaders members
                IdAndOutlineEffectSetup.Instance.Param_WorldViewProj.SetValue(localToWorldMatrix * viewProjection);

                int id = meshLib.GetTransforms()[index].Id;

                if (renderType == DynamicMeshBatcher.RenderType.IdRender)
                {
                    IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(IdGenerator.GetColorFromId(id).ToVector4());
                    IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
                }
                if (renderType == DynamicMeshBatcher.RenderType.IdOutline)
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
