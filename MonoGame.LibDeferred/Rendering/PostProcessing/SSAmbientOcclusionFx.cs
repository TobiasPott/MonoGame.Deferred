using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class SSAmbientOcclustionFx : PostFx
    {
        //Screen Space Ambient Occlusion
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);
        public readonly static NotifiedProperty<bool> ModuleEnableBlur = new NotifiedProperty<bool>(true);

        public readonly static NotifiedProperty<float> ModuleFalloffMin = new NotifiedProperty<float>(0.001f);
        public readonly static NotifiedProperty<float> ModuleFalloffMax = new NotifiedProperty<float>(0.03f);
        public readonly static NotifiedProperty<int> ModuleSamples = new NotifiedProperty<int>(8);
        public readonly static NotifiedProperty<float> ModuleRadius = new NotifiedProperty<float>(30.0f);
        public readonly static NotifiedProperty<float> ModuleStrength = new NotifiedProperty<float>(0.5f);


        protected override bool GetEnabled() => _enabled && SSAmbientOcclustionFx.ModuleEnabled;

        protected bool _enableBlur = true;
        public bool EnableBlur { get => _enableBlur && ModuleEnableBlur; set => _enableBlur = value; }


        private readonly SSAmbientOcclusionFxSetup _fxSetup = new SSAmbientOcclusionFxSetup();


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


        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
            {
                return sourceRT;
                this.Blit(sourceRT, destRT);
                return destRT;
            }

            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);

            _fxSetup.Param_AspectRatio.SetValue(_aspectRatios);
            _fxSetup.Param_InverseResolution.SetValue(_inverseResolution);
            _fxSetup.Param_FrustumCorners.SetValue(this.Frustum.ViewSpaceFrustum);

            _fxSetup.Param_FalloffMin.SetValue(SSAmbientOcclustionFx.ModuleFalloffMin);
            _fxSetup.Param_FalloffMax.SetValue(SSAmbientOcclustionFx.ModuleFalloffMax);
            _fxSetup.Param_Samples.SetValue(SSAmbientOcclustionFx.ModuleSamples);
            _fxSetup.Param_SampleRadius.SetValue(SSAmbientOcclustionFx.ModuleRadius);
            _fxSetup.Param_Strength.SetValue(SSAmbientOcclustionFx.ModuleStrength);

            _fxSetup.Effect.CurrentTechnique = _fxSetup.Technique_SSAO;
            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            DrawSSAOToBlur();
            DrawSSAOBilateralBlur();

            // sample profiler if set
            this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_SSFx_SSAO);

            return destRT;
        }

        public void DrawSSAOToBlur()
        {
            _graphicsDevice.SetRenderTarget(_ssfxTargets.AO_Blur_V);
            _spriteBatch.Begin(0, BlendState.Additive);
            _spriteBatch.Draw(_ssfxTargets.AO_Main, RenderingSettings.Screen.Rect, Color.White);
            _spriteBatch.End();
        }
        /// <summary>
        /// Bilateral blur, to upsample our undersampled SSAO
        /// </summary>
        public void DrawSSAOBilateralBlur()
        {
            if (this.EnableBlur)
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
