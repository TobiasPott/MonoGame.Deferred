using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{

    public interface IPipelineModule : IDisposable
    {
        PipelineMatrices Matrices { get; set; }
        void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
    }



    public abstract class PipelineModule : PipelineModuleCore
    {

        public PipelineProfiler Profiler { get; set; }
        public PipelineMatrices Matrices { get; set; }
        public PipelineFrustum Frustum { get; set; }


        public PipelineModule()
        { }
      

    }

}
