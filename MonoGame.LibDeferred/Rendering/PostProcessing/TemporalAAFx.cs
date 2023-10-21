using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.PostProcessing
{
    //Just a template
    public class TemporalAAFx : BaseFx
    {

        private TemporalAAFxEffectSetup _effectSetup = new TemporalAAFxEffectSetup();
        private bool _enabled = true;
        private bool _useTonemapping = true;
        private HaltonSequence _haltonSequence = new HaltonSequence();


        public bool Enabled { get => _enabled && RenderingSettings.TAA.Enabled; set { _enabled = value; } }
        public PipelineMatrices Matrices { get; set; }
        public bool IsOffFrame { get; protected set; } = true;
        public int JitterMode = 2;

        public Vector3[] FrustumCorners { set { _effectSetup.Param_FrustumCorners.SetValue(value); } }
        public Vector2 Resolution { set { _effectSetup.Param_Resolution.SetValue(value); } }
        public RenderTarget2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }

        public bool UseTonemap
        {
            get { return _useTonemapping && RenderingSettings.TAA.UseTonemapping; }
            set { _useTonemapping = value; _effectSetup.Param_UseTonemap.SetValue(value); }
        }
        public HaltonSequence HaltonSequence => _haltonSequence;


        public TemporalAAFx()
        { }

        public void SwapOffFrame()
        { IsOffFrame = !IsOffFrame; }

        public void Draw(RenderTarget2D currentFrame, RenderTarget2D lastFrame, RenderTarget2D output)
        {
            _graphicsDevice.SetRenderTarget(output);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _effectSetup.Param_UpdateMap.SetValue(currentFrame);
            _effectSetup.Param_AccumulationMap.SetValue(lastFrame);
            _effectSetup.Param_CurrentToPrevious.SetValue(Matrices.CurrentViewToPreviousViewProjection);

            this.Draw(_effectSetup.Pass_TemporalAA);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(currentFrame);
                _effectSetup.Param_UpdateMap.SetValue(output);
                this.Draw(_effectSetup.Pass_TonemapInverse);
            }
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }

    }

}
