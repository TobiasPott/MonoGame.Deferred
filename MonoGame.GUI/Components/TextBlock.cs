using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GUI
{
    /// <summary>
    /// Just a colored block with a text inside
    /// </summary>
    public class TextBlock : ColorSwatch
    {
        public Color TextColor;
        public SpriteFont TextFont;

        public override Vector2 Dimensions
        {
            get { return _dimensions; }
            set
            {
                _dimensions = value;
                ComputeFontPosition();
            }
        }

        public StringBuilder Text
        {
            get { return _text; }
            set
            {
                _text = value; 
                ComputeFontPosition();
            }
        }

        public TextAlignment TextAlignment
        {
            get { return _textAlignment; }
            set
            {
                _textAlignment = value;
                ComputeFontPosition();
            }
        }

        protected StringBuilder _text;

        protected Vector2 _fontPosition;
        public TextAlignment _textAlignment;
        protected Vector2 _textBorder = new Vector2(10,1);
        private Vector2 _dimensions;

        public TextBlock(GUIStyle style, String text) : this(
            position: Vector2.Zero,
            dimensions: style.Dimensions,
            text: text,
            font: style.TextFont,
            blockColor: style.Color,
            textColor: style.TextColor,
            textAlignment: style.TextButtonAlignment,
            textBorder: style.TextBorder,
            layer: 0,
            alignment: style.Alignment,
            parentDimensions: style.ParentDimensions)
        { }

        /// <summary>
        /// A default colored block with text on top
        /// </summary>
        public TextBlock(Vector2 position, Vector2 dimensions, String text, SpriteFont font, Color blockColor, Color textColor, TextAlignment textAlignment = TextAlignment.Left, Vector2 textBorder = default, int layer = 0, Alignment alignment = Alignment.None, Vector2 parentDimensions = default) : base(position, dimensions, blockColor, layer, alignment, parentDimensions)
        {
            _text = new StringBuilder(text);
            TextColor = textColor;
            TextFont = font;

            _textBorder = textBorder;

            TextAlignment = textAlignment;
        }

        protected virtual void FontWrap(ref Vector2 textDimension, Vector2 blockDimensions)
        {
            float textwidth = textDimension.X;
            float characterwidth = textwidth / _text.Length;
            int charactersperline = (int)((blockDimensions.X - _textBorder.X * 2) / characterwidth);

            int charactersprocessed = _text.Length;
            int lines = 1;
            while (charactersperline < charactersprocessed)
            {
                _text.Insert(charactersperline * lines + lines, '\n');
                charactersprocessed -= charactersperline;
                lines++;
            }

            if (textDimension.Y * lines + 2 * _textBorder.Y > Dimensions.Y)
            {
                _dimensions = new Vector2(_dimensions.X, textDimension.Y * lines + 2 * _textBorder.Y);
            }

            if(charactersperline<_text.Length)
                textDimension = new Vector2(charactersperline * characterwidth, textDimension.Y * lines);
        }

        protected virtual void ComputeFontPosition()
        {
            if (_text == null) return;
            Vector2 textDimension = TextFont.MeasureString(_text);

            //Let's check wrap!

            FontWrap(ref textDimension, Dimensions);

            _fontPosition = _textAlignment switch
            {
                TextAlignment.Left => Dimensions * 0.5f * Vector2.UnitY + _textBorder * Vector2.UnitX - textDimension * 0.5f * Vector2.UnitY,
                TextAlignment.Center => Dimensions * 0.5f - textDimension * 0.5f,
                TextAlignment.Right => Dimensions * new Vector2(1, 0.5f) - _textBorder * Vector2.UnitX - textDimension * 0.5f,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, SwatchColor);
            guiRenderer.DrawText(parentPosition + Position + _fontPosition, _text, TextFont, TextColor);
        }
        
    }
}