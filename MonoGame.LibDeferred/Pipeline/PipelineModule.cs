using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{

    public interface IPipelineModule : IDisposable
    {
        PipelineProfiler Profiler { get; set; }
        PipelineMatrices Matrices { get; set; }
        PipelineFrustum Frustum { get; set; }

        void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
    }



    public abstract class PipelineModule : PipelineModuleCore
    {
        public PipelineModule()
        { }

    }

}
