using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DepthReconstructPipelineModule : PipelineModule
    {

        private FullscreenTriangleBuffer _fullscreenTarget;

        private ReconstructDepthFxSetup _effectSetup = new ReconstructDepthFxSetup();

        public Texture2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }


        public DepthReconstructPipelineModule()
            : base()
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

        }



        public void ReconstructDepth()
        {
            _graphicsDevice.SetState(DepthStencilStateOption.Default);

            _effectSetup.Param_Projection.SetValue(Matrices.Projection);
            _effectSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
            _effectSetup.Param_FrustumCorners.SetValue(this.Frustum.ViewSpaceFrustum);
            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }

    }
}
