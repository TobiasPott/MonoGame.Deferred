using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class PostProcessingFx : BasePostFx
    {

        protected override bool GetEnabled() => _enabled && RenderingSettings.g_PostProcessing;


        private PostProcssingFxSetup _fxSetup = new PostProcssingFxSetup();


        // PostProcessing
        private float _chromaticAbberationStrength = 0.035f;
        public float ChromaticAbberationStrength
        {
            get { return _chromaticAbberationStrength; }
            set
            {
                _chromaticAbberationStrength = value;
                _fxSetup.Param_ChromaticAbberationStrength.SetValue(_chromaticAbberationStrength);
                _fxSetup.Effect.CurrentTechnique = _chromaticAbberationStrength <= 0 ? _fxSetup.Technique_Base : _fxSetup.Technique_VignetteChroma;
            }
        }

        private float _sCurveStrength = 0.05f;
        public float SCurveStrength
        {
            get { return _sCurveStrength; }
            set
            {
                _sCurveStrength = value;
                _fxSetup.Param_SCurveStrength.SetValue(_sCurveStrength);
            }
        }

        private float _whitePoint = 1.1f;
        public float WhitePoint
        {
            get { return _whitePoint; }
            set
            {
                _whitePoint = value;
                _fxSetup.Param_WhitePoint.SetValue(_whitePoint);
            }
        }

        private float _exposure = 0.75f;
        public float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                _fxSetup.Param_PowExposure.SetValue((float)Math.Pow(2, _exposure));
            }
        }


        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public PostProcessingFx(ContentManager content)
        {
        }

        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
                return sourceRT;

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
