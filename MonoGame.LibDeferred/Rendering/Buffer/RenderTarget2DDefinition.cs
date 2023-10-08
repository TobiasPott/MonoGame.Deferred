using Microsoft.Xna.Framework.Graphics;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public struct RenderTarget2DDefinition
    {
        public readonly bool MipMap;
        public readonly SurfaceFormat Format;
        public readonly DepthFormat DepthFormat;
        public readonly int MultiSampleCount;
        public readonly RenderTargetUsage Usage;

        public RenderTarget2DDefinition(bool mipMap, SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount, RenderTargetUsage usage)
        {
            this.MipMap = mipMap;
            this.Format = format;
            this.DepthFormat = depthFormat;
            this.MultiSampleCount = multiSampleCount;
            this.Usage = usage;
        }

        public RenderTarget2D CreateRenderTarget(GraphicsDevice graphicsDevice, int width, int height)
        {
            return new RenderTarget2D(graphicsDevice, width, height, MipMap, Format, DepthFormat, MultiSampleCount, Usage);
        }


    }

}

