namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {

        //Environment mapping
        public static bool g_environmentmapping = true;
        public static bool g_envmapupdateeveryframe = false;
        public static int g_envmapresolution = 1024;

        public static void ApplyDefaultsEnvironmentMap()
        {
            g_environmentmapping = true;
        }

    }
}
