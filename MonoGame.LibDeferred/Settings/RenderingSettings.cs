﻿using DeferredEngine.Renderer;

namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        //debug
        public static bool e_DrawBoundingBox = true;
        public static bool d_Drawlines = false;

        //Editor
        public static bool e_IsEditorEnabled = true;
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