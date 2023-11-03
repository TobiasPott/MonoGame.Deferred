using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DirectionalLightPipelineModule : PipelineModule
    {

        private Vector3 _viewOrigin;
        private readonly FullscreenTriangleBuffer _fullscreenTarget;
        private readonly DirectionalLightFxSetup _fxSetup = new DirectionalLightFxSetup();

        public Vector3 ViewOrigin { set => _viewOrigin = value; get => _viewOrigin; }

        public DirectionalLightPipelineModule()
            : base()
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }



        public void SetScreenSpaceShadowMap(RenderTarget2D renderTarget2D) => _fxSetup.Param_SSShadowMap.SetValue(renderTarget2D);
        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            _fxSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _fxSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _fxSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }


        /// <summary>
        /// Draw all directional lights, set up some shader variables first
        /// </summary>
        public void Draw(List<DirectionalLight> dirLights, bool viewProjectionHasChanged)
        {
            if (dirLights.Count < 1)
                return;
            _graphicsDevice.SetStates(DepthStencilStateOption.None, RasterizerStateOption.CullCounterClockwise);

            _fxSetup.Param_FrustumCorners.SetValue(this.Frustum.ViewSpaceFrustum);
            //If nothing has changed we don't need to update
            if (viewProjectionHasChanged)
            {
                _fxSetup.Param_ViewProjection.SetValue(this.Matrices.ViewProjection);
                _fxSetup.Param_InverseViewProjection.SetValue(this.Matrices.InverseViewProjection);
                _fxSetup.Param_CameraPosition.SetValue(this.ViewOrigin);
            }

            for (int index = 0; index < dirLights.Count; index++)
            {
                DirectionalLight light = dirLights[index];
                if (viewProjectionHasChanged)
                    light.UpdateViewSpaceProjection(this.Matrices);

                this.DrawDirectionalLight(light);
            }
        }

        /// <summary>
        /// Draw the individual light, full screen effect
        /// </summary>
        /// <param name="light"></param>
        private void DrawDirectionalLight(DirectionalLight light)
        {
            if (!light.IsEnabled) return;

            this.ApplyShader(light);
            _fullscreenTarget.Draw(_graphicsDevice);
        }
        public void ApplyShader(DirectionalLight light)
        {
            _fxSetup.Param_LightColor.SetValue(light.ColorV3);
            _fxSetup.Param_LightDirection.SetValue(light.DirectionViewSpace);
            _fxSetup.Param_LightIntensity.SetValue(light.Intensity);

            if (light.CastShadows)
            {
                _fxSetup.Param_LightView.SetValue(light.Matrices.View_ViewSpace);
                _fxSetup.Param_LightViewProjection.SetValue(light.Matrices.ViewProjection_ViewSpace);
                _fxSetup.Param_LightFarClip.SetValue(light.ShadowFarClip);
                _fxSetup.Param_ShadowMap.SetValue(light.ShadowMap);
                _fxSetup.Param_ShadowFiltering.SetValue((int)light.ShadowFiltering);
                _fxSetup.Param_ShadowMapSize.SetValue((float)light.ShadowResolution);
                _fxSetup.Technique_Shadowed.Passes[0].Apply();
            }
            else
            {
                _fxSetup.Technique_Unshadowed.Passes[0].Apply();
            }
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }

    }
}
