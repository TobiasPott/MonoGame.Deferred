﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Text;

namespace MonoGame.GUI
{
    public abstract class SliderBaseText<T> : ColorSwatch
    {
        protected bool IsEngaged = false;

        protected const float SliderIndicatorSize = 15;
        protected const float SliderIndicatorBorder = 10;
        protected const float SliderBaseHeight = 5;

        protected Vector2 _tempPosition = Vector2.One;

        protected Vector2 SliderDimensions;

        protected T _sliderPercent;

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
        protected String baseText;

        //Associated reference
        public PropertyInfo SliderProperty;
        public FieldInfo SliderField;
        public Object SliderObject;

        public SliderBaseText(GUIStyle guiStyle, T min, T max, String text) : this(Vector2.Zero, new Vector2(guiStyle.DimensionsStyle.X, 35),
            new Vector2(guiStyle.DimensionsStyle.X, 20), min, max, text,
            font: guiStyle.TextFontStyle,
            blockColor: guiStyle.BlockColorStyle,
            layer: 0,
            alignment: guiStyle.GuiAlignmentStyle,
            textAlignment: GUIStyle.TextAlignment.Left,
            sliderColor: guiStyle.SliderColorStyle,
            textBorder: guiStyle.TextBorderStyle,
            ParentDimensions: guiStyle.ParentDimensionsStyle
            )
        { }
        public SliderBaseText(Vector2 position, Vector2 sliderDimensions, Vector2 textDimensions, T min, T max, String text, 
            SpriteFont font, Color blockColor, Color sliderColor, int layer = 0, 
            Alignment alignment = Alignment.None, GUIStyle.TextAlignment textAlignment = GUIStyle.TextAlignment.Left, 
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
            baseText = text;

            UpdateText();
        }

        public void SetText(StringBuilder text)
        {
            baseText = text.ToString();
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
            _textBlock.Text.Append(baseText);
        }


    }
}