using Microsoft.Xna.Framework;

namespace MonoGame.GUI
{
    public class GUIList : GUIElement
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

        public GUIList(Vector2 position, GUIStyle style) : this(
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
        /// <param name="position"></param>
        /// <param name="defaultDimensions"></param>
        /// <param name="layer"></param>
        /// <param name="alignment"></param>
        /// <param name="parentDimensions"></param>
        public GUIList(Vector2 position, Vector2 defaultDimensions, int layer = 0, Alignment alignment = Alignment.None, Vector2 parentDimensions = default)
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

            float height = 0;

            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];

                if (child.IsHidden) continue;

                child.Draw(guiRenderer, parentPosition + Position + height * Vector2.UnitY, mousePosition);

                height += _children[index].Dimensions.Y;
            }
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
            //element.Position = new Vector2(0, _children.Count*DefaultDimensions.Y);
            //element.Dimensions = DefaultDimensions;


            // I think it is acceptable to make a for loop everytime an element is added
            float height = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                height += _children[i].Dimensions.Y;
            }
            //element.Position = new Vector2(0,height);

            Dimensions = new Vector2(DefaultDimensions.X, height + element.Dimensions.Y);

            //In Order
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
            if (IsHidden)
            {
                return;
            }

            float height = 0;
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];

                if (child.IsHidden) continue;

                child.Update(gameTime, mousePosition, parentPosition + Position + height * Vector2.UnitY);

                height += _children[index].Dimensions.Y;

            }
            if (Math.Abs(Dimensions.Y - height) > 0.01f)
                Dimensions = new Vector2(Dimensions.X, height);
        }

    }
}