using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer
{
    public abstract class MultiRenderTargetBase : IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected int _width;
        protected int _height;
        protected readonly RenderTargetBinding[] _bindings;
        protected readonly RenderTarget2D[] _renderTargets;

        public RenderTargetBinding[] Bindings => _bindings;
        public RenderTarget2D[] RenderTargets => _renderTargets;

        public RenderTarget2D this[int index] => _renderTargets[index];

        public MultiRenderTargetBase(GraphicsDevice graphicsDevice, int width, int height, int numberOfTargets)
        {
            _graphicsDevice = graphicsDevice;
            _bindings = new RenderTargetBinding[numberOfTargets];
            _renderTargets = new RenderTarget2D[numberOfTargets];
            this.Resize(width, height);
        }

        public virtual void Resize(int width, int height)
        {
            _width = width;
            _height = height;
        }


        public virtual void Dispose()
        {
            foreach (RenderTarget2D rt in _renderTargets)
            {
                rt?.Dispose();
            }
        }
    }

}

