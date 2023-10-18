using DeferredEngine.Renderer;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DirectionalLightPipelineModule : PipelineModule
    {

        private FullscreenTriangleBuffer _fullscreenTarget;
        private DirectionalLightEffectSetup _effectSetup = new DirectionalLightEffectSetup();


        public DirectionalLightPipelineModule(ContentManager content, string shaderPath = "Shaders/Deferred/DeferredDirectionalLight")
            : base(content, shaderPath)
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }

        protected override void Load(ContentManager content, string shaderPath)
        {

        }

        public void SetFrustumCorners(Vector3[] currentFrustumCorners) => _effectSetup.Param_FrustumCorners.SetValue(currentFrustumCorners);
        public void SetScreenSpaceShadowMap(RenderTarget2D renderTarget2D) => _effectSetup.Param_SSShadowMap.SetValue(renderTarget2D);
        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            _effectSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _effectSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _effectSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        protected void SetCameraAndMatrices(Vector3 cameraPosition, PipelineMatrices matrices)
        {
            _effectSetup.Param_ViewProjection.SetValue(matrices.ViewProjection);
            _effectSetup.Param_InverseViewProjection.SetValue(matrices.InverseViewProjection);
            _effectSetup.Param_CameraPosition.SetValue(cameraPosition);
        }


        /// <summary>
        /// Draw all directional lights, set up some shader variables first
        /// </summary>
        public void DrawDirectionalLights(List<DeferredDirectionalLight> dirLights, Vector3 cameraPosition, PipelineMatrices matrices, bool viewProjectionHasChanged)
        {
            if (dirLights.Count < 1) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;

            //If nothing has changed we don't need to update
            if (viewProjectionHasChanged)
                this.SetCameraAndMatrices(cameraPosition, matrices);

            for (int index = 0; index < dirLights.Count; index++)
            {
                DeferredDirectionalLight light = dirLights[index];
                if (viewProjectionHasChanged)
                    light.UpdateViewSpaceProjection(matrices);

                this.DrawDirectionalLight(light);
            }
        }

        /// <summary>
        /// Draw the individual light, full screen effect
        /// </summary>
        /// <param name="light"></param>
        private void DrawDirectionalLight(DeferredDirectionalLight light)
        {
            if (!light.IsEnabled) return;

            this.ApplyShader(light);
            _fullscreenTarget.Draw(_graphicsDevice);
        }
        public void ApplyShader(DeferredDirectionalLight light)
        {
            _effectSetup.Param_LightColor.SetValue(light.Color_sRGB);
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
