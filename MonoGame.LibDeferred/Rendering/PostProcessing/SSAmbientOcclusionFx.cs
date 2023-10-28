using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class SSAmbientOcclustionFx : PostFx
    {
        //Screen Space Ambient Occlusion
        public static bool g_ssao_blur { get; set; } = true;

        public static bool g_ssao_draw { get; set; } = true;

        public static float g_ssao_falloffmin { get; set; } = 0.001f;
        public static float g_ssao_falloffmax { get; set; } = 0.03f;
        public static int g_ssao_samples { get; set; } = 8;
        public static float g_ssao_radius { get; set; } = 30.0f;
        public static float g_ssao_strength { get; set; } = 0.5f;


        protected override bool GetEnabled() => _enabled && SSAmbientOcclustionFx.g_ssao_draw;



        private SSAmbientOcclusionFxSetup _fxSetup = new SSAmbientOcclusionFxSetup();

        public PipelineMatrices Matrices { get; set; }

        public Vector2 AspectRatios
        { set { _fxSetup.Param_AspectRatio.SetValue(value); } }
        public Vector2 InverseResolution { set { _fxSetup.Param_InverseResolution.SetValue(value); } }
        public Vector3[] FrustumCorners { set { _fxSetup.Param_FrustumCorners.SetValue(value); } }

        public RenderTarget2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }
        public RenderTarget2D NormalMap { set { _fxSetup.Param_NormalMap.SetValue(value); } }


        private SSFxTargets _ssfxTargets;
        public SSFxTargets SSFxTargets
        {
            set
            {
                _ssfxTargets = value;
                _fxSetup.Param_SSAOMap.SetValue(value?.AO_Main);
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
            if (!this.Enabled)
                return sourceRT;

            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);


            _fxSetup.Param_FalloffMin.SetValue(SSAmbientOcclustionFx.g_ssao_falloffmin);
            _fxSetup.Param_FalloffMax.SetValue(SSAmbientOcclustionFx.g_ssao_falloffmax);
            _fxSetup.Param_Samples.SetValue(SSAmbientOcclustionFx.g_ssao_samples);
            _fxSetup.Param_SampleRadius.SetValue(SSAmbientOcclustionFx.g_ssao_radius);
            _fxSetup.Param_Strength.SetValue(SSAmbientOcclustionFx.g_ssao_strength);

            _fxSetup.Effect.CurrentTechnique = _fxSetup.Technique_SSAO;
            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            DrawSSAOToBlur();
            DrawSSAOBilateralBlur();
            // ToDo: change return render target to be Blur_Final (as it is target in biliteral blur)
            return destRT;
        }

        public void DrawSSAOToBlur()
        {
            _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_V);
            _spriteBatch.Begin(0, BlendState.Additive);
            _spriteBatch.Draw(_ssfxTargets.AO_Main, RenderingSettings.Screen.g_Rect, Color.White);
            _spriteBatch.End();
        }
        /// <summary>
        /// Bilateral blur, to upsample our undersampled SSAO
        /// </summary>
        public void DrawSSAOBilateralBlur()
        {
            if (SSAmbientOcclustionFx.g_ssao_blur && this.Enabled)
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_H);
                _graphicsDevice.SetState(RasterizerStateOption.CullNone);

                _fxSetup.Param_InverseResolution.SetValue(new Vector2(1.0f / _ssfxTargets.AO_Blur_V.Width, 1.0f / _ssfxTargets.AO_Blur_V.Height) * 2);
                _fxSetup.Param_SSAOMap.SetValue(_ssfxTargets.AO_Blur_V);
                _fxSetup.Technique_BlurVertical.Passes[0].Apply();

                _fullscreenTarget.Draw(_graphicsDevice);

                _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_Final);

                _fxSetup.Param_InverseResolution.SetValue(new Vector2(1.0f / _ssfxTargets.AO_Blur_H.Width, 1.0f / _ssfxTargets.AO_Blur_H.Height) * 0.5f);
                _fxSetup.Param_SSAOMap.SetValue(_ssfxTargets.AO_Blur_H);
                _fxSetup.Technique_BlurHorizontal.Passes[0].Apply();

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

        public void SetViewPosition(Vector3 viewPosition)
        {
            _fxSetup.Param_InverseViewProjection.SetValue(this.Matrices.InverseViewProjection);
            _fxSetup.Param_Projection.SetValue(this.Matrices.Projection);
            _fxSetup.Param_ViewProjection.SetValue(this.Matrices.ViewProjection);

            _fxSetup.Param_CameraPosition.SetValue(viewPosition);
        }


        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }


    }
}
