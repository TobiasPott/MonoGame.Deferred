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


        public static bool UIIsHovered;

        public static bool e_EnableSelection = false;
        public static GizmoModes e_gizmoMode = GizmoModes.Translation;
        public static bool e_LocalTransformation = false;

        public static float sdf_load = 0;
    }
}
