using Microsoft.Xna.Framework;
using MonoGame.Ext;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {
        public static class Screen
        {
            //Default & Display settings
            public static int Width => (int)g_Resolution.X;
            public static int Height => (int)g_Resolution.Y;

            public static Vector2 g_Resolution = new Vector2(1280, 720);
            public static Vector2 g_UIResolution = new Vector2(1280, 720);
            public static Vector2 InverseResolution => Vector2.One / g_Resolution;
            public static Rectangle Rect => new Rectangle(0, 0, (int)g_Resolution.X, (int)g_Resolution.Y);
            public static Rectangle g_TargetRect = new Rectangle(0, 0, (int)g_Resolution.X, (int)g_Resolution.Y);
            public static float Aspect => g_Resolution.X / g_Resolution.Y;

            public static bool g_VSync = false;
            public static int g_FixedFPS = 0;


            public static NotifiedProperty<float> g_FarClip = new NotifiedProperty<float>(-1);



            public static void SetResolution(int width, int height) => SetResolution(new Vector2(width, height));
            public static void SetResolution(Vector2 resolution)
            {
                g_Resolution = resolution;
            }
            public static void GetDestinationRectangle(float sourceAspect, out Rectangle destRectangle)
            {
                int height;
                int width;

                if (Math.Abs(sourceAspect - Aspect) < 0.001)
                //If same aspectratio
                {
                    height = Height;
                    width = Width;
                }
                else
                {
                    if (Height < Width)
                    {
                        //Should be squared!
                        height = Height;
                        width = Height;
                    }
                    else
                    {
                        height = Width;
                        width = Width;
                    }
                }

                destRectangle = new Rectangle(0, 0, width, height);
            }

        }
    }
}