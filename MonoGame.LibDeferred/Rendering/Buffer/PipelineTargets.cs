using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{

    public class PipelineTargets : DynamicMultiRenderTarget
    {
        #region Index Constants
        /// <summary>
        /// Index to the output render target
        /// </summary>
        public const int FINALCOLOR = 0;
        /// <summary>
        /// Index to the composition render target
        /// </summary>
        public const int SWAP_HALF = 1;
        /// <summary>
        /// Index to the swap render target
        /// </summary>
        public const int SWAP = 2;

        #endregion

        internal static readonly RenderTarget2DDefinition[] Definitions = new RenderTarget2DDefinition[]
        {
            RenderTarget2DDefinition.Aux_FinalColor, RenderTarget2DDefinition.Aux_SwapHalf,
            RenderTarget2DDefinition.Aux_Swap
        };

        public PipelineTargets(GraphicsDevice graphicsDevice, Vector2 resolution)
            : base(graphicsDevice, (int)resolution.X, (int)resolution.Y, Definitions)
        { }
        public PipelineTargets(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height, Definitions)
        { }
    }

}

