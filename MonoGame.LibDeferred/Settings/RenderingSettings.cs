using DeferredEngine.Recources.Helper;
using DeferredEngine.Rendering;
using System.Text;

namespace DeferredEngine.Recources
{

    public static partial class RenderingSettings
    {
        //debug
        public static bool e_DrawBoundingBox = true;
        public static bool d_Drawlines = false;

        //Editor
        public static bool e_IsEditorEnabled = true;

        //UI
        public static bool ui_IsUIEnabled = true;

        //Renderer
        //Default Material
        public static bool d_DefaultMaterial = false;
        public static float m_DefaultRoughness = 0.5f;

        //Settings
        public static RenderModes g_RenderMode = RenderModes.Deferred;
        public static bool g_CpuCulling = true;



        public static NotifiedProperty<float> g_FarClip = new NotifiedProperty<float>(500);

    }

    public class NotifiedProperty<T>
    {
        public event Action<T> Changed;
        private T _value;
        public T Value { get => _value; set => this.Set(value); }
        public NotifiedProperty(T value)
        {
            _value = value;
        }

        public bool Set(T value)
        {
            if (Equals(value, _value))
            {
                _value = value;
                this.Changed?.Invoke(value);
                return true;
            }
            return false;
        }

        public static implicit operator T(NotifiedProperty<T> property)
        {
            return property._value;
        }

    }

}
