using DeferredEngine.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.PostProcessing
{
    public abstract class PostFx : PipelineModuleCore
    {
        protected bool _enabled = true;
        public bool Enabled { get => GetEnabled(); set { _enabled = value; } }
        /// <summary>
        /// returns the final enabled state of this instance (may include global or context flags)
        /// </summary>
        /// <returns></returns>
        protected virtual bool GetEnabled() => _enabled;


        protected FullscreenTriangleBuffer _fullscreenTarget;


        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FullscreenTriangleBuffer fullscreenTarget)
        {
            base.Initialize(graphicsDevice, spriteBatch);
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

    }
}
