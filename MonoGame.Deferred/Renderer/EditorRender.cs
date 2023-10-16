using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.Editor;
using DeferredEngine.Renderer.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeferredEngine.Renderer.RenderModules
{
    public class EditorRender
    {
        private GraphicsDevice _graphicsDevice;

        public IdAndOutlineRenderer IdAndOutlineRenderer { get; protected set; }
        public BillboardRenderer BillboardRenderer { get; protected set; }


        private double _mouseMoved;
        private bool _mouseMovement;
        private readonly double mouseMoveTimer = 400;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;


            BillboardRenderer = new BillboardRenderer();
            BillboardRenderer.Initialize(graphicsDevice);
            IdAndOutlineRenderer = new IdAndOutlineRenderer();
            IdAndOutlineRenderer.Initialize(graphicsDevice);

            BillboardRenderer.IdAndOutlineRenderer = IdAndOutlineRenderer;
            IdAndOutlineRenderer.BillboardRenderer = BillboardRenderer;

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

        public RenderTarget2D GetOutlinesRenderTarget()
        {
            return IdAndOutlineRenderer.GetRenderTarget2D();
        }


        public void DrawIds(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, EnvironmentProbe envSample, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            IdAndOutlineRenderer.Draw(meshBatcher, scene, envSample, matrices, gizmoContext, _mouseMovement);
        }

        public void DrawEditorElements(EntitySceneGroup scene, EnvironmentProbe envSample, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            IdAndOutlineRenderer.DrawGizmos(matrices, gizmoContext);
            BillboardRenderer.DrawEditorBillboards(scene, envSample, matrices.StaticViewProjection, matrices.View, gizmoContext);
        }

    }
}
