using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GUI
{

    // ToDo: UI: Is this acutally more like an 'anchor'?
    public enum Alignment
    {
        None,
        TopLeft,
        TopMiddle,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomMiddle,
        BottomRight,
    }

    public enum TextAlignment
    {
        Left, Center, Right
    }


    public class GUIStyle
    {
        public Vector2 Dimensions;
        public Color Color;
        public Color TextColor;
        public Color SliderColor;
        public SpriteFont TextFont;
        public Alignment Alignment;
        public TextAlignment TextAlignment;
        public TextAlignment TextButtonAlignment;
        public Vector2 ParentDimensions;
        public Vector2 TextBorder;

        public GUIStyle(Vector2 dimensions, SpriteFont textFont, Color blockColor, Color textColor, Color sliderColor, 
            Alignment alignment, TextAlignment textAlignment, TextAlignment textButtonAlignment, 
            Vector2 textBorder, Vector2 parentDimensions)
        {
            Dimensions = dimensions;
            TextFont = textFont;
            Color = blockColor;
            TextColor = textColor;
            Alignment = alignment;
            TextAlignment = textAlignment;
            ParentDimensions = parentDimensions;
            TextButtonAlignment = textButtonAlignment;
            TextBorder = textBorder;
            SliderColor = sliderColor;
        }

    }
}
