using DeferredEngine.Pipeline;
using Microsoft.Xna.Framework;
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


        protected Vector2 _resolution = Vector2.One;
        protected Vector2 _inverseResolution = Vector2.One;
        protected Vector2 _aspectRatios = Vector2.One;
        protected FullscreenTriangleBuffer _fullscreenTarget;

        public Vector2 Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                _inverseResolution = Vector2.One / value;
                _aspectRatios = new Vector2(Math.Min(1.0f, value.X / value.Y), Math.Min(1.0f, value.Y / value.X));
            }
        }


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
