using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{

    public abstract class RenderingPipelineModule :IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected SpriteBatch _spriteBatch;

        public RenderingPipelineModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
        }
        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        protected abstract void Load(ContentManager content, string shaderPath);
        public abstract void Dispose();

    }

}
