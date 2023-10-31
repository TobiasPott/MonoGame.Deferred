using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class PostProcessingFx : PostFx
    {
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);


        protected override bool GetEnabled() => _enabled && ModuleEnabled;


        private readonly PostProcssingFxSetup _fxSetup = new PostProcssingFxSetup();


        // PostProcessing
        private float _chromaticAbberationStrength = 0.035f;
        private float _sCurveStrength = 0.05f;
        private float _whitePoint = 1.1f;
        private float _exposure = 0.75f;

        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
            {
                this.Blit(sourceRT, destRT);
                return destRT;
            }
            _fxSetup.Param_ChromaticAbberationStrength.SetValue(_chromaticAbberationStrength);
            _fxSetup.Effect.CurrentTechnique = _chromaticAbberationStrength <= 0 ? _fxSetup.Technique_Base : _fxSetup.Technique_VignetteChroma;
            _fxSetup.Param_SCurveStrength.SetValue(_sCurveStrength);
            _fxSetup.Param_WhitePoint.SetValue(_whitePoint);
            _fxSetup.Param_PowExposure.SetValue((float)Math.Pow(2, _exposure));

            _fxSetup.Param_ScreenTexture.SetValue(sourceRT);
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);

            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            return destRT;
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }


    }
}
