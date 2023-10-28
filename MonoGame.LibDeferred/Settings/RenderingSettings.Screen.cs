using Microsoft.Xna.Framework;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {
        public static class Screen
        {
            //Default & Display settings
            public static int g_Width => (int)g_Resolution.X;
            public static int g_Height => (int)g_Resolution.Y;

            public static Vector2 g_Resolution = new Vector2(1280, 720);
            public static Vector2 g_InverseResolution => Vector2.One / g_Resolution;
            public static Rectangle g_Rect => new Rectangle(0, 0, (int)g_Resolution.X, (int)g_Resolution.Y);
            public static float g_Aspect => g_Resolution.X / g_Resolution.Y;

            public static bool g_VSync = false;
            public static int g_FixedFPS = 0;


            public static void SetResolution(int width, int height) => SetResolution(new Vector2(width, height));
            public static void SetResolution(Vector2 resolution)
            {
                g_Resolution = resolution;
            }
            public static void GetDestinationRectangle(float sourceAspect, out Rectangle destRectangle)
            {
                int height;
                int width;

                if (Math.Abs(sourceAspect - g_Aspect) < 0.001)
                //If same aspectratio
                {
                    height = g_Height;
                    width = g_Width;
                }
                else
                {
                    if (g_Height < g_Width)
                    {
                        //Should be squared!
                        height = g_Height;
                        width = g_Height;
                    }
                    else
                    {
                        height = g_Width;
                        width = g_Width;
                    }
                }

                destRectangle = new Rectangle(0, 0, width, height);
            }

        }
    }
}