using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DepthReconstructPipelineModule : PipelineModule
    {

        private FullscreenTriangleBuffer _fullscreenTarget;

        public PipelineMatrices Matrices { get; set; }
        private ReconstructDepthFxSetup _effectSetup = new ReconstructDepthFxSetup();


        public float FarClip { set { _effectSetup.Param_FarClip.SetValue(value); } }
        public Texture2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }
        public Vector3[] FrustumCorners { set { _effectSetup.Param_FrustumCorners.SetValue(value); } }


        public DepthReconstructPipelineModule(ContentManager content, string shaderPath = "Shaders/ScreenSpace/ReconstructDepth")
            : base(content, shaderPath)
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

        }

        protected override void Load(ContentManager content, string shaderPath)
        {

        }


        public void ReconstructDepth()
        {
            _graphicsDevice.SetState(DepthStencilStateOption.Default);
            _effectSetup.Param_Projection.SetValue(Matrices.Projection);
            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }

    }
}
