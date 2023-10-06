using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.Editor;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using static DeferredEngine.Renderer.RenderModules.IdAndOutlineRenderer;
using DirectionalLight = DeferredEngine.Entities.DirectionalLight;

namespace DeferredEngine.Renderer.RenderModules
{
    public class EditorRender
    {
        private IdAndOutlineRenderer _idAndOutlineRenderer;
        private GraphicsDevice _graphicsDevice;

        private BillboardBuffer _billboardBuffer;

        private Assets _assets;

        private double _mouseMoved;
        private bool _mouseMovement;
        private readonly double mouseMoveTimer = 400;

        public void Initialize(GraphicsDevice graphics, Assets assets)
        {
            _graphicsDevice = graphics;
            _assets = assets;

            _billboardBuffer = new BillboardBuffer(Color.White, graphics);
            _idAndOutlineRenderer = new IdAndOutlineRenderer();
            _idAndOutlineRenderer.Initialize(graphics, _billboardBuffer, _assets);

        }

        public void Update(GameTime gameTime)
        {
            if (RenderingStats.UIIsHovered || Input.mouseState.RightButton == ButtonState.Pressed)
            {
                _mouseMovement = false;
                return;
            }

            if (Input.mouseState != Input.mouseLastState)
            {
                //reset the timer!

                _mouseMoved = gameTime.TotalGameTime.TotalMilliseconds + mouseMoveTimer;
                _mouseMovement = true;
            }

            if (_mouseMoved < gameTime.TotalGameTime.TotalMilliseconds)
            {
                _mouseMovement = false;
            }

        }

        public void SetUpRenderTarget(int width, int height)
        {
            _idAndOutlineRenderer.SetUpRenderTarget(width, height);
        }


        public void DrawBillboards(List<Decal> decals, List<PointLight> lights, List<DirectionalLight> dirLights, EnvironmentProbe envSample, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            Shaders.BillboardEffect.CurrentTechnique = Shaders.BillboardEffectTechnique_Billboard;

            Shaders.BillboardEffectParameter_IdColor.SetValue(Color.Gray.ToVector3());

            //Decals

            Shaders.BillboardEffectParameter_Texture.SetValue(_assets.IconDecal);
            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawBillboard(decal, staticViewProjection, view, gizmoContext);
            }

            //Lights

            Shaders.BillboardEffectParameter_Texture.SetValue(_assets.IconLight);
            for (int index = 0; index < lights.Count; index++)
            {
                var light = lights[index];
                DrawBillboard(light, staticViewProjection, view, gizmoContext);
            }

            //DirectionalLights
            for (var index = 0; index < dirLights.Count; index++)
            {
                DirectionalLight light = dirLights[index];
                DrawBillboard(light, staticViewProjection, view, gizmoContext);

                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position, light.Direction * 10, 1, Color.Black, light.Color);
                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position + Vector3.UnitX * 10, light.Direction * 10, 1, Color.Black,
                        light.Color);
                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position - Vector3.UnitX * 10, light.Direction * 10, 1, Color.Black,
                        light.Color);
                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position + Vector3.UnitY * 10, light.Direction * 10, 1, Color.Black,
                        light.Color);
                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position - Vector3.UnitY * 10, light.Direction * 10, 1, Color.Black,
                        light.Color);
                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position + Vector3.UnitZ * 10, light.Direction * 10, 1, Color.Black,
                        light.Color);
                HelperGeometryManager.GetInstance()
                    .AddLineStartDir(light.Position - Vector3.UnitZ * 10, light.Direction * 10, 1, Color.Black,
                        light.Color);

                if (light.CastShadows)
                {
                    BoundingFrustum boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);

                    HelperGeometryManager.GetInstance().CreateBoundingBoxLines(boundingFrustumShadow);
                }
            }

            //EnvMap

            Shaders.BillboardEffectParameter_Texture.SetValue(_assets.IconEnvmap);

            DrawBillboard(envSample, staticViewProjection, view, gizmoContext);

        }
        private void DrawBillboard(TransformableObject billboardObject, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            Matrix world = Matrix.CreateTranslation(billboardObject.Position);
            Shaders.BillboardEffectParameter_WorldViewProj.SetValue(world * staticViewProjection);
            Shaders.BillboardEffectParameter_WorldView.SetValue(world * view);

            if (billboardObject.Id == GetHoveredId())
                Shaders.BillboardEffectParameter_IdColor.SetValue(Color.White.ToVector3());
            if (billboardObject.Id == gizmoContext.SelectedObjectId)
                Shaders.BillboardEffectParameter_IdColor.SetValue(Color.Gold.ToVector3());

            Shaders.BillboardEffect.CurrentTechnique.Passes[0].Apply();

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            if (billboardObject.Id == GetHoveredId() || billboardObject.Id == gizmoContext.SelectedObjectId)
                Shaders.BillboardEffectParameter_IdColor.SetValue(Color.Gray.ToVector3());
        }

        public void DrawIds(MeshMaterialLibrary meshMaterialLibrary, List<Decal> decals, List<PointLight> lights, List<DirectionalLight> dirLights, EnvironmentProbe envSample, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            _idAndOutlineRenderer.Draw(meshMaterialLibrary, decals, lights, dirLights, envSample, staticViewProjection, view, gizmoContext, _mouseMovement);
        }

        public void DrawEditorElements(MeshMaterialLibrary meshMaterialLibrary, List<Decal> decals, List<PointLight> lights, List<DirectionalLight> dirLights, EnvironmentProbe envSample, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            DrawGizmo(staticViewProjection, gizmoContext);
            DrawBillboards(decals, lights, dirLights, envSample, staticViewProjection, view, gizmoContext);
        }

        public void DrawGizmo(Matrix staticViewProjection, GizmoDrawContext gizmoContext)
        {
            if (gizmoContext.SelectedObjectId == 0) return;



            Vector3 position = gizmoContext.SelectedObjectPosition;
            GizmoModes gizmoMode = gizmoContext.GizmoMode;
            Matrix rotation = (RenderingStats.e_LocalTransformation || gizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            //Z
            DrawArrow(position, rotation, 0, 0, 0, GetHoveredId() == 1 ? 1 : 0.5f, Color.Blue, staticViewProjection, gizmoMode); //z 1
            DrawArrow(position, rotation, -Math.PI / 2, 0, 0, GetHoveredId() == 2 ? 1 : 0.5f, Color.Green, staticViewProjection, gizmoMode); //y 2
            DrawArrow(position, rotation, 0, Math.PI / 2, 0, GetHoveredId() == 3 ? 1 : 0.5f, Color.Red, staticViewProjection, gizmoMode); //x 3

            DrawArrow(position, rotation, Math.PI, 0, 0, GetHoveredId() == 1 ? 1 : 0.5f, Color.Blue, staticViewProjection, gizmoMode); //z 1
            DrawArrow(position, rotation, Math.PI / 2, 0, 0, GetHoveredId() == 2 ? 1 : 0.5f, Color.Green, staticViewProjection, gizmoMode); //y 2
            DrawArrow(position, rotation, 0, -Math.PI / 2, 0, GetHoveredId() == 3 ? 1 : 0.5f, Color.Red, staticViewProjection, gizmoMode); //x 3
            //DrawArrowRound(position, rotation, Math.PI, 0, 0, GetHoveredId() == 1 ? 1 : 0.5f, Color.Blue, staticViewProjection); //z 1
            //DrawArrowRound(position, rotation,-Math.PI / 2, 0, 0, GetHoveredId() == 2 ? 1 : 0.5f, Color.Green, staticViewProjection); //y 2
            //DrawArrowRound(position, rotation,0, Math.PI / 2, 0, GetHoveredId() == 3 ? 1 : 0.5f, Color.Red, staticViewProjection); //x 3
        }

        private void DrawArrow(Vector3 position, Matrix rotationObject, double angleX, double angleY, double angleZ, float scale, Color color, Matrix staticViewProjection, GizmoModes gizmoMode, Vector3? direction = null)
        {
            Matrix rotation;
            if (direction != null)
            {
                rotation = Matrix.CreateLookAt(Vector3.Zero, (Vector3)direction, Vector3.UnitX);


            }
            else
            {
                rotation = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                   Matrix.CreateRotationZ((float)angleZ);
            }

            Matrix scaleMatrix = Matrix.CreateScale(0.75f, 0.75f, scale * 1.5f);
            Matrix worldViewProj = scaleMatrix * rotation * rotationObject * Matrix.CreateTranslation(position) * staticViewProjection;

            Shaders.IdRenderEffectParameterWorldViewProj.SetValue(worldViewProj);
            Shaders.IdRenderEffectParameterColorId.SetValue(color.ToVector4());

            Model model = gizmoMode == GizmoModes.Translation
                ? _assets.EditorArrow
                : _assets.EditorArrowRound;


            ModelMeshPart meshpart = model.Meshes[0].MeshParts[0];

            Shaders.IdRenderEffectDrawId.Apply();

            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            //int vCount = meshpart.NumVertices;
            int startIndex = meshpart.StartIndex;

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
        }


        public RenderTarget2D GetOutlines()
        {
            return _idAndOutlineRenderer.GetRt();
        }

        /// <summary>
        /// Returns the id of the currently hovered object
        /// </summary>
        /// <returns></returns>
        public int GetHoveredId()
        {
            return _idAndOutlineRenderer.HoveredId;
        }
    }
}
