using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.PostProcessing
{
    public abstract class BaseFx
    {
        private GraphicsDevice _graphicsDevice;
        private FullScreenTriangleBuffer _fullscreenTarget;

        public abstract void Draw(Texture2D source, Texture2D destination);
        public abstract void Draw(Texture2D source);
        public abstract void Draw(GraphicsDevice graphics, Texture2D source, Texture2D destination);



        //public abstract void Draw(GraphicsDevice _graphicsDevice, bool useTonemap,
        //    RenderTarget2D currentFrame, RenderTarget2D previousFrames, RenderTarget2D output,
        //    Matrix currentViewToPreviousViewProjection, FullScreenTriangleBuffer fullScreenTriangle);
        //public abstract RenderTarget2D DrawGaussianBlur(RenderTarget2D renderTargetOutput, FullScreenTriangleBuffer triangle);
        //public abstract RenderTargetCube DrawGaussianBlur(RenderTargetCube renderTargetOutput, FullScreenTriangleBuffer triangle, CubeMapFace cubeFace);
        //public abstract RenderTarget2D Draw(GraphicsDevice graphics, Texture2D input, Texture2D lookupTable = null);

        //public abstract Texture2D Draw(Texture2D inputTexture, Vector2 resolution);

    }
}
