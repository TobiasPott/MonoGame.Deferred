using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer
{


    public class DynamicMultiRenderTarget : MultiRenderTargetBase
    {
        // ToDo: not yet really dynamic but didn't want to use Auxiliary in naming as it is mostly unclear and implies no intention or feature set
        public DynamicMultiRenderTarget(GraphicsDevice graphicsDevice, int width, int height, RenderTarget2DDefinition[] definitions)
            : base(graphicsDevice, width, height, definitions.Length)
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                _renderTargets[i]?.Dispose();
                _renderTargets[i] = definitions[i].CreateRenderTarget(_graphicsDevice, _width, _height);
                _bindings[i] = new RenderTargetBinding(_renderTargets[i]);
            }
        }


    }



}

