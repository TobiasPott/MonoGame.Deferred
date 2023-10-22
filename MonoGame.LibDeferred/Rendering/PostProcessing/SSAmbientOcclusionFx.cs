using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class SSAmbientOcclustionFx : BaseFx
    {

        private bool _enabled = true;
        public bool Enabled { get => _enabled && RenderingSettings.g_SSReflection; set { _enabled = value; } }


        private SSAmbientOcclusionFxSetup _effectSetup = new SSAmbientOcclusionFxSetup();

        public PipelineMatrices Matrices { get; set; }

        public Vector3[] FrustumCorners { set { _effectSetup.Param_FrustumCorners.SetValue(value); } }

        public RenderTarget2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }
        public RenderTarget2D NormalMap { set { _effectSetup.Param_NormalMap.SetValue(value); } }



        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public SSAmbientOcclustionFx(ContentManager content)
        {
        }

        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
                return sourceRT;

            //todo: more samples for more reflective materials!
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            _effectSetup.Param_Projection.SetValue(this.Matrices.Projection);

            _effectSetup.Param_Samples.SetValue(RenderingSettings.g_SSReflections_Samples);
            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            return destRT;
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }


    }
}
