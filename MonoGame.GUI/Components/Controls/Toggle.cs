using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GUIHelper;
using System.Reflection;

namespace MonoGame.GUI
{
    public class Toggle : TextBlock
    {
        public bool State;

        private const float ToggleIndicatorSize = 20;
        private const float ToggleIndicatorBorder = 10;

        public PropertyInfo ToggleProperty;
        public FieldInfo ToggleField;
        public object ToggleObject;

        public Toggle(GUIStyle style, String text) : this(
            position: Vector2.Zero,
            dimensions: style.Dimensions,
            text: text,
            font: style.TextFont,
            blockColor: style.Color,
            textColor: style.TextColor,
            textAlignment: style.TextAlignment,
            textBorder: style.TextBorder,
            layer: 0)
        { }

        public Toggle(Vector2 position, Vector2 dimensions, String text, SpriteFont font, Color blockColor, Color textColor, TextAlignment textAlignment = TextAlignment.Left, Vector2 textBorder = default, int layer = 0) : base(position, dimensions, text, font, blockColor, textColor, textAlignment, textBorder, layer)
        {

        }
        public void SetField(Object obj, string field)
        {
            ToggleObject = obj;
            ToggleField = obj.GetType().GetField(field);
            State = (bool)ToggleField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            ToggleObject = obj;
            ToggleProperty = obj.GetType().GetProperty(property);
            State = (bool)ToggleProperty.GetValue(obj);
        }

        protected override void ComputeFontPosition()
        {
            if (Text == null) return;
            Vector2 textDimensions = TextFont.MeasureString(Text);

            FontWrap(ref textDimensions, Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2));

            _fontPosition = TextAlignment switch
            {
                TextAlignment.Left => (Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2)) / 2 * Vector2.UnitY + _textBorder * Vector2.UnitX - textDimensions / 2 * Vector2.UnitY,
                TextAlignment.Center => (Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2)) / 2 - textDimensions / 2,
                TextAlignment.Right => (Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2)) * new Vector2(1, 0.5f) - _textBorder * Vector2.UnitX - textDimensions / 2,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, SwatchColor);
            guiRenderer.DrawQuad(parentPosition + Position + Dimensions * new Vector2(1, 0.5f) - ToggleIndicatorBorder * Vector2.UnitX - ToggleIndicatorSize * new Vector2(1, 0.5f), Vector2.One * ToggleIndicatorSize, State ? Color.LimeGreen : Color.Red);
            guiRenderer.DrawText(parentPosition + Position + _fontPosition, Text, TextFont, TextColor);
        }

        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (!GUIMouseInput.WasLMBClicked()) return;

            Vector2 bound1 = Position + parentPosition;
            Vector2 bound2 = bound1 + Dimensions;

            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                mousePosition.Y < bound2.Y)
            {
                State = !State;
                GUIMouseInput.UIWasUsed = true;

                if (ToggleObject != null)
                {
                    ToggleField?.SetValue(ToggleObject, State, BindingFlags.Public, null, null);
                    ToggleProperty?.SetValue(ToggleObject, State);
                }
                else
                {
                    ToggleField?.SetValue(null, State, BindingFlags.Static | BindingFlags.Public, null, null);
                    ToggleProperty?.SetValue(null, State);
                }

            }
        }

    }

}