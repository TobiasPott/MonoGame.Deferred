using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GUI
{

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

    public class GUIStyle
    {
        public Vector2 DimensionsStyle;
        public Color BlockColorStyle;
        public Color TextColorStyle;
        public Color SliderColorStyle;
        public SpriteFont TextFontStyle;
        public Alignment GuiAlignmentStyle;
        public TextAlignment TextAlignmentStyle;
        public TextAlignment TextButtonAlignmentStyle;
        public Vector2 ParentDimensionsStyle;
        public Vector2 TextBorderStyle;

        public GUIStyle(Vector2 dimensionsStyle, SpriteFont textFontStyle, Color blockColorStyle, Color textColorStyle, Color sliderColorStyle, Alignment guiAlignmentStyle, TextAlignment textAlignmentStyle, TextAlignment textButtonAlignmentStyle, Vector2 textBorderStyle, Vector2 parentDimensionsStyle)
        {
            DimensionsStyle = dimensionsStyle;
            TextFontStyle = textFontStyle;
            BlockColorStyle = blockColorStyle;
            TextColorStyle = textColorStyle;
            GuiAlignmentStyle = guiAlignmentStyle;
            TextAlignmentStyle = textAlignmentStyle;
            ParentDimensionsStyle = parentDimensionsStyle;
            TextButtonAlignmentStyle = textButtonAlignmentStyle;
            TextBorderStyle = textBorderStyle;
            SliderColorStyle = sliderColorStyle;
        }

        public enum TextAlignment
        {
            Left, Center, Right
        }


    }
}
