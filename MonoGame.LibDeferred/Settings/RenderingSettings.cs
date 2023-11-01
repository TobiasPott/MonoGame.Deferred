using Deferred.Utilities;
using DeferredEngine.Pipeline;

namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {

        private static bool _enabledSelection = false;
        public static bool EnableSelection { get { return _enabledSelection && RenderingSettings.e_IsEditorEnabled; } set => _enabledSelection = value; }

        public static GizmoModes e_gizmoMode = GizmoModes.Translation;
        public static bool e_LocalTransformation = false;

        //Editor
        public static bool e_IsEditorEnabled = true;
        //debug
        public static bool e_DrawBoundingBox = true;


        //Renderer
        //Default Material
        // ToDo: PRIO II: Move to MaterialBase class and hook up to UI
        public static bool d_DefaultMaterial = false;
        public static float m_DefaultRoughness = 0.5f;

        //Settings
        public static DeferredRenderingPasses g_CurrentPass = DeferredRenderingPasses.Deferred;
        public static bool g_CpuCulling = true;


    }

}
