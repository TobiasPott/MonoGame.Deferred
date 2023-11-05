﻿using Microsoft.Xna.Framework;
using MonoGame.GUIHelper;

namespace MonoGame.GUI
{
    public class ListToggleScroll : ListToggle
    {
        protected bool IsEngaged = false;

        protected float Scrollwidth = 20;

        protected Color ScrollBarDisabledColor = Color.DarkGray;
        protected Color ScrollBarEnabledColor = Color.DarkGray;

        protected Color SliderColor = Color.DimGray;
        protected Color SliderHoveredColor = HoverColor;

        protected bool ScrollBarEnabled = false;
        protected bool ScrollBarHovered = false;

        protected float ScrollTranslation = 0;

        protected float ScrollTotalHeight = 0;

        protected float ListHeight = 0;

        protected float percentScroll = 0;

        protected float percentOverscroll = 1;

        protected float SliderPosition = 0.5f;
        protected float SliderHeight = 10;

        public ListToggleScroll(Vector2 position, GUIStyle style) : this(
            position: position,
            defaultDimensions: style.Dimensions,
            layer: 0,
            alignment: style.Alignment,
            ParentDimensions: style.ParentDimensions)
        { }

        public ListToggleScroll(Vector2 position, Vector2 defaultDimensions, int layer = 0, Alignment alignment = Alignment.None, Vector2 ParentDimensions = new Vector2()) : base(position, defaultDimensions, layer, alignment, ParentDimensions)
        {
            this.ParentDimensions = ParentDimensions;
        }

        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (IsHidden) return;

            if (IsToggled)
            {
                float height = ToggleButtonHeight;
                for (int index = 0; index < _children.Count; index++)
                {
                    GUIElement child = _children[index];

                    if (child.IsHidden) continue;
                    child.Update(gameTime, mousePosition, parentPosition + Position + height * Vector2.UnitY + +ScrollTranslation * Vector2.UnitY);

                    height += _children[index].Dimensions.Y;
                }

                if (Math.Abs(Dimensions.Y - height) > 0.01f)
                    Dimensions = new Vector2(Dimensions.X, height);
            }
            else
            {
                Dimensions = _toggleDimensions;
            }

            //If this element is not engaged but some other one is, return.
            if (!IsEngaged && GUIMouseInput.UIElementEngaged) return;

            Vector2 bound1 = Position + parentPosition + _toggleDimensions * Vector2.UnitX;
            Vector2 bound2 = bound1 + new Vector2(Scrollwidth, ScrollTotalHeight);

            if (ScrollBarEnabled)
            {
                if (!IsEngaged)
                {
                    ScrollBarHovered = false;

                    if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                        mousePosition.Y < bound2.Y)
                    {
                        ScrollBarHovered = true;

                        if (GUIMouseInput.IsLMBPressed())
                        {
                            GUIMouseInput.UIWasUsed = true;

                            GUIMouseInput.UIElementEngaged = true;
                            IsEngaged = true;

                            // to percent
                            percentScroll = (mousePosition.Y - bound1.Y) / (bound2.Y - bound1.Y);

                            //Now get clamp to slider size
                            float minClamp = 1 / percentOverscroll / 2;
                            percentScroll = (MathHelper.Clamp(percentScroll, minClamp, 1 - minClamp) - minClamp) /
                                            (1 - minClamp * 2);

                            ScrollTranslation = -percentScroll * (ListHeight - ScrollTotalHeight);

                        }
                    }
                }
                else
                {
                    //Break engagement
                    if (!GUIMouseInput.IsLMBPressed())
                    {
                        GUIMouseInput.UIElementEngaged = false;
                        IsEngaged = false;
                    }
                    else
                    {
                        GUIMouseInput.UIWasUsed = true;
                        ScrollBarHovered = true;

                        percentScroll = (MathHelper.Clamp(mousePosition.Y, bound1.Y, bound2.Y) - bound1.Y) / (bound2.Y - bound1.Y);

                        //Now get clamp to slider size
                        float minClamp = 1 / percentOverscroll / 2;
                        percentScroll = (MathHelper.Clamp(percentScroll, minClamp, 1 - minClamp) - minClamp) /
                                        (1 - minClamp * 2);

                        ScrollTranslation = -percentScroll * (ListHeight - ScrollTotalHeight);
                    }
                }
            }

            if (IsEngaged) return;
            //Toggle

            _isHovered = false;

            bound1 = Position + parentPosition;
            bound2 = bound1 + _toggleDimensions;

            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                mousePosition.Y < bound2.Y)
            {
                _isHovered = true;
                if (GUIMouseInput.WasLMBClicked())
                {
                    GUIMouseInput.UIWasUsed = true;

                    IsToggled = !IsToggled;
                }
            }

            //Scrollbar
        }

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            if (IsHidden) return;

            Vector2 initialPosition = parentPosition + Position + ScrollTranslation * Vector2.UnitY;
            //Draw toggle element
            guiRenderer.DrawQuad(initialPosition, _toggleDimensions, _isHovered ? HoverColor : Color.DimGray);

            //arrow
            if (IsToggled)
            {
                guiRenderer.DrawQuad(
                    initialPosition + _toggleDimensions * 0.5f - ArrowButtonHeight * Vector2.One * 0.5f,
                    new Vector2(ArrowButtonHeight, ArrowButtonHeight * 0.25f), Color.White);
                guiRenderer.DrawQuad(
                    initialPosition + _toggleDimensions * 0.5f - ArrowButtonHeight * Vector2.UnitX * 0.25f,
                    new Vector2(ArrowButtonHeight, ArrowButtonHeight * 0.5f) * 0.5f, Color.White);
            }
            else
            {
                guiRenderer.DrawQuad(
                    initialPosition + _toggleDimensions * 0.5f - ArrowButtonHeight * Vector2.UnitX * 0.5f,
                    new Vector2(ArrowButtonHeight, ArrowButtonHeight * 0.25f), Color.White);
                guiRenderer.DrawQuad(
                    initialPosition + _toggleDimensions * 0.5f - ArrowButtonHeight * new Vector2(0.25f, 0.5f),
                    new Vector2(ArrowButtonHeight, ArrowButtonHeight * 0.5f) * 0.5f, Color.White);
            }

            if (IsToggled)
            {
                float height = ToggleButtonHeight;

                for (int index = 0; index < _children.Count; index++)
                {
                    GUIElement child = _children[index];

                    if (child.IsHidden) continue;

                    if (ScrollTranslation + height < ParentDimensions.Y)
                    {
                        child.Draw(guiRenderer, initialPosition + height * Vector2.UnitY, mousePosition);
                    }
                    height += _children[index].Dimensions.Y;
                }

                ScrollBarEnabled = height > ParentDimensions.Y;

                if (ScrollBarEnabled)
                {
                    ScrollTotalHeight = MathHelper.Min(height, ParentDimensions.Y);
                    ListHeight = height;

                    percentOverscroll = MathHelper.Max(1, ListHeight / ScrollTotalHeight);

                    SliderHeight = ScrollTotalHeight / percentOverscroll;
                }
                else
                {
                    ScrollTotalHeight = height;
                    ScrollTranslation = 0;
                }
                //Sidebar
                guiRenderer.DrawQuad(parentPosition + Position + _toggleDimensions * Vector2.UnitX, new Vector2(Scrollwidth, ScrollTotalHeight), ScrollBarEnabled ? ScrollBarEnabledColor : ScrollBarDisabledColor);

                //Scrollbar
                if (ScrollBarEnabled)
                    guiRenderer.DrawQuad(parentPosition + Position + _toggleDimensions * Vector2.UnitX + percentScroll * (ScrollTotalHeight - SliderHeight) * Vector2.UnitY, new Vector2(Scrollwidth, SliderHeight), ScrollBarHovered ? SliderHoveredColor : SliderColor);

            }

        }
    }
}