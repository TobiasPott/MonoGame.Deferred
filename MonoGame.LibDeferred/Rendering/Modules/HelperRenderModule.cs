﻿using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Renderer.RenderModules
{
    public class HelperRenderModule : IDisposable
    {
        private readonly HelperGeometryEffectSetup _effectSetup = new HelperGeometryEffectSetup();

        private Matrix _viewProjection;
        public Matrix ViewProjection { set { _viewProjection = value; } }


        private GraphicsDevice _graphicsDevice;

        public HelperRenderModule()
        { }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Draw()
        {
            HelperGeometryManager.GetInstance().Draw(_graphicsDevice, _viewProjection, _effectSetup);
        }

        public void Dispose()
        {
            _effectSetup?.Dispose();
        }
    }
}