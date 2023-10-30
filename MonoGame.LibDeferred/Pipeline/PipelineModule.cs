using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline
{

    public interface IPipelineModule : IDisposable
    {
        PipelineMatrices Matrices { get; set; }
        void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
    }

    public abstract class PipelineModule : IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected SpriteBatch _spriteBatch;


        public PipelineProfiler Profiler { get; set; }
        public PipelineMatrices Matrices { get; set; }
        public PipelineFrustum Frustum { get; set; }


        public PipelineModule()
        { }
        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        public abstract void Dispose();

        //public virtual void Draw(DynamicMeshBatcher meshBatcher) { }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public virtual RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D auxRT, RenderTarget2D destRT) => destRT;

        public void Blit(Texture2D source, RenderTarget2D destRT = null)
            => this.Blit(source, destRT, BlendState.Opaque);
        public void Blit(Texture2D source, RenderTarget2D destRT = null, BlendState blendState = null, SamplerState samplerState = null)
        {
            if (blendState == null)
                blendState = BlendState.Opaque;
            if (samplerState == null)
                samplerState = SamplerState.LinearWrap;

            RenderingSettings.Screen.GetDestinationRectangle(source.GetAspect(), out Rectangle destRectangle);
            _graphicsDevice.SetRenderTarget(destRT);
            _spriteBatch.Begin(0, blendState, samplerState);
            _spriteBatch.Draw(source, destRectangle, Color.White);
            _spriteBatch.End();
        }

        public void BlitCube(RenderTarget2D texture, RenderTargetCube target, CubeMapFace? face)
        {
            if (face != null)
                _graphicsDevice.SetRenderTarget(target, (CubeMapFace)face);

            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), Color.White);
            _spriteBatch.End();
        }

    }

}
