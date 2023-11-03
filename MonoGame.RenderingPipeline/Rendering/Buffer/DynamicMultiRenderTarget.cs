using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{


    public class DynamicMultiRenderTarget : MultiRenderTargetBase
    {
        // ToDo: @tpott: Function: Consider adding resize and modification functionality
        public DynamicMultiRenderTarget(GraphicsDevice graphicsDevice, int width, int height, RenderTarget2DDefinition[] definitions)
            : base(graphicsDevice, width, height, definitions)
        { }


    }



}

