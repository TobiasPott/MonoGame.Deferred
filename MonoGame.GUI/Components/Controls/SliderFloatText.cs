using DeferredEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace MonoGame.GUI
{

    public class SliderFloatText : SliderBaseText<float>
    {
        private uint roundDecimals = 1;

        public SliderFloatText(GUIStyle style, float min, float max, uint decimals, String text)
            : this(Vector2.Zero, new Vector2(style.Dimensions.X, 35), new Vector2(style.Dimensions.X, 20),
            min, max, decimals: decimals, text,
            style.TextFont, style.Color, style.SliderColor, 0,
            style.Alignment, TextAlignment.Left, style.TextBorder, style.ParentDimensions
            )
        { }

        public SliderFloatText(Vector2 position, Vector2 sliderDimensions, Vector2 textDimensions, float min, float max, uint decimals,
            String text, SpriteFont font, Color blockColor, Color sliderColor,
            int layer = 0, Alignment alignment = Alignment.None, TextAlignment textAlignment = TextAlignment.Left,
            Vector2 textBorder = default, Vector2 ParentDimensions = new Vector2())
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

        protected override float CalculateSliderValue(float percentage)
        {
            return percentage * (MaxValue - MinValue) + MinValue;
        }
        protected override void UpdateText()
        {
            base.UpdateText();
            _sliderPercent = (_sliderValue - MinValue) / (MaxValue - MinValue);
            _textBlock.Text.Concat(_sliderValue, roundDecimals);
        }

    }
}