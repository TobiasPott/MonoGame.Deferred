using DeferredEngine.Pipeline;

namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {

        private static bool _enabledSelection = false;
        public static bool EnableSelection { get { return _enabledSelection && RenderingSettings.e_IsEditorEnabled; } set => _enabledSelection = value; }

        public static bool e_LocalTransformation = false;

        //Editor
        public static bool e_IsEditorEnabled = true;


        //Renderer
        //Default Material
        // ToDo: PRIO II: Move to MaterialBase class and hook up to UI
        public static bool d_DefaultMaterial = false;

        //Settings
        public static DeferredRenderingPasses g_CurrentPass = DeferredRenderingPasses.Deferred;
        public static bool g_CpuCulling = true;


    }

}
