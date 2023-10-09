using DeferredEngine.Renderer;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {

        public static int u_showdisplayinfo = 3;

        //Editor
        public static bool e_IsEditorEnabled = true;
        public static bool e_drawoutlines = true;
        public static bool e_drawboundingbox = true;
        //debug
        public static bool d_drawlines = true;
        //Profiler
        public static bool d_IsProfileEnabled = true;

        //UI
        public static bool ui_IsUIEnabled = true;

        //Renderer
        //Default Material
        public static bool d_defaultmaterial = false;
        public static float m_defaultroughness = 0.5f;

        //Settings
        public static RenderModes g_rendermode = RenderModes.Deferred;
        public static float g_farplane = 500;
        public static bool g_cpusort = true;
        public static bool g_cpuculling = true;
        public static bool g_batchbymaterial = true; //Note this must be activated before the application is started.

        //Deferred Decals
        public static bool g_EnableDecals = true;
        //Forward pass
        public static bool g_EnableForward = true;

        //Shadow Settings
        public static int g_shadowforcefiltering = 0; //1 = PCF, 2 3 better PCF  4 = Poisson, 5 = VSM;
        public static bool g_shadowforcescreenspace = false;



        private static bool _g_Linear = true;
        public static bool g_Linear
        {
            get { return _g_Linear; }
            set
            {
                _g_Linear = value;
                Shaders.DeferredCompose.Effect.CurrentTechnique = value
                    ? Shaders.DeferredCompose.Technique_Linear
                    : Shaders.DeferredCompose.Technique_NonLinear;
            }
        }

        public static bool g_VolumetricLights = true;
        public static bool g_ClearGBuffer = true;


        public static int g_UseDepthStencilLightCulling = 1; //None, Depth, Depth+Stencil


        public static float ShadowBias = 0.005f;
        public static bool e_saveBoundingBoxes = true;
        public static bool d_hotreloadshaders = true;

        public static void ApplySettings()
        {
            ApplyDefaultsSSAO();

            ApplyDefaultsPostProcessing();

            ApplyDefaultsTAA();

            ApplyDefaultsEnvironmentMap();
            
            ApplyDefaultsSSR();
            
            g_Linear = _g_Linear;

            d_defaultmaterial = false;
            SCurveStrength = _sCurveStrength;
            Exposure = _exposure;
            ChromaticAbberationStrength = _chromaticAbberationStrength;

        }

    }
}
