using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline
{
    //Just a template
    public class GBufferPipelineModule : PipelineModule, IRenderModule
    {
        private readonly GBufferFxSetup _fxSetup = new GBufferFxSetup();
        private GBufferTarget _gBufferTarget;
        private FullscreenTriangleBuffer _fullscreenTarget;


        public GBufferTarget GBufferTarget { set { _gBufferTarget = value; } }

        public bool ClearGBuffer { get; set; } = true;


        public GBufferPipelineModule()
            : base()
        { }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }
        public void Draw(DynamicMeshBatcher meshBatcher)
        {
            _graphicsDevice.SetRenderTargets(_gBufferTarget.Bindings);

            //Clear the GBuffer
            if (this.ClearGBuffer)
            {
                _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullNone, BlendStateOption.Opaque);
                _fxSetup.Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!
            if (meshBatcher.CheckRequiresRedraw(RenderType.Opaque, true, false))
                meshBatcher.Draw(renderType: RenderType.Opaque, this.Matrices, RenderContext.Default, this);

            // sample profiler if set
            this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_GBuffer);
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Matrix worldView = localWorldMatrix * (Matrix)view;
            _fxSetup.Param_WorldView.SetValue(worldView);
            _fxSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            worldView = Matrix.Invert(Matrix.Transpose(worldView));
            _fxSetup.Param_WorldViewIT.SetValue(worldView);
            _fxSetup.Effect_GBuffer.CurrentTechnique.Passes[0].Apply();

            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
        }

        public void SetMaterialSettings(MaterialBase material)
        {
            material.SetGBufferForMaterial(_fxSetup);
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }
    }

}
