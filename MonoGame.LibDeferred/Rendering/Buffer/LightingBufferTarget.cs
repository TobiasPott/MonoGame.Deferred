using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    public class LightingBufferTarget : MultiRenderTargetBase
    {
        public const int DIFFUSE = 0;
        public const int SPECULAR = 1;
        public const int VOLUME = 2;

        private static readonly RenderTarget2DDefinition[] Definitions = new RenderTarget2DDefinition[3] {
                            new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents),
                            new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents),
                            new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents)
        };


        public RenderTarget2D Diffuse => _renderTargets[DIFFUSE];
        public RenderTarget2D Specular => _renderTargets[SPECULAR];
        public RenderTarget2D Volume => _renderTargets[VOLUME];

        public LightingBufferTarget(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height, 3)
        { }

        public override void Resize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                base.Resize(width, height);
                _renderTargets[DIFFUSE] = Definitions[DIFFUSE].CreateRenderTarget(_graphicsDevice, _width, _height);
                _renderTargets[SPECULAR] = Definitions[SPECULAR].CreateRenderTarget(_graphicsDevice, _width, _height);
                _renderTargets[VOLUME] = Definitions[VOLUME].CreateRenderTarget(_graphicsDevice, _width, _height);

                _bindings[DIFFUSE] = new RenderTargetBinding(_renderTargets[DIFFUSE]);
                _bindings[SPECULAR] = new RenderTargetBinding(_renderTargets[SPECULAR]);
                _bindings[VOLUME] = new RenderTargetBinding(_renderTargets[VOLUME]);
            }
        }

    }

}

