using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Rendering
{

    public enum ResamplingModes
    {
        Downsample_x5 = -32,
        Downsample_x4 = -16,
        Downsample_x3 = -8,
        Downsample_x2 = -4,
        Downsample_x1 = -2,
        Original = 0,
        Upsample_x1 = 2,
        Upsample_x2 = 4,
        Upsample_x3 = 8,
        Upsample_x4 = 16,
        Upsample_x5 = 32
    }

    public readonly struct RenderTarget2DDefinition
    {
        public static readonly RenderTarget2DDefinition Albedo = new RenderTarget2DDefinition(nameof(Albedo), false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Normal = new RenderTarget2DDefinition(nameof(Normal), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Depth = new RenderTarget2DDefinition(nameof(Depth), false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition Aux_FinalColor = new RenderTarget2DDefinition(nameof(Aux_FinalColor), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Aux_Swap = new RenderTarget2DDefinition(nameof(Aux_Swap), false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Aux_SwapHalf = new RenderTarget2DDefinition(nameof(Aux_SwapHalf), false, SurfaceFormat.HalfVector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_Bloom = new RenderTarget2DDefinition(nameof(SSFx_Bloom), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_TAA_Even = new RenderTarget2DDefinition(nameof(SSFx_TAA_Even), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition SSFx_TAA_Odd = new RenderTarget2DDefinition(nameof(SSFx_TAA_Odd), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_AO_Main = new RenderTarget2DDefinition(nameof(SSFx_AO_Main), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, ResamplingModes.Downsample_x1);
        public static readonly RenderTarget2DDefinition SSFx_AO_Blur_V = new RenderTarget2DDefinition(nameof(SSFx_AO_Blur_V), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition SSFx_AO_Blur_H = new RenderTarget2DDefinition(nameof(SSFx_AO_Blur_H), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, ResamplingModes.Downsample_x1);
        public static readonly RenderTarget2DDefinition SSFx_AO_Blur_Final = new RenderTarget2DDefinition(nameof(SSFx_AO_Blur_Final), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, ResamplingModes.Downsample_x1);

        public static readonly RenderTarget2DDefinition SSFx_SSR_Main = new RenderTarget2DDefinition(nameof(SSFx_SSR_Main), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);


        public static readonly RenderTarget2DDefinition Aux_Id = new RenderTarget2DDefinition(nameof(Aux_Id), false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
       
        
        public static readonly RenderTarget2DDefinition Shadow_PointLight = new RenderTarget2DDefinition(nameof(Shadow_PointLight), false, SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition Shadow_DirectionalLight = new RenderTarget2DDefinition(nameof(Shadow_DirectionalLight), false, SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);





        public readonly string Id;
        public readonly bool MipMap;
        public readonly SurfaceFormat Format;
        public readonly DepthFormat DepthFormat;
        public readonly int MultiSampleCount;
        public readonly RenderTargetUsage Usage;
        public readonly ResamplingModes Resampling;


        public RenderTarget2DDefinition(string id, bool mipMap, SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount, RenderTargetUsage usage, ResamplingModes resModifier = ResamplingModes.Original)
        {
            this.Id = id;
            this.MipMap = mipMap;
            this.Format = format;
            this.DepthFormat = depthFormat;
            this.MultiSampleCount = multiSampleCount;
            this.Usage = usage;
            this.Resampling = resModifier;
        }

        public readonly RenderTarget2D CreateRenderTarget(GraphicsDevice graphicsDevice, Vector2 resolution) => CreateRenderTarget(graphicsDevice, (int)resolution.X, (int)resolution.Y);
        public readonly RenderTarget2D CreateRenderTarget(GraphicsDevice graphicsDevice, int width, int height)
        {
            if (this.Resampling != ResamplingModes.Original)
                Resample(this.Resampling, ref width, ref height);

            long size = width * height;
            if (this.Format == SurfaceFormat.Color || this.Format == SurfaceFormat.ColorSRgb
                || this.Format == SurfaceFormat.Single)
                size *= 4;
            else if (this.Format == SurfaceFormat.HalfSingle)
                size *= 2;
            else if (this.Format == SurfaceFormat.Vector4)
                size *= 16;
            else if (this.Format == SurfaceFormat.HalfVector4)
                size *= 8;


            if (string.IsNullOrEmpty(Id))
                Debug.WriteLine($"CreateRenderTarget '': {width} x {height} {Format} ({size / 1024.0f / 1024.0f:0.000} MBytes)");
            else
                Debug.WriteLine($"CreateRenderTarget {"'" + Id + "'",-20}: {width} x {height} {Format} ({size / 1024.0f / 1024.0f:0.000} MBytes)");

            return new RenderTarget2D(graphicsDevice, width, height, MipMap, Format, DepthFormat, MultiSampleCount, Usage);
        }

        private static void Resample(ResamplingModes resampling, ref int width, ref int height)
        {
            if (resampling < 0)
            {
                int downsample = Math.Abs((int)resampling);
                width /= downsample; height /= downsample;
            }
            else if (resampling > 0)
            {
                int upsample = Math.Abs((int)resampling);
                width *= upsample; height *= upsample;
            }
        }

    }

}

