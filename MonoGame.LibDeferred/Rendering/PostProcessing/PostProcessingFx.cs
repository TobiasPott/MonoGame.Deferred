using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class PostProcessingFx : BaseFx
    {

        protected override bool GetEnabled() => _enabled && RenderingSettings.g_PostProcessing;


        private PostProcssingFxSetup _effectSetup = new PostProcssingFxSetup();


        // PostProcessing
        private float _chromaticAbberationStrength = 0.035f;
        public float ChromaticAbberationStrength
        {
            get { return _chromaticAbberationStrength; }
            set
            {
                _chromaticAbberationStrength = value;
                _effectSetup.Param_ChromaticAbberationStrength.SetValue(_chromaticAbberationStrength);
                _effectSetup.Effect.CurrentTechnique = _chromaticAbberationStrength <= 0 ? _effectSetup.Technique_Base : _effectSetup.Technique_VignetteChroma;
            }
        }

        private float _sCurveStrength = 0.05f;
        public float SCurveStrength
        {
            get { return _sCurveStrength; }
            set
            {
                _sCurveStrength = value;
                _effectSetup.Param_SCurveStrength.SetValue(_sCurveStrength);
            }
        }

        private float _whitePoint = 1.1f;
        public float WhitePoint
        {
            get { return _whitePoint; }
            set
            {
                _whitePoint = value;
                _effectSetup.Param_WhitePoint.SetValue(_whitePoint);
            }
        }

        private float _exposure = 0.75f;
        public float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                _effectSetup.Param_PowExposure.SetValue((float)Math.Pow(2, _exposure));
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

            _effectSetup.Param_ScreenTexture.SetValue(sourceRT);
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);

            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            return destRT;
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }


    }
}
