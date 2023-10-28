using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DirectionalLightPipelineModule : PipelineModule
    {

        private FullscreenTriangleBuffer _fullscreenTarget;

        private DirectionalLightFxSetup _effectSetup = new DirectionalLightFxSetup();

        public DirectionalLightPipelineModule()
            : base()
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }



        public void SetScreenSpaceShadowMap(RenderTarget2D renderTarget2D) => _effectSetup.Param_SSShadowMap.SetValue(renderTarget2D);
        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            _effectSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _effectSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _effectSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }


        /// <summary>
        /// Draw all directional lights, set up some shader variables first
        /// </summary>
        public void DrawDirectionalLights(List<DirectionalLight> dirLights, Vector3 cameraPosition, bool viewProjectionHasChanged)
        {
            if (dirLights.Count < 1)
                return;
            _graphicsDevice.SetStates(DepthStencilStateOption.None, RasterizerStateOption.CullCounterClockwise);

            _effectSetup.Param_FrustumCorners.SetValue(this.Frustum.ViewSpaceFrustum);
            //If nothing has changed we don't need to update
            if (viewProjectionHasChanged)
            {
                _effectSetup.Param_ViewProjection.SetValue(this.Matrices.ViewProjection);
                _effectSetup.Param_InverseViewProjection.SetValue(this.Matrices.InverseViewProjection);
                _effectSetup.Param_CameraPosition.SetValue(cameraPosition);
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
            _effectSetup.Param_LightColor.SetValue(light.ColorV3);
            _effectSetup.Param_LightDirection.SetValue(light.DirectionViewSpace);
            _effectSetup.Param_LightIntensity.SetValue(light.Intensity);

            if (light.CastShadows)
            {
                _effectSetup.Param_LightView.SetValue(light.Matrices.View_ViewSpace);
                _effectSetup.Param_LightViewProjection.SetValue(light.Matrices.ViewProjection_ViewSpace);
                _effectSetup.Param_LightFarClip.SetValue(light.ShadowFarClip);
                _effectSetup.Param_ShadowMap.SetValue(light.ShadowMap);
                _effectSetup.Param_ShadowFiltering.SetValue((int)light.ShadowFiltering);
                _effectSetup.Param_ShadowMapSize.SetValue((float)light.ShadowResolution);
                _effectSetup.Technique_Shadowed.Passes[0].Apply();
            }
            else
            {
                _effectSetup.Technique_Unshadowed.Passes[0].Apply();
            }
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }

    }
}
