using Microsoft.Xna.Framework;
using System.Reflection;

namespace MonoGame.GUI
{
    public abstract class SliderBase<T> : ColorSwatch
    {
        protected const float SliderIndicatorSize = 15;
        protected const float SliderIndicatorBorder = 10;
        protected const float SliderBaseHeight = 5;


        protected bool IsEngaged = false;
        protected float _sliderPercent;

        protected Color _sliderColor;
        protected T _sliderValue;
        public abstract T SliderValue { get; set; }
        public T MaxValue;
        public T MinValue;


        public PropertyInfo SliderProperty;
        public FieldInfo SliderField;
        public Object SliderObject;


        public SliderBase(GUIStyle style, T min, T max) 
            : this(
                position: Vector2.Zero,
                dimensions: new Vector2(style.Dimensions.X, 35),
                min: min,
                max: max,
                blockColor: style.Color,
                sliderColor: style.SliderColor,
                layer: 0,
                alignment: style.Alignment,
                ParentDimensions: style.ParentDimensions
                )
        { }

        public SliderBase(Vector2 position, Vector2 dimensions, T min, T max, Color blockColor, Color sliderColor, int layer = 0, Alignment alignment = Alignment.None, Vector2 ParentDimensions = new Vector2())
            : base(position, dimensions, blockColor, layer, alignment, ParentDimensions)
        {
            _sliderColor = sliderColor;
            _sliderValue = min;
            MinValue = min;
            MaxValue = max;
        }

        public void SetField(Object obj, string field)
        {
            SliderObject = obj;
            SliderField = obj.GetType().GetField(field);
            SliderValue = (T)SliderField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            SliderObject = obj;
            SliderProperty = obj.GetType().GetProperty(property);
            SliderValue = (T)SliderProperty.GetValue(obj);
        }
    }
}