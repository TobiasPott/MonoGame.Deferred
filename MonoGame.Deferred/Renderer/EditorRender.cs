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
using MonoGame.Ext;
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

            _idAndOutlineRenderer.DrawGizmos(matrices, gizmoContext);
            DrawBillboards(scene, envSample, matrices.StaticViewProjection, matrices.View, gizmoContext);
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
