using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DepthReconstructPipelineModule : PipelineModule
    {

        private readonly FullscreenTriangleBuffer _fullscreenTarget;
        private readonly ReconstructDepthFxSetup _fxSetup = new ReconstructDepthFxSetup();

        public Texture2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }


        public DepthReconstructPipelineModule()
            : base()
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

        }



        public void ReconstructDepth()
        {
            _graphicsDevice.SetState(DepthStencilStateOption.Default);

            _fxSetup.Param_Projection.SetValue(Matrices.Projection);
            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }

    }
}
