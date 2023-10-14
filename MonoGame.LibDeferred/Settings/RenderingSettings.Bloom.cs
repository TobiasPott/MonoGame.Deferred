namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {
        // Bloom
        public static class Bloom
        {
            public static bool Enabled = true;

            public static float Threshold = 0.0f;

            public static float[] Radius = new float[5]; 
            public static float[] Strength = new float[5];

            public static float Radius1 = 1.0f;
            public static float Radius2 = 1.0f;
            public static float Radius3 = 2.0f;
            public static float Radius4 = 3.0f;
            public static float Radius5 = 4.0f;

            public static float Strength1 = 0.5f;
            public static float Strength2 = 1;
            public static float Strength3 = 1;
            public static float Strength4 = 1.0f;
            public static float Strength5 = 1.0f;
        }
    }
}
