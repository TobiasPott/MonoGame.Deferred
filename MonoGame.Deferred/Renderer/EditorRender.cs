using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.Editor;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using DeferredEngine.Renderer.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DeferredEngine.Renderer.RenderModules
{
    public class EditorRender
    {
        private IdAndOutlineRenderer _idAndOutlineRenderer;
        private GraphicsDevice _graphicsDevice;

        private BillboardBuffer _billboardBuffer;


        private double _mouseMoved;
        private bool _mouseMovement;
        private readonly double mouseMoveTimer = 400;

        public void Initialize(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;

            _billboardBuffer = new BillboardBuffer(Color.White, graphics);
            _idAndOutlineRenderer = new IdAndOutlineRenderer();
            _idAndOutlineRenderer.Initialize(graphics, _billboardBuffer);

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
        public RenderTarget2D GetOutlines()
        {
            return _idAndOutlineRenderer.GetRenderTarget2D();
        }


        public void DrawBillboards(EntitySceneGroup scene, EnvironmentProbe envSample, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            List<Decal> decals = scene.Decals;
            List<DeferredPointLight> pointLights = scene.PointLights;
            List<DeferredDirectionalLight> dirLights = scene.DirectionalLights;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            Shaders.Billboard.Effect.CurrentTechnique = Shaders.Billboard.Technique_Billboard;

            Shaders.Billboard.Param_IdColor.SetValue(Color.Gray.ToVector3());

            //Decals
            Shaders.Billboard.Param_Texture.SetValue(StaticAssets.Instance.IconDecal);
            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawBillboard(decal, staticViewProjection, view, gizmoContext);
            }

            //Lights
            Shaders.Billboard.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            for (int index = 0; index < pointLights.Count; index++)
            {
                var light = pointLights[index];
                DrawBillboard(light, staticViewProjection, view, gizmoContext);
            }

            HelperGeometryManager helperManager = HelperGeometryManager.GetInstance();
            //DirectionalLights
            for (var index = 0; index < dirLights.Count; index++)
            {
                DeferredDirectionalLight light = dirLights[index];
                DrawBillboard(light, staticViewProjection, view, gizmoContext);

                Vector3 lPosition = light.Position;
                Vector3 lDirection = light.Direction * 10;
                Color lColor = light.Color;
                helperManager.AddLineStartDir(lPosition, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition + Vector3.UnitX * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition - Vector3.UnitX * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition + Vector3.UnitY * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition - Vector3.UnitY * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition + Vector3.UnitZ * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition - Vector3.UnitZ * 10, lDirection, 1, Color.Black, lColor);

                if (light.CastShadows)
                {
                    BoundingFrustum boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);
                    helperManager.CreateBoundingBoxLines(boundingFrustumShadow);
                }
            }

            //EnvMap

            Shaders.Billboard.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);

            DrawBillboard(envSample, staticViewProjection, view, gizmoContext);

        }
        private void DrawBillboard(TransformableObject billboardObject, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            Matrix world = Matrix.CreateTranslation(billboardObject.Position);
            Shaders.Billboard.Param_WorldViewProj.SetValue(world * staticViewProjection);
            Shaders.Billboard.Param_WorldView.SetValue(world * view);

            if (billboardObject.Id == GetHoveredId())
                Shaders.Billboard.Param_IdColor.SetValue(Color.White.ToVector3());
            if (billboardObject.Id == gizmoContext.SelectedObjectId)
                Shaders.Billboard.Param_IdColor.SetValue(Color.Gold.ToVector3());

            Shaders.Billboard.Effect.CurrentTechnique.Passes[0].Apply();

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            if (billboardObject.Id == GetHoveredId() || billboardObject.Id == gizmoContext.SelectedObjectId)
                Shaders.Billboard.Param_IdColor.SetValue(Color.Gray.ToVector3());
        }

        public void DrawIds(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, EnvironmentProbe envSample, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            _idAndOutlineRenderer.Draw(meshBatcher, scene, envSample, matrices, gizmoContext, _mouseMovement);
        }

        public void DrawEditorElements(EntitySceneGroup scene, EnvironmentProbe envSample, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            DrawGizmo(matrices, gizmoContext);
            DrawBillboards(scene, envSample, matrices.StaticViewProjection, matrices.View, gizmoContext);
        }

        protected void DrawGizmo(PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            if (gizmoContext.SelectedObjectId == 0) return;

            Vector3 position = gizmoContext.SelectedObjectPosition;
            GizmoModes gizmoMode = gizmoContext.GizmoMode;
            Matrix rotation = (RenderingStats.e_LocalTransformation || gizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            //Z
            DrawArrow(position, rotation, 0, 0, 0, GetHoveredId() == 1 ? 1 : 0.5f, Color.Blue, matrices.StaticViewProjection, gizmoMode); //z 1
            DrawArrow(position, rotation, -Math.PI / 2, 0, 0, GetHoveredId() == 2 ? 1 : 0.5f, Color.Green, matrices.StaticViewProjection, gizmoMode); //y 2
            DrawArrow(position, rotation, 0, Math.PI / 2, 0, GetHoveredId() == 3 ? 1 : 0.5f, Color.Red, matrices.StaticViewProjection, gizmoMode); //x 3

            DrawArrow(position, rotation, Math.PI, 0, 0, GetHoveredId() == 1 ? 1 : 0.5f, Color.Blue, matrices.StaticViewProjection, gizmoMode); //z 1
            DrawArrow(position, rotation, Math.PI / 2, 0, 0, GetHoveredId() == 2 ? 1 : 0.5f, Color.Green, matrices.StaticViewProjection, gizmoMode); //y 2
            DrawArrow(position, rotation, 0, -Math.PI / 2, 0, GetHoveredId() == 3 ? 1 : 0.5f, Color.Red, matrices.StaticViewProjection, gizmoMode); //x 3
            //DrawArrowRound(position, rotation, Math.PI, 0, 0, GetHoveredId() == 1 ? 1 : 0.5f, Color.Blue, staticViewProjection); //z 1
            //DrawArrowRound(position, rotation,-Math.PI / 2, 0, 0, GetHoveredId() == 2 ? 1 : 0.5f, Color.Green, staticViewProjection); //y 2
            //DrawArrowRound(position, rotation,0, Math.PI / 2, 0, GetHoveredId() == 3 ? 1 : 0.5f, Color.Red, staticViewProjection); //x 3
        }

        private void DrawArrow(Vector3 position, Matrix rotationObject, double angleX, double angleY, double angleZ, float scale, Color color, Matrix staticViewProjection, GizmoModes gizmoMode, Vector3? direction = null)
        {
            Matrix rotation;
            if (direction != null)
                rotation = Matrix.CreateLookAt(Vector3.Zero, (Vector3)direction, Vector3.UnitX);
            else
                rotation = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) * Matrix.CreateRotationZ((float)angleZ);

            Matrix scaleMatrix = Matrix.CreateScale(0.75f, 0.75f, scale * 1.5f);
            Matrix worldViewProj = scaleMatrix * rotation * rotationObject * Matrix.CreateTranslation(position) * staticViewProjection;

            Shaders.IdRender.Param_WorldViewProj.SetValue(worldViewProj);
            Shaders.IdRender.Param_ColorId.SetValue(color.ToVector4());

            Model model = gizmoMode == GizmoModes.Translation ? StaticAssets.Instance.EditorArrow3D : StaticAssets.Instance.EditorArrow3DRound;
            ModelMeshPart meshpart = model.Meshes[0].MeshParts[0];

            Shaders.IdRender.Technique_Id.Apply();

            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            //int vCount = meshpart.NumVertices;
            int startIndex = meshpart.StartIndex;

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
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
