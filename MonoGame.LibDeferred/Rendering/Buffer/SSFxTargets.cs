using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    public class SSFxTargets : MultiRenderTargetBase
    {

        ///// <summary>
        ///// Index to the screen space ambient occlusion render target
        ///// </summary>
        public const int AO_MAIN = 0;
        ///// <summary>
        ///// Index to the vertical blur render target
        ///// </summary>
        public const int AO_BLUR_V = 1;
        ///// <summary>
        ///// Index to the horizontal blur render target
        ///// </summary>
        public const int AO_BLUR_H = 2;
        ///// <summary>
        ///// Index to the final blur render target
        ///// </summary>
        public const int AO_BLUR_FINAL = 3;

        /// <summary>
        /// Index to the first temporal anti-aliasing render target
        /// </summary>
        public const int TAA_EVEN = 4;
        /// <summary>
        /// Index to the second temporal anti-aliasing render target
        /// </summary>
        public const int TAA_ODD = 5;

        ///// <summary>
        ///// Index to the reflection render target
        ///// </summary>
        public const int SSR_MAIN = 6;



        private static readonly RenderTarget2DDefinition[] Definitions = new RenderTarget2DDefinition[] {
            RenderTarget2DDefinition.SSFx_AO_Main,
            RenderTarget2DDefinition.SSFx_AO_Blur_V,
            RenderTarget2DDefinition.SSFx_AO_Blur_H,
            RenderTarget2DDefinition.SSFx_AO_Blur_Final,
            RenderTarget2DDefinition.SSFx_TAA_Even, RenderTarget2DDefinition.SSFx_TAA_Odd,
            RenderTarget2DDefinition.SSFx_Reflections,
        };


        public RenderTarget2D AO_Main => _renderTargets[AO_MAIN];
        public RenderTarget2D AO_Blur_V => _renderTargets[AO_BLUR_V];
        public RenderTarget2D AO_Blur_H => _renderTargets[AO_BLUR_H];
        public RenderTarget2D AO_Blur_Final => _renderTargets[AO_BLUR_FINAL];

        public RenderTarget2D TAA_Even => _renderTargets[TAA_EVEN];
        public RenderTarget2D TAA_Odd => _renderTargets[TAA_ODD];

        public RenderTarget2D SSR_Main => _renderTargets[SSR_MAIN];


        public SSFxTargets(GraphicsDevice graphicsDevice, Vector2 resolution)
            : base(graphicsDevice, (int)resolution.X, (int)resolution.Y, Definitions.Length)
        { }
        public SSFxTargets(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height, Definitions.Length)
        { }

        public override void Resize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                base.Resize(width, height);
                for (int i = 0; i < Definitions.Length; i++)
                {
                    _renderTargets[i] = Definitions[i].CreateRenderTarget(_graphicsDevice, _width, _height);
                    _bindings[i] = new RenderTargetBinding(_renderTargets[i]);
                }

            }
        }
        public void GetTemporalAARenderTargets(bool isOffFrame, out RenderTarget2D currentRT, out RenderTarget2D previousRT)
        {
            currentRT = !isOffFrame ? this.TAA_Even : this.TAA_Odd;
            previousRT = isOffFrame ? this.TAA_Even : this.TAA_Odd;
        }
        public RenderTarget2D GetSSReflectionRenderTargets(bool isTaaEnabled, bool isOffFrame)
        {
            if (isTaaEnabled)
                return isOffFrame ? this.TAA_Even : this.TAA_Odd;
            else
                return null;
        }

    }

}

