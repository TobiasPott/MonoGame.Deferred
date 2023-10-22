using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    public class GBufferTarget : MultiRenderTargetBase
    {
        public const int ALBEDO = 0;
        public const int NORMAL = 1;
        public const int DEPTH = 2;


        private static readonly RenderTarget2DDefinition[] Definitions = new RenderTarget2DDefinition[3] {
                            new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents),
                            new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents),
                            new RenderTarget2DDefinition(false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents)
        };


        public RenderTarget2D Albedo => _renderTargets[ALBEDO];
        public RenderTarget2D Normal => _renderTargets[NORMAL];
        public RenderTarget2D Depth => _renderTargets[DEPTH];

        public GBufferTarget(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height, Definitions.Length)
        { }

        public override void Resize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                base.Resize(width, height);
                _renderTargets[ALBEDO]?.Dispose();
                _renderTargets[NORMAL]?.Dispose();
                _renderTargets[DEPTH]?.Dispose();

                _renderTargets[ALBEDO] = Definitions[ALBEDO].CreateRenderTarget(_graphicsDevice, _width, _height);
                _renderTargets[NORMAL] = Definitions[NORMAL].CreateRenderTarget(_graphicsDevice, _width, _height);
                _renderTargets[DEPTH] = Definitions[DEPTH].CreateRenderTarget(_graphicsDevice, _width, _height);

                _bindings[ALBEDO] = new RenderTargetBinding(_renderTargets[ALBEDO]);
                _bindings[NORMAL] = new RenderTargetBinding(_renderTargets[NORMAL]);
                _bindings[DEPTH] = new RenderTargetBinding(_renderTargets[DEPTH]);

            }
        }

    }

}

