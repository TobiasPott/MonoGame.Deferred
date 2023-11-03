using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{
    //Just a template
    public class TemporalAAFx : PostFx
    {
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);
        public readonly static NotifiedProperty<bool> ModuleTonemapEnabled = new NotifiedProperty<bool>(true);



        private bool _useTonemapping = true;
        private readonly TemporalAAFxSetup _fxSetup = new TemporalAAFxSetup();
        private readonly HaltonSequence _haltonSequence = new HaltonSequence();


        public bool IsOffFrame { get; protected set; } = true;
        public int JitterMode = 2;


        public RenderTarget2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }

        private SSFxTargets _ssfxTargets;
        public SSFxTargets SSFxTargets { set { _ssfxTargets = value; } }
        public bool UseTonemap
        {
            get { return _useTonemapping && TemporalAAFx.ModuleTonemapEnabled; }
            set { _useTonemapping = value;  }
        }
        public HaltonSequence HaltonSequence => _haltonSequence;



        protected override bool GetEnabled() => _enabled && TemporalAAFx.ModuleEnabled;
        public void SwapOffFrame()
        { IsOffFrame = !IsOffFrame; }

        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            if (previousRT == null && sourceRT == null)
                _ssfxTargets?.GetTemporalAARenderTargets(this.IsOffFrame, out sourceRT, out previousRT);
            else if (previousRT == null)
                _ssfxTargets?.GetTemporalAARenderTargets(this.IsOffFrame, out _, out previousRT);
            else if (sourceRT == null)
                _ssfxTargets?.GetTemporalAARenderTargets(this.IsOffFrame, out sourceRT, out _);

            if (!this.Enabled)
                return sourceRT;

            _graphicsDevice.SetRenderTarget(sourceRT);
            _graphicsDevice.SetState(BlendStateOption.Opaque);

            _fxSetup.Param_UseTonemap.SetValue(this.UseTonemap);
            _fxSetup.Param_Resolution.SetValue(_resolution);
            _fxSetup.Param_FrustumCorners.SetValue(this.Frustum.ViewSpaceFrustum);

            _fxSetup.Param_UpdateMap.SetValue(destRT);
            _fxSetup.Param_AccumulationMap.SetValue(previousRT);
            _fxSetup.Param_CurrentToPrevious.SetValue(Matrices.CurrentViewToPreviousViewProjection);

            this.Draw(_fxSetup.Pass_TemporalAA);
            this.Blit(sourceRT, destRT);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(destRT);
                _fxSetup.Param_UpdateMap.SetValue(sourceRT);
                this.Draw(_fxSetup.Pass_TonemapInverse);

                // sample profiler if set
                this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_CombineTAA);

                return destRT;
            }

            // sample profiler if set
            this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_CombineTAA);

            return destRT;
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }

    }

}
