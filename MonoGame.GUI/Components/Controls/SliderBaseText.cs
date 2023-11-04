using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GUIHelper;
using System.Reflection;
using System.Text;

namespace MonoGame.GUI
{
    public abstract class SliderBaseText<T> : ColorSwatch
    {

        protected const float SliderIndicatorSize = 15;
        protected const float SliderIndicatorBorder = 10;
        protected const float SliderBaseHeight = 5;


        protected bool IsEngaged = false;

        protected Vector2 _tempPosition = Vector2.One;

        protected Vector2 SliderDimensions;

        protected float _sliderPercent;
        protected T _sliderValue;
        public T SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                UpdateText();
            }
        }

        public T MaxValue;
        public T MinValue;

        protected Color _sliderColor;
        //TextBlock associated
        protected TextBlock _textBlock;
        protected String _baseText;

        //Associated reference
        public PropertyInfo SliderProperty;
        public FieldInfo SliderField;
        public Object SliderObject;

        public SliderBaseText(GUIStyle style, T min, T max, String text) : this(Vector2.Zero, new Vector2(style.Dimensions.X, 35),
            new Vector2(style.Dimensions.X, 20), min, max, text,
            font: style.TextFont,
            blockColor: style.Color,
            layer: 0,
            alignment: style.Alignment,
            textAlignment: TextAlignment.Left,
            sliderColor: style.SliderColor,
            textBorder: style.TextBorder,
            ParentDimensions: style.ParentDimensions
            )
        { }
        public SliderBaseText(Vector2 position, Vector2 sliderDimensions, Vector2 textDimensions, T min, T max, String text, 
            SpriteFont font, Color blockColor, Color sliderColor, int layer = 0, 
            Alignment alignment = Alignment.None, TextAlignment textAlignment = TextAlignment.Left, 
            Vector2 textBorder = default, Vector2 ParentDimensions = new Vector2()) 
            : base(position, sliderDimensions, blockColor, layer, alignment, ParentDimensions)
        {
            _textBlock = new TextBlock(position, textDimensions, text, font, blockColor, sliderColor, textAlignment, textBorder, layer, alignment, ParentDimensions);

            Dimensions = sliderDimensions + _textBlock.Dimensions * Vector2.UnitY;
            SliderDimensions = sliderDimensions;
            _sliderColor = sliderColor;
            MinValue = min;
            MaxValue = max;
            _sliderValue = min;
            _baseText = text;

            UpdateText();
        }

        public void SetText(StringBuilder text)
        {
            _baseText = text.ToString();
            _textBlock.Text = text;
            UpdateText();
        }

        public void SetField(Object obj, string field)
        {
            SliderObject = obj;
            SliderField = obj.GetType().GetField(field);
            SliderProperty = null;
            SliderValue = (T)SliderField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            SliderObject = obj;
            SliderProperty = obj.GetType().GetProperty(property);
            SliderField = null;
            SliderValue = (T)SliderProperty.GetValue(obj);
        }
        protected virtual void UpdateText()
        {
            _textBlock.Text.Clear();
            _textBlock.Text.Append(_baseText);
        }

        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (GUIMouseInput.UIElementEngaged && !IsEngaged) return;

            //Break Engagement
            if (IsEngaged && !GUIMouseInput.IsLMBPressed())
            {
                GUIMouseInput.UIElementEngaged = false;
                IsEngaged = false;
            }

            if (!GUIMouseInput.IsLMBPressed()) return;

            Vector2 bound1 = Position + parentPosition + _textBlock.Dimensions * Vector2.UnitY /*+ SliderIndicatorBorder*Vector2.UnitX*/;
            Vector2 bound2 = bound1 + SliderDimensions/* - 2*SliderIndicatorBorder * Vector2.UnitX*/;

            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                mousePosition.Y < bound2.Y + 1)
            {
                GUIMouseInput.UIElementEngaged = true;
                IsEngaged = true;
            }

            if (IsEngaged)
            {
                GUIMouseInput.UIWasUsed = true;

                float lowerx = bound1.X + SliderIndicatorBorder;
                float upperx = bound2.X - SliderIndicatorBorder;

                _sliderPercent = MathHelper.Clamp((mousePosition.X - lowerx) / (upperx - lowerx), 0, 1);

                _sliderValue = CalculateSliderValue(_sliderPercent); // _sliderPercent * (MaxValue - MinValue) + MinValue;

                UpdateText();

                if (SliderObject != null)
                {
                    if (SliderField != null)
                        SliderField.SetValue(SliderObject, SliderValue, BindingFlags.Public, null, null);
                    else SliderProperty?.SetValue(SliderObject, SliderValue);
                }
                else
                {
                    if (SliderField != null)
                        SliderField.SetValue(null, SliderValue, BindingFlags.Static | BindingFlags.Public, null, null);
                    else SliderProperty?.SetValue(null, SliderValue);
                }
            }
        }

        protected abstract T CalculateSliderValue(float percentage);

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            _textBlock.Draw(guiRenderer, parentPosition, mousePosition);

            _tempPosition = parentPosition + Position + _textBlock.Dimensions * Vector2.UnitY;
            guiRenderer.DrawQuad(_tempPosition, SliderDimensions, SwatchColor);

            Vector2 slideDimensions = new Vector2(SliderDimensions.X - SliderIndicatorBorder * 2, SliderBaseHeight);
            guiRenderer.DrawQuad(_tempPosition + new Vector2(SliderIndicatorBorder,
                SliderDimensions.Y * 0.5f - SliderBaseHeight * 0.5f), slideDimensions, Color.DarkGray);

            //slideDimensions = new Vector2(slideDimensions.X + SliderIndicatorSize* 0.5f, slideDimensions.Y);
            guiRenderer.DrawQuad(_tempPosition + new Vector2(SliderIndicatorBorder - SliderIndicatorSize * 0.5f,
                 SliderDimensions.Y * 0.5f - SliderIndicatorSize * 0.5f) + _sliderPercent * slideDimensions * Vector2.UnitX, new Vector2(SliderIndicatorSize, SliderIndicatorSize), _sliderColor);
        }
    }
}