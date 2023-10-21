using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Rendering.PostProcessing
{
    public class PostProcessingStack : IDisposable
    {
        public readonly BloomFx Bloom;
        public readonly TemporalAAFx TemporaAA;
        public readonly ColorGradingFx ColorGrading;
        //GaussianBlurFx _gaussianBlur;

        private SpriteBatch _spriteBatch;

        public PostProcessingStack(ContentManager content)
        {
            Bloom = new BloomFx(content);
            TemporaAA = new TemporalAAFx();
            ColorGrading = new ColorGradingFx(content);
            //_gaussianBlur = new GaussianBlurFx();
        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            Bloom.Initialize(graphicsDevice, RenderingSettings.g_ScreenResolution);
            TemporaAA.Initialize(graphicsDevice, FullscreenTriangleBuffer.Instance);
            ColorGrading.Initialize(graphicsDevice, FullscreenTriangleBuffer.Instance);

        }

        public void SetPipelineMatrices(PipelineMatrices matrices)
        {
            TemporaAA.Matrices = matrices;
        }


        public void Dispose()
        {
            Bloom?.Dispose();
            TemporaAA?.Dispose();
            ColorGrading?.Dispose();
            //_gaussianBlur?.Dispose();

        }

    }

}

