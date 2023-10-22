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
        public bool Enabled { get => _enabled && RenderingSettings.g_ssao_draw; set { _enabled = value; } }


        private SSAmbientOcclusionFxSetup _effectSetup = new SSAmbientOcclusionFxSetup();

        public PipelineMatrices Matrices { get; set; }

        public Vector2 AspectRatios
        { set { _effectSetup.Param_AspectRatio.SetValue(value); } }
        public Vector2 InverseResolution { set { _effectSetup.Param_InverseResolution.SetValue(value); } }
        public Vector3[] FrustumCorners { set { _effectSetup.Param_FrustumCorners.SetValue(value); } }

        public RenderTarget2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }
        public RenderTarget2D NormalMap { set { _effectSetup.Param_NormalMap.SetValue(value); } }


        private SSFxTargets _ssfxTargets;
        public SSFxTargets SSFxTargets
        {
            set
            {
                _ssfxTargets = value;
                _effectSetup.Param_SSAOMap.SetValue(value?.AO_Main);
            }
        }
        private SpriteBatch _spriteBatch;

        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public SSAmbientOcclustionFx(ContentManager content)
        {
        }

        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FullscreenTriangleBuffer fullscreenTarget)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _fullscreenTarget = fullscreenTarget;

        }

        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            return DrawSSAO(sourceRT, previousRT, destRT);
        }
        /// <summary>
        /// Draw SSAO to a different rendertarget
        /// </summary>
        private RenderTarget2D DrawSSAO(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            // ToDo: @tpott: extract to own BaseFx derived type
            if (!this.Enabled)
                return sourceRT;

            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);


            _effectSetup.Param_FalloffMin.SetValue(RenderingSettings.g_ssao_falloffmin);
            _effectSetup.Param_FalloffMax.SetValue(RenderingSettings.g_ssao_falloffmax);
            _effectSetup.Param_Samples.SetValue(RenderingSettings.g_ssao_samples);
            _effectSetup.Param_SampleRadius.SetValue(RenderingSettings.g_ssao_radius);
            _effectSetup.Param_Strength.SetValue(RenderingSettings.g_ssao_strength);

            _effectSetup.Effect.CurrentTechnique = _effectSetup.Technique_SSAO;
            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            DrawSSAOToBlur();
            DrawSSAOBilateralBlur();
            // ToDo: change return render target to be Blur_Final (as it is target in biliteral blur
            return destRT;
        }

        public void DrawSSAOToBlur()
        {
            _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_V);
            _spriteBatch.Begin(0, BlendState.Additive);
            _spriteBatch.Draw(_ssfxTargets.AO_Main, RenderingSettings.g_ScreenRect, Color.White);
            _spriteBatch.End();
        }
        /// <summary>
        /// Bilateral blur, to upsample our undersampled SSAO
        /// </summary>
        public void DrawSSAOBilateralBlur()
        {
            if (RenderingSettings.g_ssao_blur && this.Enabled)
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_H);
                _graphicsDevice.SetState(RasterizerStateOption.CullNone);

                _effectSetup.Param_InverseResolution.SetValue(new Vector2(1.0f / _ssfxTargets.AO_Blur_V.Width, 1.0f / _ssfxTargets.AO_Blur_V.Height) * 2);
                _effectSetup.Param_SSAOMap.SetValue(_ssfxTargets.AO_Blur_V);
                _effectSetup.Technique_BlurVertical.Passes[0].Apply();

                _fullscreenTarget.Draw(_graphicsDevice);

                _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_Final);

                _effectSetup.Param_InverseResolution.SetValue(new Vector2(1.0f / _ssfxTargets.AO_Blur_H.Width, 1.0f / _ssfxTargets.AO_Blur_H.Height) * 0.5f);
                _effectSetup.Param_SSAOMap.SetValue(_ssfxTargets.AO_Blur_H);
                _effectSetup.Technique_BlurHorizontal.Passes[0].Apply();

                _fullscreenTarget.Draw(_graphicsDevice);

            }
            else
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_Final);
                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp);
                _spriteBatch.Draw(_ssfxTargets.AO_Blur_V, new Rectangle(0, 0, _ssfxTargets.AO_Blur_Final.Width, _ssfxTargets.AO_Blur_Final.Height), Color.White);
                _spriteBatch.End();
            }

        }

        public void SetCameraAndMatrices(Vector3 cameraPosition, PipelineMatrices matrices)
        {
            _effectSetup.Param_InverseViewProjection.SetValue(matrices.InverseViewProjection);
            _effectSetup.Param_Projection.SetValue(matrices.Projection);
            _effectSetup.Param_ViewProjection.SetValue(matrices.ViewProjection);

            _effectSetup.Param_CameraPosition.SetValue(cameraPosition);
        }


        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }


    }
}
