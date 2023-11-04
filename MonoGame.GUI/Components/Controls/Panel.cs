using Microsoft.Xna.Framework;

namespace MonoGame.GUI
{
    public class Panel : GUIElement
    {
        public Vector2 DefaultDimensions;

        protected List<GUIElement> _children = new List<GUIElement>();

        private Alignment _alignment;
        public override Alignment Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value;
                if (value != Alignment.None)
                {
                    ParentResized(ParentDimensions);
                }
            }
        }

        public Panel(Vector2 position, GUIStyle style) : this(
            position: position,
            defaultDimensions: style.Dimensions,
            layer: 0,
            alignment: style.Alignment,
            parentDimensions: style.ParentDimensions)
        {

        }

        /// <summary>
        /// A list has a unified width/height of the elements. Each element is rendered below the other one
        /// </summary>
        public Panel(Vector2 position, Vector2 defaultDimensions, int layer = 0, Alignment alignment = Alignment.None, Vector2 parentDimensions = default)
        {
            DefaultDimensions = defaultDimensions;
            ParentDimensions = parentDimensions;
            Position = position;
            OffsetPosition = position;
            Layer = layer;
            Alignment = alignment;
        }

        //Draw the GUI, cycle through the children
        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            if (IsHidden) return;

            foreach(GUIElement child in _children.Where(x => !x.IsHidden))
                child.Draw(guiRenderer, parentPosition + Position, mousePosition);
        }

        //Adjust things when resized
        public override void ParentResized(Vector2 parentDimensions)
        {
            //for (int index = 0; index < _children.Count; index++)
            //{
            //    GUIElement child = _children[index];
            //    child.ParentResized(ElementDimensions);
            //}

            Position = Canvas.UpdateAlignment(Alignment, parentDimensions, Dimensions, Position, OffsetPosition);
        }


        public virtual void AddElement(GUIElement element)
        {
            //In Order
            _children.Add(element);
        }

        public override int Layer { get; set; }


        /// <summary>
        /// Update our logic
        /// </summary>
        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (IsHidden)
                return;

            foreach (GUIElement child in _children.Where(x => !x.IsHidden))
                child.Update(gameTime, mousePosition, parentPosition + Position);

        }

    }
}