namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        public static void ApplyDefaultsSSR()
        {
            g_SSReflection = true;
            g_SSReflections_Samples = msamples;
            g_SSReflections_RefinementSamples = ssamples;
            g_SSReflection_FireflyReduction = _g_SSReflection_FireflyReduction;
            g_SSReflection_FireflyThreshold = _g_SSReflection_FireflyThreshold;

        }

        // SSR
        private static bool _g_SSReflection = true;

        public static bool g_SSReflection
        {
            get { return _g_SSReflection; }
            set
            {
                _g_SSReflection = value;
            }
        }

        private static bool _g_SSReflection_FireflyReduction = true;

        public static bool g_SSReflection_FireflyReduction
        {
            get { return _g_SSReflection_FireflyReduction; }
            set
            {
                _g_SSReflection_FireflyReduction = value;
            }
        }

        private static float _g_SSReflection_FireflyThreshold = 1.75f;

        public static float g_SSReflection_FireflyThreshold
        {
            get { return _g_SSReflection_FireflyThreshold; }
            set
            {
                _g_SSReflection_FireflyThreshold = value;
            }
        }


        public static bool g_SSReflectionNoise = true;
        private static bool _g_SSReflection_Taa = true;
        public static bool g_SSReflectionTaa
        {
            get { return _g_SSReflection_Taa; }
            set
            {
                _g_SSReflection_Taa = value;
                if (value) g_SSReflectionNoise = true;
            }
        }

        // Screen Space Ambient Occlusion


        //5 and 5 are good, 3 and 3 are cheap
        private static int msamples = 3;
        public static int g_SSReflections_Samples
        {
            get { return msamples; }
            set
            {
                msamples = value;
            }
        }

        private static int ssamples = 3;
        public static int g_SSReflections_RefinementSamples
        {
            get { return ssamples; }
            set
            {
                ssamples = value;
            }
        }

    }

}
