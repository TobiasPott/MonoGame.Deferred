using Deferred.Utilities;

namespace DeferredEngine.Recources
{
    public static class RenderingStats
    {
        public static int MeshDraws = 0;
        public static int MaterialDraws = 0;
        public static int LightsDrawn = 0;

        public static int shadowMaps = 0;
        public static int activeShadowMaps = 0;
        public static int EmissiveMeshDraws = 0;

        public static float sdf_load = 0;


        public static bool UIIsHovered;


        public static void ResetStats()
        {
            sdf_load = 0.0f;
            MaterialDraws = 0;
            MeshDraws = 0;
            LightsDrawn = 0;
            shadowMaps = 0;
            activeShadowMaps = 0;
            EmissiveMeshDraws = 0;
        }

    }
}
