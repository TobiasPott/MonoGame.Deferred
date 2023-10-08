using DeferredEngine.Renderer;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {
        //Default & Display settings
        public static int g_ScreenWidth => (int)g_ScreenResolution.X;
        public static int g_ScreenHeight => (int)g_ScreenResolution.Y;
        public static Vector2 g_ScreenResolution = new Vector2(1280, 720);
        public static Rectangle g_ScreenRect = new Rectangle(0, 0, 1280, 720);
        public static float g_ScreenAspect = g_ScreenResolution.X / g_ScreenResolution.Y;

        public static bool g_ScreenVSync = false;
        public static int g_ScreenFixedFPS = 0;

        public static void SetResolution(int width, int height) => SetResolution(new Vector2(width, height));
        public static void SetResolution(Vector2 resolution)
        {
            g_ScreenResolution = resolution;
            g_ScreenRect.Width = g_ScreenWidth;
            g_ScreenRect.Height = g_ScreenHeight;
            g_ScreenAspect = g_ScreenWidth / g_ScreenHeight;
        }

    }
}
