using Microsoft.Xna.Framework;

namespace MonoGame.GUI
{
    //todo:
    //Sort by layer to see which UIClick is the active one (only on top!)

    public class Canvas : GUIElement
    {
        public bool IsEnabled = true;

        private readonly List<GUIElement> _children = new List<GUIElement>();

        public Canvas(Vector2 position, Vector2 dimensions, int layer = 0, Alignment alignment = Alignment.None, Vector2 ParentDimensions = default)
        {
            Dimensions = dimensions;
            Alignment = alignment;
            Position = position;
            OffsetPosition = position;
            Layer = layer;
            if (Alignment != Alignment.None)
            {
                ParentResized(ParentDimensions);
            }
        }

        //Draw the GUI, cycle through the children
        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            if (!IsEnabled) return;
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                if (child.IsHidden) continue;
                child.Draw(guiRenderer, parentPosition + Position, mousePosition);
            }
        }

        public void Resize(Vector2 dimensions)
        {
            Dimensions = dimensions;
            ParentResized(Dimensions);
        }

        //Adjust things when resized
        public override void ParentResized(Vector2 parentDimensions)
        {
            Position = UpdateAlignment(Alignment, parentDimensions, Dimensions, Position, OffsetPosition);

            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                if (child.IsHidden) continue;
                child.ParentResized(Dimensions);
            }

        }

        //If the parent resized then our alignemnt may have changed and we need new position coordinates
        public static Vector2 UpdateAlignment(Alignment alignment, Vector2 parentDimensions, Vector2 dimensions, Vector2 position, Vector2 offsetPosition)
        {
            if (parentDimensions == Vector2.Zero) throw new NotImplementedException();

            switch (alignment)
            {
                case Alignment.None:
                    break;
                case Alignment.TopLeft:
                    position.X = 0;
                    position.Y = 0;
                    break;
                case Alignment.TopMiddle:
                    position.X = parentDimensions.X / 2 - dimensions.X / 2;
                    position.Y = 0;
                    break;
                case Alignment.TopRight:
                    position.X = parentDimensions.X - dimensions.X;
                    position.Y = 0;
                    break;
                case Alignment.BottomLeft:
                    position.X = 0;
                    position.Y = parentDimensions.Y - dimensions.Y;
                    break;
                case Alignment.BottomMiddle:
                    position.X = parentDimensions.X / 2 - dimensions.X / 2;
                    position.Y = parentDimensions.Y - dimensions.Y;
                    break;
                case Alignment.BottomRight:
                    position = parentDimensions - dimensions;
                    break;
                case Alignment.CenterLeft:
                    position.X = 0;
                    position.Y = parentDimensions.Y / 2 - dimensions.Y / 2;
                    break;
                case Alignment.Center:
                    position = parentDimensions / 2 - dimensions / 2;
                    break;
                case Alignment.CenterRight:
                    position.X = parentDimensions.X - dimensions.X;
                    position.Y = parentDimensions.Y / 2 - dimensions.Y / 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return position + offsetPosition;
        }

        public void AddElement(GUIElement element)
        {
            //In Order
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].Layer > element.Layer)
                {
                    _children.Insert(i, element);
                    return;
                }
            }

            _children.Add(element);
        }

        public override int Layer { get; set; }

        /// <summary>
        /// Update our logic
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="mousePosition"></param>
        /// <param name="parentPosition"></param>
        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (!IsEnabled) return;
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                child.Update(gameTime, mousePosition, parentPosition + Position);
            }
        }

        public override Alignment Alignment { get; set; }
    }
}
