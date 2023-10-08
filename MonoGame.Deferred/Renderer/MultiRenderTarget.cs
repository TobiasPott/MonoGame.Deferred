using Microsoft.Xna.Framework.Graphics;
using System;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public abstract class MRTBase : IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected int _width;
        protected int _height;
        protected readonly RenderTargetBinding[] _bindings;
        protected readonly RenderTarget2D[] _renderTargets;

        public RenderTargetBinding[] Bindings => _bindings;
        public RenderTarget2D[] RenderTargets => _renderTargets;

        public MRTBase(GraphicsDevice graphicsDevice, int width, int height, int numberOfTargets)
        {
            _graphicsDevice = graphicsDevice;
            _width = width;
            _height = height;
            _bindings = new RenderTargetBinding[numberOfTargets];
            _renderTargets = new RenderTarget2D[numberOfTargets];
        }

        public abstract void Resize(int width, int height);

        public virtual void Dispose()
        {
            foreach (RenderTarget2D rt in _renderTargets)
            {
                rt?.Dispose();
            }
        }
    }
    public class MultiRenderTarget : IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        private int _width;
        private int _height;
        private readonly RenderTargetBinding[] _renderTargetBinding = new RenderTargetBinding[3];

        private RenderTarget2D _renderTargetAlbedo;
        private RenderTarget2D _renderTargetDepth;
        private RenderTarget2D _renderTargetNormal;


        public RenderTarget2D Albedo => _renderTargetAlbedo;
        public RenderTarget2D Depth => _renderTargetDepth;
        public RenderTarget2D Normal => _renderTargetNormal;
        public RenderTargetBinding[] Bindings => _renderTargetBinding;

        public MultiRenderTarget(GraphicsDevice graphicsDevice, int targetWidth, int targetHeight)
        {
            _graphicsDevice = graphicsDevice;
            _width = targetWidth;
            _height = targetHeight;

            Resize(targetWidth, targetHeight);
        }

        public void Resize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                _width = width;
                _height = height;
                _renderTargetAlbedo?.Dispose();
                _renderTargetDepth?.Dispose();
                _renderTargetNormal?.Dispose();

                _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, _width, _height,
                    false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetNormal = new RenderTarget2D(_graphicsDevice, _width, _height,
                    false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                _renderTargetDepth = new RenderTarget2D(_graphicsDevice, _width, _height,
                    false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
                _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
                _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            }
        }

        public void Dispose()
        {
            _renderTargetAlbedo?.Dispose();
            _renderTargetDepth?.Dispose();
            _renderTargetNormal?.Dispose();
        }
    }

}

