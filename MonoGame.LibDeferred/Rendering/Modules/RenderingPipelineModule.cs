using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.LibDeferred.Rendering.Modules
{

    public abstract class RenderingPipelineModule :IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected SpriteBatch _spriteBatch;

        public RenderingPipelineModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
        }
        protected abstract void Load(ContentManager content, string shaderPath);
        protected virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        public abstract void Dispose();

    }

}
