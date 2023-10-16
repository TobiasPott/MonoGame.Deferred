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
        public bool HasMouseMovement { get; protected set; }
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
                HasMouseMovement = false;
                return;
            }
            if (Input.mouseState != Input.mouseLastState)
            {
                //reset the timer!
                _mouseMoved = gameTime.TotalGameTime.TotalMilliseconds + mouseMoveTimer;
                HasMouseMovement = true;
            }
            if (_mouseMoved < gameTime.TotalGameTime.TotalMilliseconds)
            {
                HasMouseMovement = false;
            }

        }

        public void DrawEditor(EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            BillboardRenderer.DrawEditorBillboards(scene, matrices, gizmoContext);
            IdAndOutlineRenderer.DrawTransformGizmos(matrices, gizmoContext, IdAndOutlineRenderer.Pass.Color);
        }

    }
}
