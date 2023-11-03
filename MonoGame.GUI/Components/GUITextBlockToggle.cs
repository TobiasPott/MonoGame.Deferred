using System;
using System.Reflection;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelperSuite.GUI
{
    public class GUITextBlockToggle : GUITextBlock
    {
        public bool Toggle;

        private const float ToggleIndicatorSize = 20;
        private const float ToggleIndicatorBorder = 10;
        
        public PropertyInfo ToggleProperty;
        public FieldInfo ToggleField;
        public object ToggleObject;

        public GUITextBlockToggle(GUIStyle guitStyle, String text) : this(
            position: Vector2.Zero,
            dimensions: guitStyle.DimensionsStyle,
            text: text,
            font: guitStyle.TextFontStyle,
            blockColor: guitStyle.BlockColorStyle,
            textColor: guitStyle.TextColorStyle,
            textAlignment: guitStyle.TextAlignmentStyle,
            textBorder: guitStyle.TextBorderStyle,
            layer: 0)
        { }

        public GUITextBlockToggle(Vector2 position, Vector2 dimensions, String text, SpriteFont font, Color blockColor, Color textColor, GUIStyle.TextAlignment textAlignment = GUIStyle.TextAlignment.Left, Vector2 textBorder = default, int layer = 0) : base(position, dimensions, text, font, blockColor, textColor, textAlignment, textBorder, layer)
        {

        }
        public void SetField(Object obj, string field)
        {
            ToggleObject = obj;
            ToggleField = obj.GetType().GetField(field);
            Toggle = (bool)ToggleField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            ToggleObject = obj;
            ToggleProperty = obj.GetType().GetProperty(property);
            Toggle = (bool)ToggleProperty.GetValue(obj);
        }

        protected override void ComputeFontPosition()
        {
            if (Text == null) return;
            Vector2 textDimensions = TextFont.MeasureString(Text);

            FontWrap(ref textDimensions, Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2));

            _fontPosition = TextAlignment switch
            {
                GUIStyle.TextAlignment.Left     => (Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2)) / 2 * Vector2.UnitY + _textBorder * Vector2.UnitX - textDimensions / 2 * Vector2.UnitY,
                GUIStyle.TextAlignment.Center   => (Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2)) / 2 - textDimensions / 2,
                GUIStyle.TextAlignment.Right    => (Dimensions - Vector2.UnitX * (ToggleIndicatorSize + ToggleIndicatorBorder * 2)) * new Vector2(1, 0.5f) - _textBorder * Vector2.UnitX - textDimensions / 2,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, BlockColor);
            guiRenderer.DrawQuad(parentPosition + Position + Dimensions * new Vector2(1, 0.5f) - ToggleIndicatorBorder * Vector2.UnitX - ToggleIndicatorSize * new Vector2(1,0.5f) , Vector2.One * ToggleIndicatorSize, Toggle ? Color.LimeGreen : Color.Red);
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
                Toggle = !Toggle;
                GUIMouseInput.UIWasUsed = true;

                if (ToggleObject != null)
                {
                    ToggleField?.SetValue(ToggleObject, Toggle, BindingFlags.Public, null, null);
                    ToggleProperty?.SetValue(ToggleObject, Toggle);
                }
                else
                {
                    ToggleField?.SetValue(null, Toggle, BindingFlags.Static | BindingFlags.Public, null, null);
                    ToggleProperty?.SetValue(null, Toggle);
                }

            }
        }

    }
    
}