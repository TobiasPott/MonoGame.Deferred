﻿using Deferred.Utilities;
using DeferredEngine.Pipeline;
using MonoGame.Ext;

namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {

        private static bool _enabledSelection = false;
        public static bool e_EnableSelection { get { return _enabledSelection && RenderingSettings.e_IsEditorEnabled; } set => _enabledSelection = value; }

        public static GizmoModes e_gizmoMode = GizmoModes.Translation;
        public static bool e_LocalTransformation = false;


        //debug
        public static bool e_DrawBoundingBox = true;
        public static bool d_Drawlines = false;

        //Editor
        public static bool e_IsEditorEnabled = true;

        //UI
        public static bool ui_IsUIEnabled = true;

        //Renderer
        //Default Material
        public static bool d_DefaultMaterial = false;
        public static float m_DefaultRoughness = 0.5f;

        //Settings
        public static PipelineOutputPasses g_CurrentPass = PipelineOutputPasses.Deferred;
        public static bool g_CpuCulling = true;


    }

}
