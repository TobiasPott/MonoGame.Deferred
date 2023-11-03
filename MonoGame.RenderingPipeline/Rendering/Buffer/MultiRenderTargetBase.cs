using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
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
        public MultiRenderTargetBase(GraphicsDevice graphicsDevice, int width, int height, RenderTarget2DDefinition[] definitions)
        {
            _graphicsDevice = graphicsDevice;
            _bindings = new RenderTargetBinding[definitions.Length];
            _renderTargets = new RenderTarget2D[definitions.Length];

            for (int i = 0; i < definitions.Length; i++)
            {
                _renderTargets[i]?.Dispose();
                _renderTargets[i] = definitions[i].CreateRenderTarget(_graphicsDevice, width, height);
                _bindings[i] = new RenderTargetBinding(_renderTargets[i]);
            }

            this.Resize(width, height);
        }

        public void Resize(Vector2 resolution) => this.Resize((int)resolution.X, (int)resolution.Y);
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

