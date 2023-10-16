using DeferredEngine.Logic;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeferredEngine.Renderer.RenderModules
{
    public class EditorRender
    {

        private double _mouseMoved;
        public bool HasMouseMovement { get; protected set; }
        private readonly double mouseMoveTimer = 400;


        public void Update(GameTime gameTime)
        {
            if (RenderingStats.UIIsHovered || Input.mouseState.RightButton == ButtonState.Pressed)
            {
                HasMouseMovement = false;
                return;
            }
            if (Input.mouseState != Input.mouseLastState)
            {
                //reset the timer!
                _mouseMoved = gameTime.TotalGameTime.TotalMilliseconds + mouseMoveTimer;
                HasMouseMovement = true;
            }
            if (_mouseMoved < gameTime.TotalGameTime.TotalMilliseconds)
            {
                HasMouseMovement = false;
            }

        }

    }

}
