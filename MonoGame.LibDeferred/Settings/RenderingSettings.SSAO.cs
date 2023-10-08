namespace DeferredEngine.Recources
{

    public partial class RenderingSettings
    {

        public static void ApplyDefaultsSSAO()
        {
            g_ssao_falloffmax = _g_ssao_falloffmax;
            g_ssao_falloffmin = _g_ssao_falloffmin;
            g_ssao_radius = _g_ssao_radius;
            g_ssao_samples = g_ssao_samples;
            g_ssao_strength = g_ssao_strength;
            g_ssao_draw = _g_ssao_draw;
            g_ssao_draw = true;

        }


        //Screen Space Ambient Occlusion
        public static bool g_ssao_blur = true;

        private static bool _g_ssao_draw = true;
        public static bool g_ssao_draw
        {
            get { return _g_ssao_draw; }
            set
            {
                _g_ssao_draw = value;
                Shaders.DeferredCompose.Effect.Parameters["useSSAO"].SetValue(_g_ssao_draw);
            }
        }

        private static float _g_ssao_falloffmin = 0.001f;
        public static float g_ssao_falloffmin
        {
            get { return _g_ssao_falloffmin; }
            set
            {
                _g_ssao_falloffmin = value;
                Shaders.SSAO.Param_FalloffMin.SetValue(value);
            }
        }

        private static float _g_ssao_falloffmax = 0.03f;
        public static float g_ssao_falloffmax
        {
            get { return _g_ssao_falloffmax; }
            set
            {
                _g_ssao_falloffmax = value;
                Shaders.SSAO.Param_FalloffMax.SetValue(value);
            }
        }

        private static int _g_ssao_samples = 8;
        public static int g_ssao_samples
        {
            get { return _g_ssao_samples; }
            set
            {
                _g_ssao_samples = value;
                Shaders.SSAO.Param_Samples.SetValue(value);
            }
        }

        private static float _g_ssao_radius = 30;
        public static float g_ssao_radius
        {
            get { return _g_ssao_radius; }
            set
            {
                _g_ssao_radius = value;
                Shaders.SSAO.Param_SampleRadius.SetValue(value);
            }
        }

        private static float _g_ssao_strength = 0.5f;
        public static float g_ssao_strength
        {
            get { return _g_ssao_strength; }
            set
            {
                _g_ssao_strength = value;
                Shaders.SSAO.Param_Strength.SetValue(value);
            }
        }

    }
}
