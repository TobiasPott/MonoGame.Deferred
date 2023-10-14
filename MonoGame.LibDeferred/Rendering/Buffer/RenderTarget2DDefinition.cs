using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{

    public enum ResamplingModes
    {
        Downsample_x2 = -2,
        Downsample_x1 = -1,
        Original = 0,
        Upsample_x1 = 1,
        Upsample_x2 = 2
    }

    public struct RenderTarget2DDefinition
    {
        public static readonly RenderTarget2DDefinition Albedo = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Normal = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Depth = new RenderTarget2DDefinition(false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition Aux_Output = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Aux_Compose = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Aux_Decal = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_Bloom = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_TAA_First = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition SSFx_TAA_Second = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_Blur_Vertical = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        // ToDo: Does the horizontal blur and final blur render target really needs the depth channel?
        // Half Size Targets (ToDo: Needs extension of the definition type to include super and sub sizing)
        //          blur final, blur horizontal, ambient occlusion
        public static readonly RenderTarget2DDefinition SSFx_Blur_Horizontal = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, ResamplingModes.Downsample_x1);
        public static readonly RenderTarget2DDefinition SSFx_Blur_Final = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, ResamplingModes.Downsample_x1);

        public static readonly RenderTarget2DDefinition SSFx_Reflections = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition SSFx_AmbientOcclusion = new RenderTarget2DDefinition(false, SurfaceFormat.HalfSingle, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, ResamplingModes.Downsample_x1);



        public readonly bool MipMap;
        public readonly SurfaceFormat Format;
        public readonly DepthFormat DepthFormat;
        public readonly int MultiSampleCount;
        public readonly RenderTargetUsage Usage;
        public readonly ResamplingModes Resampling;


        public RenderTarget2DDefinition(bool mipMap, SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount, RenderTargetUsage usage, ResamplingModes resModifier = ResamplingModes.Original)
        {
            this.MipMap = mipMap;
            this.Format = format;
            this.DepthFormat = depthFormat;
            this.MultiSampleCount = multiSampleCount;
            this.Usage = usage;
            this.Resampling = resModifier;
        }

        public RenderTarget2D CreateRenderTarget(GraphicsDevice graphicsDevice, int width, int height)
        {
            if (this.Resampling != ResamplingModes.Original)
                this.Resample(this.Resampling, ref width, ref height);
            Debug.WriteLine($"{width}x{height}");
            return new RenderTarget2D(graphicsDevice, width, height, MipMap, Format, DepthFormat, MultiSampleCount, Usage);
        }

        private void Resample(ResamplingModes resampling, ref int width, ref int height)
        {
            if (resampling == ResamplingModes.Downsample_x1)
            {
                width /= 2; height /= 2;
            }
            else if (resampling == ResamplingModes.Downsample_x2)
            {
                width /= 4; height /= 4;
            }
            else if (resampling == ResamplingModes.Upsample_x1)
            {
                width *= 2; height *= 2;
            }
            else if (resampling == ResamplingModes.Upsample_x2)
            {
                width *= 4; height *= 4;
            }
        }

    }

}

