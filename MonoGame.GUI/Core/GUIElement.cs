﻿using Microsoft.Xna.Framework;

namespace MonoGame.GUI
{
    public abstract class GUIElement
    {
        public Vector2 Position;
        public Vector2 OffsetPosition;
        public Vector2 ParentDimensions;
        public bool IsHidden;

        public virtual Vector2 Dimensions { get; set; }
        public abstract void Draw(GUIRenderer renderer, Vector2 parentPosition, Vector2 mousePosition);
        public abstract void ParentResized(Vector2 dimensions);
        public abstract int Layer { get; set; }
        public abstract void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition);
        public abstract Alignment Alignment { get; set; }
    }
}