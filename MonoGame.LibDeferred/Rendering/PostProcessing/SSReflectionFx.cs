using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class SSReflectionFx : PostFx
    {
        // SSR
        public static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);
        public static bool g_FireflyReduction { get; set; } = true;



        public static float g_FireflyThreshold { get; set; } = 1.75f;


        public static NotifiedProperty<bool> g_Noise = new NotifiedProperty<bool>(true);
        public static NotifiedProperty<bool> g_UseTaa = new NotifiedProperty<bool>(true);

        //5 and 5 are good, 3 and 3 are cheap#
        public static NotifiedProperty<int> g_Samples = new NotifiedProperty<int>(3);
        public static NotifiedProperty<int> g_RefinementSamples = new NotifiedProperty<int>(3);



        private SSFxTargets _ssfxTargets;
        public SSFxTargets SSFxTargets { set { _ssfxTargets = value; } }


        private readonly SSReflectionFxSetup _fxSetup = new SSReflectionFxSetup();


        public float Time { set { _fxSetup.Param_Time.SetValue(SSReflectionFx.g_Noise ? value : 0.0f); } }

        public RenderTarget2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }
        public RenderTarget2D NormalMap { set { _fxSetup.Param_NormalMap.SetValue(value); } }
        public RenderTarget2D SourceMap { set { _fxSetup.Param_SourceMap.SetValue(value); } }



        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public SSReflectionFx()
        {
            g_Noise.Changed += Global_Noise_Changed;
        }

        private void Global_Noise_Changed(bool enabled)
        {
            Time = 0.0f;
        }

        protected override bool GetEnabled() => _enabled && SSReflectionFx.ModuleEnabled;
        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
                return sourceRT;

            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            _fxSetup.Param_SourceMap.SetValue(sourceRT);
            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
            _fxSetup.Param_Resolution.SetValue(_resolution);
            _fxSetup.Param_FrustumCorners.SetValue(this.Frustum.ViewSpaceFrustum);
            _fxSetup.Param_Projection.SetValue(this.Matrices.Projection);

            _fxSetup.Param_Samples.SetValue(SSReflectionFx.g_Samples);
            _fxSetup.Param_SecondarySamples.SetValue(SSReflectionFx.g_RefinementSamples);

            _fxSetup.Effect.CurrentTechnique = SSReflectionFx.g_UseTaa ? _fxSetup.Technique_Taa : _fxSetup.Technique_Default;
            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            // sample profiler if set
            this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_SSFx_SSR);
            return destRT;
        }
        public RenderTarget2D GetSSReflectionRenderTargets(TemporalAAFx taaFx)
        {
            if (_ssfxTargets != null && taaFx != null && taaFx.Enabled)
                return taaFx.IsOffFrame ? _ssfxTargets.TAA_Even : _ssfxTargets.TAA_Odd;
            else
                return null;
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }


    }
}
