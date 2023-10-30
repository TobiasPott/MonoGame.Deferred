using MonoGame.Ext;

namespace DeferredEngine.Recources
{


    public static partial class RenderingSettings
    {
        // Bloom
        public static class Bloom
        {
            public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);

            public readonly static NotifiedProperty<float> Threshold = new NotifiedProperty<float>(-1.0f);
        }
    }
}
