using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{
    //Just a template
    public class TemporalAAFx : PostFx
    {
        public static bool g_Enabled { get; set; } = true;
        public static bool g_UseTonemapping { get; set; } = true;




        private TemporalAAFxSetup _fxSetup = new TemporalAAFxSetup();
        private bool _useTonemapping = true;
        private HaltonSequence _haltonSequence = new HaltonSequence();


        public PipelineMatrices Matrices { get; set; }
        public bool IsOffFrame { get; protected set; } = true;
        public int JitterMode = 2;

        public Vector3[] FrustumCorners { set { _fxSetup.Param_FrustumCorners.SetValue(value); } }
        public Vector2 Resolution { set { _fxSetup.Param_Resolution.SetValue(value); } }
        public RenderTarget2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }

        public bool UseTonemap
        {
            get { return _useTonemapping && TemporalAAFx.g_UseTonemapping; }
            set { _useTonemapping = value; _fxSetup.Param_UseTonemap.SetValue(value); }
        }
        public HaltonSequence HaltonSequence => _haltonSequence;


        public TemporalAAFx()
        { }

        protected override bool GetEnabled() => _enabled && TemporalAAFx.g_Enabled;
        public void SwapOffFrame()
        { IsOffFrame = !IsOffFrame; }

        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetState(BlendStateOption.Opaque);

            _fxSetup.Param_UpdateMap.SetValue(sourceRT);
            _fxSetup.Param_AccumulationMap.SetValue(previousRT);
            _fxSetup.Param_CurrentToPrevious.SetValue(Matrices.CurrentViewToPreviousViewProjection);

            this.Draw(_fxSetup.Pass_TemporalAA);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(sourceRT);
                _fxSetup.Param_UpdateMap.SetValue(destRT);
                this.Draw(_fxSetup.Pass_TonemapInverse);
            }

            return destRT;
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }

    }

}
