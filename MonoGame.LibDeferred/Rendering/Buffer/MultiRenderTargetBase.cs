using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeferredEngine.Renderer
{
    public abstract class MultiRenderTargetBase : IDisposable
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
        public static readonly RenderTarget2DDefinition SSFx_Blur_Horizontal = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition SSFx_Blur_Final = new RenderTarget2DDefinition(false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

        public static readonly RenderTarget2DDefinition SSFx_Reflections = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        public static readonly RenderTarget2DDefinition SSFx_AmbientOcclusion = new RenderTarget2DDefinition(false, SurfaceFormat.HalfSingle, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        protected GraphicsDevice _graphicsDevice;
        protected int _width;
        protected int _height;
        protected readonly RenderTargetBinding[] _bindings;
        protected readonly RenderTarget2D[] _renderTargets;

        public RenderTargetBinding[] Bindings => _bindings;
        public RenderTarget2D[] RenderTargets => _renderTargets;

        public RenderTarget2D this[int index] => _renderTargets[index];

        public MultiRenderTargetBase(GraphicsDevice graphicsDevice, int width, int height, int numberOfTargets)
        {
            _graphicsDevice = graphicsDevice;
            _bindings = new RenderTargetBinding[numberOfTargets];
            _renderTargets = new RenderTarget2D[numberOfTargets];
            this.Resize(width, height);
        }

        public virtual void Resize(int width, int height)
        {
            _width = width;
            _height = height;
        }


        public virtual void Dispose()
        {
            foreach (RenderTarget2D rt in _renderTargets)
            {
                rt?.Dispose();
            }
        }
    }

}

