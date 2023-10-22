using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    public class SSAOTargets : MultiRenderTargetBase
    {

        ///// <summary>
        ///// Index to the screen space ambient occlusion render target
        ///// </summary>
        public const int MAIN = 0;
        ///// <summary>
        ///// Index to the vertical blur render target
        ///// </summary>
        public const int BLUR_V = 1;
        ///// <summary>
        ///// Index to the horizontal blur render target
        ///// </summary>
        public const int BLUR_H = 2;
        ///// <summary>
        ///// Index to the final blur render target
        ///// </summary>
        public const int BLUR_FINAL = 3;


        private static readonly RenderTarget2DDefinition[] Definitions = new RenderTarget2DDefinition[4] {
            RenderTarget2DDefinition.SSFx_AmbientOcclusion,
            RenderTarget2DDefinition.SSFx_AO_Blur_Vertical,
            RenderTarget2DDefinition.SSFx_AO_Blur_Horizontal,
            RenderTarget2DDefinition.SSFx_AO_Blur_Final,
        };


        public RenderTarget2D Main => _renderTargets[MAIN];
        public RenderTarget2D Blur_V => _renderTargets[BLUR_V];
        public RenderTarget2D Blur_H => _renderTargets[BLUR_H];
        public RenderTarget2D Blur_Final => _renderTargets[BLUR_FINAL];

        public SSAOTargets(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height, Definitions.Length)
        { }

        public override void Resize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                base.Resize(width, height);
                for (int i = 0; i < 4; i++)
                {
                    _renderTargets[i] = Definitions[i].CreateRenderTarget(_graphicsDevice, _width, _height);
                    _bindings[i] = new RenderTargetBinding(_renderTargets[i]);
                }

            }
        }

    }

}

