namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        //Temporal AntiAliasing
        public static bool g_taa = true;
        public static int g_taa_jittermode = 2;
        public static bool g_taa_tonemapped = true;



        public static void ApplyDefaultsTAA()
        {
            g_taa = true;
        }

    }
}
