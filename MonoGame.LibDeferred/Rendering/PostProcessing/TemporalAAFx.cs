using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Renderer.PostProcessing
{
    //Just a template
    public class TemporalAAFx : BaseFx
    {

        private bool _enabled = true;
        private bool _useTonemapping = true;
        private HaltonSequence _haltonSequence = new HaltonSequence();


        public bool Enabled { get => _enabled && RenderingSettings.TAA.Enabled; set { _enabled = value; } }
        public PipelineMatrices Matrices { get; set; }
        public bool IsOffFrame { get; protected set; } = true;
        public int JitterMode = 2;

        public Vector3[] FrustumCorners { set { Shaders.TAA.Param_FrustumCorners.SetValue(value); } }
        public Vector2 Resolution { set { Shaders.TAA.Param_Resolution.SetValue(value); } }
        public RenderTarget2D DepthMap { set { Shaders.TAA.Param_DepthMap.SetValue(value); } }

        public bool UseTonemap
        {
            get { return _useTonemapping && RenderingSettings.TAA.UseTonemapping; }
            set { _useTonemapping = value; Shaders.TAA.Param_UseTonemap.SetValue(value); }
        }


        public TemporalAAFx()
        { }

        public void SwapOffFrame()
        { IsOffFrame = !IsOffFrame; }
        public void Draw(RenderTarget2D currentFrame, RenderTarget2D previousFrames, RenderTarget2D output)
        {
            _graphicsDevice.SetRenderTarget(output);
            _graphicsDevice.BlendState = BlendState.Opaque;

            Shaders.TAA.Param_AccumulationMap.SetValue(previousFrames);
            Shaders.TAA.Param_UpdateMap.SetValue(currentFrame);
            Shaders.TAA.Param_CurrentToPrevious.SetValue(Matrices.CurrentViewToPreviousViewProjection);

            this.Draw(Shaders.TAA.Pass_TemporalAA);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(currentFrame);
                Shaders.TAA.Param_UpdateMap.SetValue(output);
                this.Draw(Shaders.TAA.Pass_TonemapInverse);
            }
        }

        public bool UpdateViewProjection(PipelineMatrices matrices)
        {
            switch (this.JitterMode)
            {
                case 0: //2 frames, just basic translation. Worst taa implementation. Not good with the continous integration used
                    {
                        Vector2 translation = Vector2.One * (this.IsOffFrame ? 0.5f : -0.5f);
                        matrices.ViewProjection *= (translation / RenderingSettings.g_ScreenResolution).ToMatrixTranslationXY();
                        return true;
                    }
                case 1: // Just random translation
                    {
                        float randomAngle = FastRand.NextAngle();
                        Vector2 translation = (new Vector2((float)Math.Sin(randomAngle), (float)Math.Cos(randomAngle)) / RenderingSettings.g_ScreenResolution) * 0.5f;
                        matrices.ViewProjection *= translation.ToMatrixTranslationXY();
                        return true;
                    }
                case 2: // Halton sequence, default
                    {
                        Vector3 translation = _haltonSequence.GetHaltonSequence();
                        matrices.ViewProjection *= Matrix.CreateTranslation(translation);
                        return true;
                    }
            }
            return false;
        }

        public override void Dispose()
        {
        }
    }
}

namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {
        public static class TAA
        {

            public static Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/TemporalAntiAliasing/TemporalAntiAliasing");

            public static EffectPass Pass_TemporalAA = Effect.Techniques["TemporalAntialiasing"].Passes[0];
            public static EffectPass Pass_TonemapInverse = Effect.Techniques["InverseTonemap"].Passes[0];

            public static EffectParameter Param_AccumulationMap = Effect.Parameters["AccumulationMap"];
            public static EffectParameter Param_UpdateMap = Effect.Parameters["UpdateMap"];
            public static EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static EffectParameter Param_CurrentToPrevious = Effect.Parameters["CurrentToPrevious"];
            public static EffectParameter Param_Resolution = Effect.Parameters["Resolution"];
            public static EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            public static EffectParameter Param_UseTonemap = Effect.Parameters["UseTonemap"];

        }
    }
}