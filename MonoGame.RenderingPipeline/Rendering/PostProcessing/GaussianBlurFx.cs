﻿using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.PostProcessing
{
    [Obsolete($"{nameof(GaussianBlurFx)} is unused and needs refactoring when the rendering pipeline is modularized.")]
    public class GaussianBlurFx : PostFx
    {
        protected override bool GetEnabled() => _enabled;


        private readonly GaussianBlurFxSetup _fxSetup = new GaussianBlurFxSetup();

        private RenderTarget2D _rt2562;
        private RenderTarget2D _rt5122;
        private RenderTarget2D _rt10242;
        private RenderTarget2D _rt20482;


        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FullscreenTriangleBuffer fullScreenTarget)
        {
            base.Initialize(graphicsDevice, spriteBatch, fullScreenTarget);

            _rt2562 = new RenderTarget2D(graphicsDevice, 256, 256, false, SurfaceFormat.Vector2, DepthFormat.None);
            _rt5122 = new RenderTarget2D(graphicsDevice, 512, 512, false, SurfaceFormat.Vector2, DepthFormat.None);
            _rt10242 = new RenderTarget2D(graphicsDevice, 1024, 1024, false, SurfaceFormat.Vector2, DepthFormat.None);
            _rt20482 = new RenderTarget2D(graphicsDevice, 2048, 2048, false, SurfaceFormat.Vector2, DepthFormat.None);
        }

        public override void Dispose()
        {
            _rt2562.Dispose();
            _rt5122.Dispose();
            _rt10242.Dispose();
            _rt20482.Dispose();
        }

        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            this.EnsureRenderTargetFormat(sourceRT, SurfaceFormat.Vector2);

            //Only square expected
            int size = sourceRT.Width;
            //select rendertarget
            RenderTarget2D renderTargetBlur = GetRenderTarget2D(size);
            this.EnsureRenderTargetReference(renderTargetBlur, null);

            _graphicsDevice.SetRenderTarget(renderTargetBlur);

            Vector2 invRes = new Vector2(1.0f / size, 1.0f / size);
            _fxSetup.Param_InverseResolution.SetValue(invRes);
            _fxSetup.Param_TargetMap.SetValue(sourceRT);

            this.Draw(_fxSetup.Pass_Horizontal);

            _graphicsDevice.SetRenderTarget(sourceRT);
            _fxSetup.Param_TargetMap.SetValue(renderTargetBlur);
            this.Draw(_fxSetup.Pass_Vertical);

            return sourceRT;
        }

        public RenderTargetCube Draw(RenderTargetCube outputCube, CubeMapFace cubeFace)
        {
            this.EnsureRenderTargetFormat(outputCube, SurfaceFormat.Vector2);

            //Only square expected
            int size = outputCube.Size;
            //select rendertarget
            RenderTarget2D renderTargetBlur = GetRenderTarget2D(size);
            this.EnsureRenderTargetReference(renderTargetBlur, null);

            _graphicsDevice.SetRenderTarget(renderTargetBlur);

            Vector2 invRes = new Vector2(1.0f / size, 1.0f / size);
            _fxSetup.Param_InverseResolution.SetValue(invRes);
            _fxSetup.Param_TargetMap.SetValue(outputCube);
            this.Draw(_fxSetup.Pass_Horizontal);

            _graphicsDevice.SetRenderTarget(outputCube, cubeFace);
            _fxSetup.Param_TargetMap.SetValue(renderTargetBlur);
            this.Draw(_fxSetup.Pass_Vertical);

            return outputCube;
        }

        protected RenderTarget2D GetRenderTarget2D(int size)
        {
            return size switch
            {
                256 => _rt2562,
                512 => _rt5122,
                1024 => _rt10242,
                2048 => _rt20482,
                _ => throw new ArgumentException("The given size is unsupported. Use 256, 512, 1024 or 2048 innstead."),
            };
        }
        protected void EnsureRenderTargetFormat(Texture renderTarget, SurfaceFormat format = SurfaceFormat.Vector2)
        {
            if (renderTarget.Format != format)
                throw new NotImplementedException("Unsupported Format for blurring");
        }
        protected void EnsureRenderTargetReference(Texture renderTarget, Texture reference = null)
        {
            if (renderTarget == reference)
                throw new NotImplementedException("Unsupported Size for blurring");
        }

    }

}