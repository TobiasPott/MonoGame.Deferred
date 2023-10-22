using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using Windows.Web.Syndication;

namespace DeferredEngine.Rendering.PostProcessing
{

    public enum PipelineFxStage
    {
        Bloom,
        TemporalAA,
        PostProcessing,
        ColorGrading
    }

    public class PipelineFxStack : IDisposable
    {
        protected readonly BloomFx Bloom;
        public readonly TemporalAAFx TemporaAA;
        protected readonly ColorGradingFx ColorGrading;

        //GaussianBlurFx _gaussianBlur;

        private float _superSampling = 1;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private FullscreenTriangleBuffer _fullscreenTarget;


        public PipelineFxStack(ContentManager content)
        {
            Bloom = new BloomFx(content);
            TemporaAA = new TemporalAAFx();
            ColorGrading = new ColorGradingFx(content);
            //_gaussianBlur = new GaussianBlurFx();
        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

            Bloom.Initialize(graphicsDevice, RenderingSettings.g_ScreenResolution);
            TemporaAA.Initialize(graphicsDevice, _fullscreenTarget);
            ColorGrading.Initialize(graphicsDevice, _fullscreenTarget);

        }

        public void SetPipelineMatrices(PipelineMatrices matrices)
        {
            TemporaAA.Matrices = matrices;
        }

        public RenderTarget2D Draw(PipelineFxStage stage, RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            return stage switch
            {
                PipelineFxStage.Bloom => DrawBloom(sourceRT, previousRT, destRT),
                PipelineFxStage.TemporalAA => DrawTemporalAA(sourceRT, previousRT, destRT),
                PipelineFxStage.PostProcessing => DrawPostProcessing(sourceRT, previousRT, destRT),
                PipelineFxStage.ColorGrading => DrawColorGrading(sourceRT, previousRT, destRT),
                _ => sourceRT,
            };
        }

        private RenderTarget2D DrawPostProcessing(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!RenderingSettings.g_PostProcessing) return sourceRT;

            Shaders.PostProcssing.Param_ScreenTexture.SetValue(sourceRT);
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);

            Shaders.PostProcssing.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            return destRT;
            // ToDo: determine why post processing and color grading cannot be called independent from each other?
        }
        private RenderTarget2D DrawColorGrading(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!RenderingSettings.g_PostProcessing) return sourceRT;

            if (this.ColorGrading.Enabled)
                destRT = this.ColorGrading.Draw(destRT);

            DrawTextureToScreenToFullScreen(destRT);
            
            return destRT;
        }
        private RenderTarget2D DrawBloom(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (this.Bloom.Enabled)
            {
                Texture2D bloom = this.Bloom.Draw(sourceRT, null, null);

                _graphicsDevice.SetRenderTargets(destRT); // _auxTargets[MRT.BLOOM]);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                _spriteBatch.Draw(sourceRT, RenderingSettings.g_ScreenRect, Color.White);
                _spriteBatch.Draw(bloom, RenderingSettings.g_ScreenRect, Color.White);

                _spriteBatch.End();

                return destRT; // _auxTargets[MRT.BLOOM];
            }
            else
            {
                return sourceRT;
            }
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D DrawTemporalAA(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            if (!this.TemporaAA.Enabled)
                return sourceRT;

            this.TemporaAA.Draw(sourceRT, previousRT, destRT);

            return RenderingSettings.TAA.UseTonemapping ? sourceRT : destRT;
        }


        private void DrawTextureToScreenToFullScreen(Texture2D source, BlendState blendState = null, RenderTarget2D destRT = null)
        {
            if (blendState == null) blendState = BlendState.Opaque;

            RenderingSettings.GetDestinationRectangle(source.GetAspect(), out Rectangle destRectangle);
            _graphicsDevice.SetRenderTarget(destRT);
            _spriteBatch.Begin(0, blendState, _superSampling > 1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
            _spriteBatch.Draw(source, destRectangle, Color.White);
            _spriteBatch.End();
        }



        public void Dispose()
        {
            Bloom?.Dispose();
            TemporaAA?.Dispose();
            ColorGrading?.Dispose();
            //_gaussianBlur?.Dispose();

        }

    }

}

