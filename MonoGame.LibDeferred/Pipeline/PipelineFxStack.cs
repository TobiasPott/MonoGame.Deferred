using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System.Reflection;

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
        public readonly TemporalAAFx TemporalAA;
        protected readonly ColorGradingFx ColorGrading;
        protected readonly PostProcessingFx PostProcessing;
        public readonly SSReflectionFx SSReflection;
        public readonly SSAmbientOcclustionFx SSAmbientOcclusion;

        //GaussianBlurFx _gaussianBlur;

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

        public Vector2 Resolution
        {
            set
            {
                TemporalAA.Resolution = value;
                SSReflection.Resolution = value;
                ///////////////////
                // HALF RESOLUTION
                value /= 2;
                SSAmbientOcclusion.InverseResolution = Vector2.One / value;
                SSAmbientOcclusion.AspectRatios = new Vector2(Math.Min(1.0f, value.X / value.Y), Math.Min(1.0f, value.Y / value.X));
            }
        }

        public PipelineMatrices Matrices
        { set { foreach (PostFx module in _modules) module.Matrices = value; } }
        public PipelineFrustum Frustum
        { set { foreach (PostFx module in _modules) module.Frustum = value; } }


        private List<PostFx> _modules = new List<PostFx>();


        public PipelineFxStack(ContentManager content)
        {
            Bloom = new BloomFx();
            TemporalAA = new TemporalAAFx();
            ColorGrading = new ColorGradingFx();
            ColorGrading.LookUpTable = content.Load<Texture2D>("Shaders/PostProcessing/lut");
            PostProcessing = new PostProcessingFx();
            SSReflection = new SSReflectionFx();
            SSAmbientOcclusion = new SSAmbientOcclustionFx();

            _modules.AddRange(new PostFx[] {
                Bloom, TemporalAA, ColorGrading, PostProcessing, SSReflection, SSAmbientOcclusion
            });
        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

            Bloom.Initialize(graphicsDevice, spriteBatch, _fullscreenTarget);
            TemporalAA.Initialize(graphicsDevice, spriteBatch, _fullscreenTarget);
            ColorGrading.Initialize(graphicsDevice, spriteBatch, _fullscreenTarget);
            PostProcessing.Initialize(graphicsDevice, spriteBatch, _fullscreenTarget);
            SSReflection.Initialize(graphicsDevice, spriteBatch, _fullscreenTarget);
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
            if (!this.TemporalAA.Enabled)
                return sourceRT;
            this.TemporalAA.Draw(sourceRT, previousRT, destRT);

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

            TemporalAA.DepthMap = gBufferTarget.Depth;

            SSAmbientOcclusion.NormalMap = gBufferTarget.Normal;
            SSAmbientOcclusion.DepthMap = gBufferTarget.Depth;
        }

        private void DrawTextureToScreenToFullScreen(Texture2D source, BlendState blendState = null, RenderTarget2D destRT = null)
        {
            if (blendState == null) blendState = BlendState.Opaque;

            RenderingSettings.Screen.GetDestinationRectangle(source.GetAspect(), out Rectangle destRectangle);
            _graphicsDevice.SetRenderTarget(destRT);
            _spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp);
            _spriteBatch.Draw(source, destRectangle, Color.White);
            _spriteBatch.End();
        }



        public void Dispose()
        {
            Bloom?.Dispose();
            TemporalAA?.Dispose();
            ColorGrading?.Dispose();
            PostProcessing?.Dispose();
            SSReflection?.Dispose();
            SSAmbientOcclusion?.Dispose();
            //_gaussianBlur?.Dispose();

        }

    }

}

