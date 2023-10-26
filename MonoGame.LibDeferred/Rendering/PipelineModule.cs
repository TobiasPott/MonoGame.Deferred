using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace DeferredEngine.Pipeline
{

    public interface IPipelineModule : IDisposable
    {
        PipelineMatrices Matrices { get; set; }
        void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
    }

    public abstract class PipelineModule : IDisposable
    {
        protected GraphicsDevice _graphicsDevice;
        protected SpriteBatch _spriteBatch;


        public PipelineMatrices Matrices { get; set; }


        public PipelineModule()
        { }
        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        public abstract void Dispose();

        public virtual void Draw(DynamicMeshBatcher meshBatcher)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual RenderTarget2D Draw(RenderTarget2D destination) => destination;

        //public RenderTarget2D Draw(RenderTarget2D output, MeshMaterialLibrary meshMat, Matrix viewProjection, Camera camera, List<DeferredPointLight> pointLights, BoundingFrustum frustum)
        //public void Draw(MeshMaterialLibrary meshMaterialLibrary, List<ModelEntity> entities, List<DeferredPointLight> pointLights, List<DeferredDirectionalLight> dirLights, Camera camera)
        //public void Draw(RenderTargetBinding[] _renderTargetBinding, MeshMaterialLibrary meshMaterialLibrary, Matrix _viewProjection, Matrix _view)
        //public void Draw(MeshMaterialLibrary meshMaterialLibrary, List<ModelEntity> entities, List<DeferredPointLight> pointLights, List<DeferredDirectionalLight> dirLights)

    }

}
