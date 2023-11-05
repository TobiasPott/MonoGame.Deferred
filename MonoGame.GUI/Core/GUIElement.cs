using Microsoft.Xna.Framework;

namespace MonoGame.GUI
{
    public abstract class GUIElement
    {
        public Vector2 Position;
        public Vector2 OffsetPosition;
        public Vector2 ParentDimensions;
        public bool IsHidden;
        public bool IsEnabled = true;

        public abstract Alignment Alignment { get; set; }
        public virtual Vector2 Dimensions { get; set; }
        public abstract int Layer { get; set; }

        public abstract void ParentResized(Vector2 dimensions);
        public abstract void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition);
        public abstract void Draw(GUIRenderer renderer, Vector2 parentPosition, Vector2 mousePosition);
    }
}