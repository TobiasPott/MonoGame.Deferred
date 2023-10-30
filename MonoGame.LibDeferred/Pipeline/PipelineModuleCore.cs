﻿using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline
{
    public abstract class PipelineModuleCore : IDisposable
    {

        protected GraphicsDevice _graphicsDevice;
        protected SpriteBatch _spriteBatch;

        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        public abstract void Dispose();

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
