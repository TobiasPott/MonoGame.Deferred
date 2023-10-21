using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.PostProcessing
{
    public abstract class BaseFx : IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected FullscreenTriangleBuffer _fullscreenTarget;

        
        public virtual void Initialize(GraphicsDevice graphicsDevice, FullscreenTriangleBuffer fullscreenTarget)
        {
            _graphicsDevice = graphicsDevice;
            _fullscreenTarget = fullscreenTarget;
        }
        protected void Draw(EffectPass pass)
        {
            pass?.Apply();
            this.Draw();
        }
        protected void Draw()
        { _fullscreenTarget.Draw(_graphicsDevice); }

        public abstract RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null);
        public abstract void Dispose();

    }
}
