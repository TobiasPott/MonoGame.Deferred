using DeferredEngine.Renderer;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {

        public static int u_ShowDisplayInfo = 3;

        //Editor
        public static bool e_IsEditorEnabled = true;
        public static bool e_DrawOutlines = true;
        public static bool e_DrawBoundingBox = true;
        //debug
        public static bool d_Drawlines = true;
        //Profiler
        public static bool d_IsProfileEnabled = false;

        //UI
        public static bool ui_IsUIEnabled = true;

        //Renderer
        //Default Material
        public static bool d_DefaultMaterial = false;
        public static float m_DefaultRoughness = 0.5f;

        //Settings
        public static RenderModes g_RenderMode = RenderModes.Deferred;
        public static float g_FarPlane = 500;
        public static bool g_CpuCulling = true;

        //Deferred Decals
        public static bool g_EnableDecals = true;
        //Forward pass
        public static bool g_EnableForward = true;

        //Shadow Settings
        public static bool g_VolumetricLights = true;

        public static int g_UseDepthStencilLightCulling = 1; //None, Depth, Depth+Stencil


        public static float ShadowBias = 0.005f;
        public static bool d_hotreloadshaders = true;

        public static void ApplySettings()
        {
            ApplyDefaultsSSAO();

            ApplyDefaultsPostProcessing();

            EnvironmentMapping.ApplyDefaultsEnvironmentMap();

            ApplyDefaultsSSR();

            d_DefaultMaterial = false;
            SCurveStrength = _sCurveStrength;
            Exposure = _exposure;
            ChromaticAbberationStrength = _chromaticAbberationStrength;

        }

    }
}
