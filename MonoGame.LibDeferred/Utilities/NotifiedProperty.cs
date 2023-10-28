using System.Reflection;

namespace MonoGame.Ext
{
    public class NotifiedProperty<T>
    {
        public event Action<T> Changed;
        private T _value;
        public T Value { get => _value; set => Set(value); }
        public NotifiedProperty(T value)
        {
            _value = value;
        }

        public void Set(T value)
        {
            if (!Equals(value, _value))
            {
                _value = value;
                Changed?.Invoke(value);
            }
        }

        public static implicit operator T(NotifiedProperty<T> property)
        {
            return property._value;
        }


        public PropertyInfo GetValuePropertyInfo()
        {
            return GetType().GetProperty("Value");
        }
    }

}
