using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.DeferredLighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class LightingPipelineModule : IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        private FullscreenTriangleBuffer _fullscreenTarget;

        private bool _useDepthStencilLightCulling;
        private PipelineMatrices _matrices;
        private BlendState _lightBlendState;

        private bool _viewProjectionHasChanged;

        public PointLightRenderModule PointLightRenderModule;
        public DirectionalLightRenderModule DirectionalLightRenderModule;


        public LightingPipelineModule()
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


        public void UpdateGameTime(GameTime gameTime)
        {
            PointLightRenderModule.GameTime = gameTime;
        }
        /// <summary>
        /// Needs to be called before draw
        /// </summary>
        public void UpdateViewProjection(BoundingFrustum boundingFrustum, bool viewProjHasChanged, PipelineMatrices matrices)
        {
            PointLightRenderModule.Frustum = boundingFrustum;

            _viewProjectionHasChanged = viewProjHasChanged;
            _matrices = matrices;
        }

        /// <summary>
        /// Draw our lights to the diffuse/specular/volume buffer
        /// </summary>
        public void DrawLights(EntitySceneGroup scene, Vector3 cameraOrigin, RenderTargetBinding[] renderTargetLightBinding,
            RenderTarget2D renderTargetDiffuse)
        {
            //Reconstruct Depth
            if (RenderingSettings.g_UseDepthStencilLightCulling > 0)
            {
                _graphicsDevice.SetRenderTarget(renderTargetDiffuse);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, new Color(0, 0, 0, 0.0f), 1, 0);
                _graphicsDevice.Clear(ClearOptions.Stencil, new Color(0, 0, 0, 0.0f), 1, 0);
                ReconstructDepth();

                _useDepthStencilLightCulling = true;
            }
            else
            {
                if (_useDepthStencilLightCulling)
                {
                    _useDepthStencilLightCulling = false;
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

            PointLightRenderModule.Draw(scene.PointLights, cameraOrigin, _matrices, _viewProjectionHasChanged);
            DirectionalLightRenderModule.DrawDirectionalLights(scene.DirectionalLights, cameraOrigin, _matrices, _viewProjectionHasChanged);

        }
        private void ReconstructDepth()
        {
            if (_viewProjectionHasChanged)
                Shaders.ReconstructDepth.Param_Projection.SetValue(_matrices.Projection);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.ReconstructDepth.Effect.CurrentTechnique.Passes[0].Apply();
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
