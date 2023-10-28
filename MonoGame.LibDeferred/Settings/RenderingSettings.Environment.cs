using MonoGame.Ext;

namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        //Environment mapping
        public static class Environment
        {
            public readonly static NotifiedProperty<bool> Enabled = new NotifiedProperty<bool>(true);

        }

    }
}
