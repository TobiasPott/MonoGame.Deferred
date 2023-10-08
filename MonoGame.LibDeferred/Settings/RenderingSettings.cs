using DeferredEngine.Renderer;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {

        public static int u_showdisplayinfo = 3;
        public static bool p_physics = false;


        //Editor
        public static bool e_enableeditor = true;
        public static bool e_drawoutlines = true;
        public static bool e_drawboundingbox = true;

        //UI
        public static bool ui_enabled = true;

        //Renderer

        //debug
        public static bool d_drawlines = true;

        //Default Material
        public static bool d_defaultmaterial = false;
        public static float m_defaultroughness = 0.5f;

        //Settings
        public static RenderModes g_rendermode = RenderModes.Deferred;
        public static float g_farplane = 500;
        public static bool g_cpusort = true;
        public static bool g_cpuculling = true;
        public static bool g_batchbymaterial = false; //Note this must be activated before the application is started.

        //Deferred Decals
        public static bool g_drawdecals = true;

        //Forward pass
        public static bool g_forwardenable = true;

        //Profiler
        public static bool d_profiler = false;

        //Environment mapping
        public static bool g_environmentmapping = true;
        public static bool g_envmapupdateeveryframe = false;
        public static int g_envmapresolution = 1024;

        //Shadow Settings
        public static int g_shadowforcefiltering = 0; //1 = PCF, 2 3 better PCF  4 = Poisson, 5 = VSM;
        public static bool g_shadowforcescreenspace = false;

        //Temporal AntiAliasing
        public static bool g_taa = true;
        public static int g_taa_jittermode = 2;
        public static bool g_taa_tonemapped = true;


        // Emissive 

        //public static bool g_EmissiveDraw = true;
        //public static bool g_EmissiveDrawDiffuse = true;
        //public static bool g_EmissiveDrawSpecular = true;
        //public static bool g_EmissiveNoise = false;
        //public static float g_EmissiveDrawFOVFactor = 2;

        ////Whether or not materials' lighting scales with strength
        //public static bool g_EmissiveMaterialeSizeStrengthScaling = true;

        //private static int _g_EmissiveMaterialSamples = 8;
        //public static int g_EmissiveMaterialSamples
        //{
        //    get { return _g_EmissiveMaterialSamples; }
        //    set
        //    {
        //        _g_EmissiveMaterialSamples = value;
        //        Shaders.EmissiveEffect.Parameters["Samples"].SetValue(_g_EmissiveMaterialSamples);
        //    }
        //}



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


        private static bool _g_Linear = true;
        public static bool g_Linear
        {
            get { return _g_Linear; }
            set
            {
                _g_Linear = value;
                Shaders.DeferredCompose.CurrentTechnique = value
                    ? Shaders.DeferredComposeTechnique_Linear
                    : Shaders.DeferredComposeTechnique_NonLinear;
            }
        }

        public static bool g_SSReflectionNoise = true;
        public static bool g_VolumetricLights = true;
        public static bool g_ClearGBuffer = true;


        private static bool _g_SSReflection_Taa = true;
        public static bool g_SSReflectionTaa
        {
            get { return _g_SSReflection_Taa; }
            set
            {
                _g_SSReflection_Taa = value;
                Shaders.ScreenSpaceReflectionEffect.CurrentTechnique = value
                    ? Shaders.ScreenSpaceReflectionTechnique_Taa
                    : Shaders.ScreenSpaceReflectionTechnique_Default;

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
                Shaders.ScreenSpaceReflectionEffect.Parameters["Samples"].SetValue(msamples);
            }
        }

        private static int ssamples = 3;
        public static int g_SSReflections_RefinementSamples
        {
            get { return ssamples; }
            set
            {
                ssamples = value;
                Shaders.ScreenSpaceReflectionEffect.Parameters["SecondarySamples"].SetValue(ssamples);
            }
        }

        //private static float _g_TemporalAntiAliasingThreshold = 0.9f;
        public static int g_UseDepthStencilLightCulling = 1; //None, Depth, Depth+Stencil

        public static float ShadowBias = 0.005f;
        public static int sdf_threads = 4;
        public static bool sdf_cpu = false;
        public static bool sdf_draw = true;
        public static bool sdf_drawdistance = false;
        public static bool sdf_debug = false;
        public static bool sdf_subsurface = true;
        public static bool sdf_drawvolume = false;
        public static bool sdf_regenerate;
        public static bool d_drawnothing = false;
        public static bool e_saveBoundingBoxes = true;
        public static bool d_hotreloadshaders = true;

        public static void ApplySettings()
        {
            ApplyDefaultsSSAO();

            g_ssao_draw = true;


            g_PostProcessing = true;
            g_taa = true;
            g_environmentmapping = true;

            g_SSReflection = true;
            g_SSReflections_Samples = msamples;
            g_SSReflections_RefinementSamples = ssamples;
            g_SSReflection_FireflyReduction = _g_SSReflection_FireflyReduction;
            g_SSReflection_FireflyThreshold = _g_SSReflection_FireflyThreshold;

            g_Linear = _g_Linear;

            d_defaultmaterial = false;
            SCurveStrength = _sCurveStrength;
            Exposure = _exposure;
            ChromaticAbberationStrength = _chromaticAbberationStrength;

        }

    }
}
