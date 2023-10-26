namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {
        // Bloom
        public static class Bloom
        {
            public static bool Enabled = true;

            public readonly static NotifiedProperty<float> g_Threshold = new NotifiedProperty<float>(-1.0f);
            public static float Threshold { get => g_Threshold; set { g_Threshold.Set(value); } }


            public static float[] Radius = new float[] { 1.0f, 1.0f, 2.0f, 3.0f, 4.0f };
            public static float[] Strength = new float[] { 0.5f, 1.0f, 1.0f, 1.0f, 1.0f };

        }
    }
}
