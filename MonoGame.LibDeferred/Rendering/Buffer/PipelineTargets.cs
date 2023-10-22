using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{

    public class PipelineTargets : DynamicMultiRenderTarget
    {
        #region Index Constants
        /// <summary>
        /// Index to the output render target
        /// </summary>
        public const int OUTPUT = 0;
        /// <summary>
        /// Index to the composition render target
        /// </summary>
        public const int COMPOSE = 1;
        /// <summary>
        /// Index to the decal render target
        /// </summary>
        public const int DECAL = 2;

        #endregion

        internal static readonly RenderTarget2DDefinition[] PipelineDefinitions = new RenderTarget2DDefinition[]
        {
            RenderTarget2DDefinition.Aux_Output, RenderTarget2DDefinition.Aux_Compose,
            RenderTarget2DDefinition.Aux_Decal,
        };

        public PipelineTargets(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height, PipelineDefinitions)
        { }
    }

}

