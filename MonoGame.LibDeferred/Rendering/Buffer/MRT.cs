using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
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

        ///// <summary>
        ///// Index to the first temporal anti-aliasing render target
        ///// </summary>
        //public const int SSFX_TAA_1 = 4;
        ///// <summary>
        ///// Index to the second temporal anti-aliasing render target
        ///// </summary>
        //public const int SSFX_TAA_2 = 5;

        ///// <summary>
        ///// Index to the reflection render target
        ///// </summary>
        //public const int SSFX_REFLECTION = 6;


        #endregion


        internal static readonly RenderTarget2DDefinition[] PipelineDefinitions = new RenderTarget2DDefinition[]
        {
            RenderTarget2DDefinition.Aux_Output, RenderTarget2DDefinition.Aux_Compose,
            RenderTarget2DDefinition.Aux_Decal, RenderTarget2DDefinition.SSFx_Bloom,
            //RenderTarget2DDefinition.SSFx_TAA_Even, RenderTarget2DDefinition.SSFx_TAA_Odd,
        };
        public class PipelineTargets : DynamicMultiRenderTarget
        {
            public PipelineTargets(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height, MRT.PipelineDefinitions)
            { }

            //public void GetTemporalAARenderTargets(bool isOffFrame, out RenderTarget2D destRT, out RenderTarget2D previousRT)
            //{
            //    destRT = !isOffFrame ? this[MRT.SSFX_TAA_1] : this[MRT.SSFX_TAA_2];
            //    previousRT = isOffFrame ? this[MRT.SSFX_TAA_1] : this[MRT.SSFX_TAA_2];
            //}
            //public RenderTarget2D GetSSReflectionRenderTargets(bool isTaaEnabled, bool isOffFrame)
            //{
            //    if (isTaaEnabled)
            //        return isOffFrame ? this[MRT.SSFX_TAA_1] : this[MRT.SSFX_TAA_2];
            //    else
            //        return this[MRT.COMPOSE];
            //}
        }



    }

}

