﻿using Microsoft.Xna.Framework;
using MonoGame.GUIHelper;
using System.Reflection;

namespace MonoGame.GUI
{
    /// <summary>
    /// A slider that can reference float values
    /// </summary>
    public class SliderFloat : ColorSwatch
    {
        protected bool IsEngaged = false;

        protected const float SliderIndicatorSize = 15;
        protected const float SliderIndicatorBorder = 10;
        protected const float SliderBaseHeight = 5;

        protected float _sliderPercent;

        private float _sliderValue;
        public float SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                _sliderPercent = (_sliderValue - MinValue) / (MaxValue - MinValue);
            }
        }

        public float MaxValue = 1;
        public float MinValue;

        protected Color _sliderColor;

        public PropertyInfo SliderProperty;
        public FieldInfo SliderField;
        public Object SliderObject;

        public SliderFloat(GUIStyle style, float min, float max) : this(
            position: Vector2.Zero,
            dimensions: new Vector2(style.Dimensions.X, 35),
            min: min,
            max: max,
            blockColor: style.Color,
            sliderColor: style.SliderColor,
            layer: 0,
            alignment: style.GuiAlignment,
            ParentDimensions: style.ParentDimensions
            )
        { }

        public SliderFloat(Vector2 position, Vector2 dimensions, float min, float max, Color blockColor, Color sliderColor, int layer = 0, Alignment alignment = Alignment.None, Vector2 ParentDimensions = new Vector2()) : base(position, dimensions, blockColor, layer, alignment, ParentDimensions)
        {
            _sliderColor = sliderColor;
            MinValue = min;
            MaxValue = max;
            _sliderValue = min;
        }

        public void SetField(Object obj, string field)
        {
            SliderObject = obj;
            SliderField = obj.GetType().GetField(field);
            SliderValue = (float)SliderField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            SliderObject = obj;
            SliderProperty = obj.GetType().GetProperty(property);
            SliderValue = (float)SliderProperty.GetValue(obj);
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

            Vector2 bound1 = Position + parentPosition /*+ SliderIndicatorBorder*Vector2.UnitX*/;
            Vector2 bound2 = bound1 + Dimensions/* - 2*SliderIndicatorBorder * Vector2.UnitX*/;

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

                _sliderValue = _sliderPercent * (MaxValue - MinValue) + MinValue;

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

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, SwatchColor);

            Vector2 slideDimensions = new Vector2(Dimensions.X - SliderIndicatorBorder * 2, SliderBaseHeight);
            guiRenderer.DrawQuad(parentPosition + Position + new Vector2(SliderIndicatorBorder,
                Dimensions.Y * 0.5f - SliderBaseHeight * 0.5f), slideDimensions, Color.DarkGray);

            //slideDimensions = new Vector2(slideDimensions.X + SliderIndicatorSize* 0.5f, slideDimensions.Y);
            guiRenderer.DrawQuad(parentPosition + Position + new Vector2(SliderIndicatorBorder - SliderIndicatorSize * 0.5f,
                 Dimensions.Y * 0.5f - SliderIndicatorSize * 0.5f) + _sliderPercent * slideDimensions * Vector2.UnitX, new Vector2(SliderIndicatorSize, SliderIndicatorSize), _sliderColor);
        }
    }
}