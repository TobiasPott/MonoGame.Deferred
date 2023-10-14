namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        //Environment mapping
        public static class EnvironmentMapping
        {
            public static bool Enabled = true;
            //public static bool MapUpdateOnEveryFrame = true;
            public static int MapResolution = 1024;

            public static void ApplyDefaultsEnvironmentMap()
            {
                Enabled = true;
            }

        }

    }
}
