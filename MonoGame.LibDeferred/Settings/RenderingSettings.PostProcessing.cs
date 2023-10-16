﻿namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        public static void ApplyDefaultsPostProcessing()
        {
            g_PostProcessing = true;
        }


        public static bool g_PostProcessing = true;

        public static bool g_ColorGrading = true;

        // PostProcessing
        private static float _chromaticAbberationStrength = 0.035f;
        public static float ChromaticAbberationStrength
        {
            get { return _chromaticAbberationStrength; }
            set
            {
                _chromaticAbberationStrength = value;
                Shaders.PostProcssing.Param_ChromaticAbberationStrength.SetValue(_chromaticAbberationStrength);

                if (_chromaticAbberationStrength <= 0)
                    Shaders.PostProcssing.Effect.CurrentTechnique = Shaders.PostProcssing.Technique_Base;
                else
                {
                    Shaders.PostProcssing.Effect.CurrentTechnique = Shaders.PostProcssing.Technique_VignetteChroma;
                }
            }
        }

        private static float _sCurveStrength = 0.05f;
        public static float SCurveStrength
        {
            get { return _sCurveStrength; }
            set
            {
                _sCurveStrength = value;
                Shaders.PostProcssing.Param_SCurveStrength.SetValue(_sCurveStrength);
            }
        }

        private static float _whitePoint = 1.1f;
        public static float WhitePoint
        {
            get { return _whitePoint; }
            set
            {
                _whitePoint = value;
                Shaders.PostProcssing.Param_WhitePoint.SetValue(_whitePoint);
            }
        }

        private static float _exposure = 0.75f;
        public static float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                Shaders.PostProcssing.Param_PowExposure.SetValue((float)Math.Pow(2, _exposure));
            }
        }

    }
}