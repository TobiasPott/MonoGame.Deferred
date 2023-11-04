using DeferredEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace MonoGame.GUI
{
    public class SliderIntText : SliderBaseText<int>
    {
        public int StepSize = 1;



        public SliderIntText(GUIStyle style, int min, int max, int stepSize, String text) : this(
            position: Vector2.Zero,
            sliderDimensions: new Vector2(style.Dimensions.X, 35),
            textDimensions: new Vector2(style.Dimensions.X, 20),
            min: min,
            max: max,
            stepSize: stepSize,
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

        public SliderIntText(Vector2 position, Vector2 sliderDimensions, Vector2 textDimensions, int min, int max, int stepSize, String text, SpriteFont font, Color blockColor, Color sliderColor, int layer = 0, Alignment alignment = Alignment.None, TextAlignment textAlignment = TextAlignment.Left, Vector2 textBorder = default, Vector2 ParentDimensions = new Vector2())
             : base(position, sliderDimensions, textDimensions, min, max, text, font, blockColor, sliderColor, layer, alignment, textAlignment, textBorder, ParentDimensions)
        {
            StepSize = stepSize;
            UpdateText();
        }

        public void SetValues(string text, int minValue, int maxValue, int stepSize)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            StepSize = stepSize;
            SetText(new StringBuilder(text));
        }
        protected override void UpdateText()
        {
            base.UpdateText();
            _sliderPercent = (float)(_sliderValue - MinValue) / (MaxValue - MinValue);
            _textBlock.Text.Concat(_sliderValue);
        }
        protected override int CalculateSliderValue(float percentage)
        {
            return (int)Math.Floor(percentage * (MaxValue - MinValue) + MinValue);
        }

    }
}