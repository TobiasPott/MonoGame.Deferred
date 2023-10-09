using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.DeferredLighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class LightAccumulationModule : IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        private FullscreenTriangleBuffer _fullscreenTarget;

        private bool _g_UseDepthStencilLightCulling;
        private BlendState _lightBlendState;
        private BoundingFrustum _boundingFrustum;

        private bool _viewProjectionHasChanged;

        private Matrix _view;
        private Matrix _inverseView;
        private Matrix _viewIT;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _inverseViewProjection;

        public PointLightRenderModule PointLightRenderModule;


        public LightAccumulationModule()
        { }
        public void Load(ContentManager content, string shaderPath = "")
        {

        }
        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

            _lightBlendState = new BlendState
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };

        }


        /// <summary>
        /// Needs to be called before draw
        /// </summary>
        /// <param name="boundingFrustum"></param>
        /// <param name="viewProjHasChanged"></param>
        /// <param name="view"></param>
        /// <param name="inverseView"></param>
        /// <param name="viewIT"></param>
        /// <param name="projection"></param>
        /// <param name="viewProjection"></param>
        /// <param name="staticViewProjection"></param>
        /// <param name="inverseViewProjection"></param>
        public void UpdateViewProjection(BoundingFrustum boundingFrustum,
                                         bool viewProjHasChanged,
                                         Matrix view,
                                         Matrix inverseView,
                                         Matrix viewIT,
                                         Matrix projection,
                                         Matrix viewProjection,
                                         Matrix inverseViewProjection)
        {
            _boundingFrustum = boundingFrustum;
            _viewProjectionHasChanged = viewProjHasChanged;
            _view = view;
            _inverseView = inverseView;
            _viewIT = viewIT;
            _projection = projection;
            _viewProjection = viewProjection;
            _inverseViewProjection = inverseViewProjection;
        }

        /// <summary>
        /// Draw our lights to the diffuse/specular/volume buffer
        /// </summary>
        public void DrawLights(EntitySceneGroup scene, Vector3 cameraOrigin, GameTime gameTime, RenderTargetBinding[] renderTargetLightBinding, RenderTarget2D renderTargetDiffuse)
        {
            List<DeferredPointLight> pointLights = scene.PointLights;
            List<DeferredDirectionalLight> dirLights = scene.DirectionalLights;

            //Reconstruct Depth
            if (RenderingSettings.g_UseDepthStencilLightCulling > 0)
            {
                _graphicsDevice.SetRenderTarget(renderTargetDiffuse);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, new Color(0, 0, 0, 0.0f), 1, 0);
                _graphicsDevice.Clear(ClearOptions.Stencil, new Color(0, 0, 0, 0.0f), 1, 0);
                ReconstructDepth();

                _g_UseDepthStencilLightCulling = true;
            }
            else
            {
                if (_g_UseDepthStencilLightCulling)
                {
                    _g_UseDepthStencilLightCulling = false;
                    _graphicsDevice.SetRenderTarget(renderTargetDiffuse);
                    _graphicsDevice.Clear(ClearOptions.DepthBuffer, new Color(0, 0, 0, 0.0f), 1, 0);
                }
            }

            //Setup volumetex
            //Shaders.deferredPointLightParameter_VolumeTexParam.SetValue(volumeTex.Texture);
            //Shaders.deferredPointLightParameter_VolumeTexInverseMatrix.SetValue(volumeTex.RotationMatrix);
            //Shaders.deferredPointLightParameter_VolumeTexPositionParam.SetValue(volumeTex.Position);
            //Shaders.deferredPointLightParameter_VolumeTexResolution.SetValue(volumeTex.Resolution);
            //Shaders.deferredPointLightParameter_VolumeTexScale.SetValue(volumeTex.Scale);
            //Shaders.deferredPointLightParameter_VolumeTexSizeParam.SetValue(volumeTex.Size);

            _graphicsDevice.SetRenderTargets(renderTargetLightBinding);
            _graphicsDevice.Clear(ClearOptions.Target, new Color(0, 0, 0, 0.0f), 1, 0);
            _graphicsDevice.BlendState = _lightBlendState;

            PointLightRenderModule.Draw(pointLights, cameraOrigin, gameTime, _boundingFrustum, _viewProjectionHasChanged, _view, _viewProjection, _inverseView, _graphicsDevice);
            DrawDirectionalLights(dirLights, cameraOrigin);

            ////Performance Profiler
            //if (GameSettings.d_profiler)
            //{
            //    long performanceCurrentTime = _performanceTimer.ElapsedTicks;
            //    GameStats.d_profileDrawLights = performanceCurrentTime - _performancePreviousTime;

            //    _performancePreviousTime = performanceCurrentTime;
            //}

        }
        private void ReconstructDepth()
        {
            if (_viewProjectionHasChanged)
                Shaders.ReconstructDepth.Param_Projection.SetValue(_projection);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.ReconstructDepth.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);
        }


        /// <summary>
        /// Draw all directional lights, set up some shader variables first
        /// </summary>
        /// <param name="dirLights"></param>
        /// <param name="cameraOrigin"></param>
        private void DrawDirectionalLights(List<DeferredDirectionalLight> dirLights, Vector3 cameraOrigin)
        {
            if (dirLights.Count < 1) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //If nothing has changed we don't need to update
            if (_viewProjectionHasChanged)
            {
                Shaders.DeferredDirectionalLight.Param_ViewProjection.SetValue(_viewProjection);
                Shaders.DeferredDirectionalLight.Param_CameraPosition.SetValue(cameraOrigin);
                Shaders.DeferredDirectionalLight.Param_InverseViewProjection.SetValue(_inverseViewProjection);
            }

            _graphicsDevice.DepthStencilState = DepthStencilState.None;

            for (int index = 0; index < dirLights.Count; index++)
            {
                DeferredDirectionalLight light = dirLights[index];
                DrawDirectionalLight(light);
            }
        }

        /// <summary>
        /// Draw the individual light, full screen effect
        /// </summary>
        /// <param name="light"></param>
        private void DrawDirectionalLight(DeferredDirectionalLight light)
        {
            if (!light.IsEnabled) return;

            if (_viewProjectionHasChanged)
            {
                light.DirectionViewSpace = Vector3.Transform(light.Direction, _viewIT);
                light.LightViewProjection_ViewSpace = _inverseView * light.LightViewProjection;
                light.LightView_ViewSpace = _inverseView * light.LightView;
            }

            Shaders.DeferredDirectionalLight.Param_LightColor.SetValue(light.ColorV3);
            Shaders.DeferredDirectionalLight.Param_LightDirection.SetValue(light.DirectionViewSpace);
            Shaders.DeferredDirectionalLight.Param_LightIntensity.SetValue(light.Intensity);
            light.ApplyShader();
            _fullscreenTarget.Draw(_graphicsDevice);
        }

        public void Dispose()
        {
            _graphicsDevice?.Dispose();
            _lightBlendState?.Dispose();

            PointLightRenderModule.Dispose();
        }
    }
}
