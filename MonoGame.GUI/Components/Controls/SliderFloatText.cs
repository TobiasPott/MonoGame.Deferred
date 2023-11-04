using DeferredEngine.Utilities;
using MonoGame.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Text;

namespace MonoGame.GUI
{

    public class SliderFloatText : SliderBaseText<float>
    {
        private uint roundDecimals = 1;

        public SliderFloatText(GUIStyle style, float min, float max, uint decimals, String text) 
            : this(
            position: Vector2.Zero,
            sliderDimensions: new Vector2(style.Dimensions.X, 35),
            textDimensions: new Vector2(style.Dimensions.X, 20),
            min: min,
            max: max,
            decimals: decimals,
            text: text,
            font: style.TextFont,
            textBorder: style.TextBorder,
            textAlignment: TextAlignment.Left,
            blockColor: style.Color,
            sliderColor: style.SliderColor,
            layer: 0,
            alignment: style.Alignment,
            ParentDimensions: style.ParentDimensions
            )
        { }

        public SliderFloatText(Vector2 position, Vector2 sliderDimensions, Vector2 textDimensions, float min, float max, uint decimals,
            String text, SpriteFont font, Color blockColor, Color sliderColor,
            int layer = 0, Alignment alignment = Alignment.None, TextAlignment textAlignment = TextAlignment.Left, Vector2 textBorder = default, Vector2 ParentDimensions = new Vector2())
            : base(position, sliderDimensions, textDimensions, min, max, text, font, blockColor, sliderColor, layer, alignment, textAlignment, textBorder, ParentDimensions)
        {
            roundDecimals = decimals;
            UpdateText();
        }


        public void SetValues(string text, float minValue, float maxValue, uint decimals)
        {
            SetText(new StringBuilder(text));
            MinValue = minValue;
            MaxValue = maxValue;
            roundDecimals = decimals;
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

                _sliderValue = _sliderPercent * (MaxValue - MinValue) + MinValue;

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

        protected override void UpdateText()
        {
            base.UpdateText();
            _sliderPercent = (_sliderValue - MinValue) / (MaxValue - MinValue);
            _textBlock.Text.Concat(_sliderValue, roundDecimals);
        }

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