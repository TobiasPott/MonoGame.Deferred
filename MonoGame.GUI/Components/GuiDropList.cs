using MonoGame.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace MonoGame.GUI
{
    public class GuiDropList : TextBlock
    {
        public bool Toggle;

        private static readonly float ButtonBorder = 2;

        private static readonly Color HoverColor = Color.LightGray;

        private Vector2 _declarationTextDimensions;

        private bool _isHovered;

        private bool _isToggled = false;

        private Vector2 _baseDimensions;

        //Load
        private readonly StringBuilder _selectedOptionName = new StringBuilder(100);

        public GuiDropList(GUIStyle style, string text) : this(
            position: Vector2.Zero,
            dimensions: style.Dimensions,
            text: text,
            font: style.TextFont,
            blockColor: style.Color,
            textColor: style.TextColor,
            textAlignment: TextAlignment.Left,
            textBorder: style.TextBorder,
            layer: 0
            )
        {
        }

        public GuiDropList(Vector2 position, Vector2 dimensions, string text, SpriteFont font, Color blockColor, Color textColor, TextAlignment textAlignment = TextAlignment.Center, Vector2 textBorder = default, int layer = 0) : base(position, dimensions, text, font, blockColor, textColor, textAlignment, textBorder, layer)
        {
            _selectedOptionName.Append("...");

            _baseDimensions = Dimensions;

            throw new NotImplementedException();
        }

        protected override void ComputeFontPosition()
        {
            if (_text == null) return;
            _declarationTextDimensions = TextFont.MeasureString(_text);

            //Let's check wrap!

            //FontWrap(ref textDimension, Dimensions);

            _fontPosition = Dimensions * 0.5f * Vector2.UnitY + _textBorder * Vector2.UnitX - _declarationTextDimensions * 0.5f * Vector2.UnitY;
        }

        protected void ComputeObjectNameLength()
        {
            if (_selectedOptionName.Length > 0)
            {
                //Max length
                Vector2 textDimensions = TextFont.MeasureString(_selectedOptionName);

                float characterLength = textDimensions.X / _selectedOptionName.Length;

                Vector2 buttonLeft = (_declarationTextDimensions + _fontPosition * 1.5f) * Vector2.UnitX;
                Vector2 spaceAvailable = Dimensions - 2 * Vector2.One * ButtonBorder - buttonLeft -
                                         (2 + _textBorder.X) * Vector2.UnitX;

                int characters = (int)(spaceAvailable.X / characterLength);

                _selectedOptionName.Length = characters < _selectedOptionName.Length ? characters : _selectedOptionName.Length;
            }
        }


        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            Vector2 buttonLeft = (_declarationTextDimensions + _fontPosition * 1.2f) * Vector2.UnitX;
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, SwatchColor);
            guiRenderer.DrawQuad(parentPosition + Position + buttonLeft + Vector2.One * ButtonBorder, Dimensions - 2 * Vector2.One * ButtonBorder - buttonLeft - (2 + _textBorder.X) * Vector2.UnitX, _isHovered ? HoverColor : Color.DimGray);

            guiRenderer.DrawText(parentPosition + Position + _fontPosition, Text, TextFont, TextColor);

            //Description
            guiRenderer.DrawText(parentPosition + Position + buttonLeft + new Vector2(4, _fontPosition.Y), _selectedOptionName, TextFont, TextColor);

        }

        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            _isHovered = false;

            Vector2 bound1 = Position + parentPosition;
            Vector2 bound2 = bound1 + Dimensions;

            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                mousePosition.Y < bound2.Y)
            {
                _isHovered = true;

                if (!GUIMouseInput.WasLMBClicked()) return;

                _isToggled = !_isToggled;
                Dimensions = new Vector2(_baseDimensions.X, _baseDimensions.Y + (_isToggled ? 100 : 0));

                GUIMouseInput.UIWasUsed = true;
            }
        }

    }

}