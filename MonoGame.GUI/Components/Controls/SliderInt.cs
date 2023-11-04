﻿using MonoGame.GUIHelper;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace MonoGame.GUI
{
    public class SliderInt : SliderFloat
    {
        public int MaxValueInt = 1;
        public int MinValueInt = 0;
        public int StepSize = 1;

        public int _sliderValue;
        public new int SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                _sliderPercent = (float)(_sliderValue - MinValue) / (MaxValue - MinValue);
            }
        }

        public SliderInt(GUIStyle guiStyle, int min, int max, int stepSize) : this(
            position: Vector2.Zero,
            dimensions: new Vector2(guiStyle.DimensionsStyle.X, 35),
            min: min,
            max: max,
            stepSize: stepSize,
            blockColor: guiStyle.BlockColorStyle,
            sliderColor: guiStyle.SliderColorStyle,
            layer: 0,
            alignment: guiStyle.GuiAlignmentStyle,
            ParentDimensions: guiStyle.ParentDimensionsStyle
            )
        { }

        public SliderInt(Vector2 position, Vector2 dimensions, int min, int max, int stepSize, Color blockColor, Color sliderColor, int layer = 0, Alignment alignment = Alignment.None, Vector2 ParentDimensions = new Vector2()) : base(position, dimensions, min, max, blockColor, sliderColor, layer, alignment, ParentDimensions)
        {
            MaxValueInt = max;
            MinValueInt = min;
            StepSize = stepSize;
        }

        public void SetField(Object obj, string field)
        {
            SliderObject = obj;
            SliderField = obj.GetType().GetField(field);
            SliderValue = (int)SliderField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            SliderObject = obj;
            SliderProperty = obj.GetType().GetProperty(property);
            SliderValue = (int)SliderProperty.GetValue(obj);
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

                _sliderValue = (int)Math.Round(_sliderPercent * (float)(MaxValue - MinValue) + MinValue) / StepSize * StepSize;

                _sliderPercent = (float)(_sliderValue - MinValueInt) / (MaxValueInt - MinValueInt);

                if (SliderObject != null)
                {
                    if (SliderField != null) SliderField.SetValue(SliderObject, SliderValue, BindingFlags.Public, null, null);
                    else SliderProperty?.SetValue(SliderObject, SliderValue);
                }
                else
                {
                    if (SliderField != null) SliderField.SetValue(null, SliderValue, BindingFlags.Static | BindingFlags.Public, null, null);
                    else SliderProperty?.SetValue(null, SliderValue);
                }
            }
        }
    }
}