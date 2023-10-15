using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer
{
    public static class MRT
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
        /// <summary>
        /// Index to the bloom render target
        /// </summary>
        public const int BLOOM = 3;

        /// <summary>
        /// Index to the first temporal anti-aliasing render target
        /// </summary>
        public const int SSFX_TAA_1 = 4;
        /// <summary>
        /// Index to the second temporal anti-aliasing render target
        /// </summary>
        public const int SSFX_TAA_2 = 5;

        /// <summary>
        /// Index to the reflection render target
        /// </summary>
        public const int SSFX_REFLECTION = 6;
        /// <summary>
        /// Index to the screen space ambient occlusion render target
        /// </summary>
        public const int SSFX_AMBIENTOCCLUSION = 7;

        /// <summary>
        /// Index to the horizontal blur render target
        /// </summary>
        public const int SSFX_BLUR_HORIZONTAL = 8;
        /// <summary>
        /// Index to the vertical blur render target
        /// </summary>
        public const int SSFX_BLUR_VERTICAL = 9;
        /// <summary>
        /// Index to the final blur render target
        /// </summary>
        public const int SSFX_BLUR_FINAL = 10;

        #endregion


        internal static readonly RenderTarget2DDefinition[] PipelineDefinitions = new RenderTarget2DDefinition[]
        {
            RenderTarget2DDefinition.Aux_Output, RenderTarget2DDefinition.Aux_Compose,
            RenderTarget2DDefinition.Aux_Decal, RenderTarget2DDefinition.SSFx_Bloom,
            RenderTarget2DDefinition.SSFx_TAA_First, RenderTarget2DDefinition.SSFx_TAA_Second,
            RenderTarget2DDefinition.SSFx_Reflections, RenderTarget2DDefinition.SSFx_AmbientOcclusion,
            RenderTarget2DDefinition.SSFx_Blur_Vertical, RenderTarget2DDefinition.SSFx_Blur_Horizontal,
            RenderTarget2DDefinition.SSFx_Blur_Final,
        };
        public class PipelineTargets : DynamicMultiRenderTarget
        {
            public PipelineTargets(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height, MRT.PipelineDefinitions)
            { }
        }

    }

}

