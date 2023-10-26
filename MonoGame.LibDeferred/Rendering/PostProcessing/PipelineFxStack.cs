using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public enum PipelineFxStage
    {
        Bloom,
        TemporalAA,
        PostProcessing,
        ColorGrading,
        SSReflection,
        SSAmbientOcclusion
    }

    public class PipelineFxStack : IDisposable
    {
        protected readonly BloomFx Bloom;
        public readonly TemporalAAFx TemporaAA;
        protected readonly ColorGradingFx ColorGrading;
        protected readonly PostProcessingFx PostProcessing;
        public readonly SSReflectionFx SSReflection;
        public readonly SSAmbientOcclustionFx SSAmbientOcclusion;

        //GaussianBlurFx _gaussianBlur;

        private float _superSampling = 1;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private FullscreenTriangleBuffer _fullscreenTarget;

        public SSFxTargets SSFxTargets
        {
            set
            {
                SSAmbientOcclusion.SSFxTargets = value;
            }
        }
        public float FarClip
        {
            set
            {
                SSReflection.FarClip = value;

            }
        }
        public PipelineMatrices Matrices
        {
            set
            {
                TemporaAA.Matrices = value;
                SSReflection.Matrices = value;
                SSAmbientOcclusion.Matrices = value;
            }
        }

        public Vector3[] FrustumCorners
        {
            set
            {
                SSAmbientOcclusion.FrustumCorners = value;
                TemporaAA.FrustumCorners = value;
                SSReflection.FrustumCorners = value;
            }
        }



        public PipelineFxStack(ContentManager content)
        {
            Bloom = new BloomFx(content);
            TemporaAA = new TemporalAAFx();
            ColorGrading = new ColorGradingFx(content);
            PostProcessing = new PostProcessingFx(content);
            SSReflection = new SSReflectionFx(content);
            SSAmbientOcclusion = new SSAmbientOcclustionFx(content);
            //_gaussianBlur = new GaussianBlurFx();

        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

            Bloom.Initialize(graphicsDevice, RenderingSettings.Screen.g_Resolution);
            TemporaAA.Initialize(graphicsDevice, _fullscreenTarget);
            ColorGrading.Initialize(graphicsDevice, _fullscreenTarget);
            PostProcessing.Initialize(graphicsDevice, _fullscreenTarget);
            SSReflection.Initialize(graphicsDevice, _fullscreenTarget);
            SSAmbientOcclusion.Initialize(graphicsDevice, spriteBatch, _fullscreenTarget);
        }

        public RenderTarget2D Draw(PipelineFxStage stage, RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            return stage switch
            {
                PipelineFxStage.Bloom => DrawBloom(sourceRT, previousRT, destRT),
                PipelineFxStage.TemporalAA => DrawTemporalAA(sourceRT, previousRT, destRT),
                PipelineFxStage.PostProcessing => DrawPostProcessing(sourceRT, previousRT, destRT),
                PipelineFxStage.ColorGrading => DrawColorGrading(sourceRT, previousRT, destRT),
                PipelineFxStage.SSReflection => DrawSSReflection(sourceRT, previousRT, destRT),
                PipelineFxStage.SSAmbientOcclusion => DrawSSAmbientOcclusion(sourceRT, previousRT, destRT),
                _ => sourceRT,
            }; ;
        }

        private RenderTarget2D DrawPostProcessing(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.PostProcessing.Enabled)
                return sourceRT;

            return this.PostProcessing.Draw(sourceRT, previousRT, destRT);
        }
        private RenderTarget2D DrawColorGrading(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (this.ColorGrading.Enabled)
                sourceRT = this.ColorGrading.Draw(sourceRT, null, null);

            DrawTextureToScreenToFullScreen(sourceRT);

            return sourceRT;
        }
        private RenderTarget2D DrawBloom(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (this.Bloom.Enabled)
            {
                Texture2D bloom = this.Bloom.Draw(sourceRT, null, null);

                _graphicsDevice.SetRenderTargets(destRT);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                _spriteBatch.Draw(sourceRT, RenderingSettings.Screen.g_Rect, Color.White);
                _spriteBatch.Draw(bloom, RenderingSettings.Screen.g_Rect, Color.White);

                _spriteBatch.End();

                return destRT;
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

            return TemporalAAFx.g_UseTonemapping ? sourceRT : destRT;
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D DrawSSReflection(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            if (!this.SSReflection.Enabled)
                return sourceRT;
            return this.SSReflection.Draw(sourceRT, previousRT, destRT);
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D DrawSSAmbientOcclusion(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            if (!this.SSAmbientOcclusion.Enabled)
                return sourceRT;
            return this.SSAmbientOcclusion.Draw(sourceRT, previousRT, destRT);
        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            SSReflection.NormalMap = gBufferTarget.Normal;
            SSReflection.DepthMap = gBufferTarget.Depth;

            TemporaAA.DepthMap = gBufferTarget.Depth;

            SSAmbientOcclusion.NormalMap = gBufferTarget.Normal;
            SSAmbientOcclusion.DepthMap = gBufferTarget.Depth;
        }

        private void DrawTextureToScreenToFullScreen(Texture2D source, BlendState blendState = null, RenderTarget2D destRT = null)
        {
            if (blendState == null) blendState = BlendState.Opaque;

            RenderingSettings.Screen.GetDestinationRectangle(source.GetAspect(), out Rectangle destRectangle);
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
            PostProcessing?.Dispose();
            SSReflection?.Dispose();
            SSAmbientOcclusion?.Dispose();
            //_gaussianBlur?.Dispose();

        }

    }

}

