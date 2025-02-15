﻿using DeferredEngine.Pipeline;
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
        public readonly TemporalAAFx TemporalAA;
        protected readonly ColorGradingFx ColorGrading;
        protected readonly PostProcessingFx PostProcessing;
        public readonly SSReflectionFx SSReflection;
        public readonly SSAmbientOcclustionFx SSAmbientOcclusion;

        //GaussianBlurFx _gaussianBlur;

        private FullscreenTriangleBuffer _fullscreenTarget;

        public GBufferTarget GBufferTarget
        {
            set
            {
                SSReflection.NormalMap = value.Normal;
                SSReflection.DepthMap = value.Depth;

                TemporalAA.DepthMap = value.Depth;

                SSAmbientOcclusion.NormalMap = value.Normal;
                SSAmbientOcclusion.DepthMap = value.Depth;
            }
        }

        public SSFxTargets SSFxTargets
        {
            set
            {
                SSAmbientOcclusion.SSFxTargets = value;
                SSReflection.SSFxTargets = value;
                TemporalAA.SSFxTargets = value;
            }
        }

        public Vector2 Resolution
        {
            set
            {
                TemporalAA.Resolution = value;
                SSReflection.Resolution = value;
                // HALF RESOLUTION
                value /= 2;
                SSAmbientOcclusion.Resolution = value;
            }
        }

        public PipelineProfiler Profiler
        { set { foreach (PostFx module in _modules) module.Profiler = value; } }
        public PipelineMatrices Matrices
        { set { foreach (PostFx module in _modules) module.Matrices = value; } }
        public PipelineFrustum Frustum
        { set { foreach (PostFx module in _modules) module.Frustum = value; } }


        private readonly List<PostFx> _modules = new List<PostFx>();


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
            return this.ColorGrading.Draw(sourceRT, previousRT, destRT);
        }
        private RenderTarget2D DrawBloom(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            return this.Bloom.Draw(sourceRT, previousRT, destRT);

        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D DrawTemporalAA(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            return this.TemporalAA.Draw(sourceRT, previousRT, destRT);
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D DrawSSReflection(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            // optional: use TemporalAA buffers as sourceRT
            sourceRT ??= this.SSReflection.GetSSReflectionRenderTargets(TemporalAA);

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

