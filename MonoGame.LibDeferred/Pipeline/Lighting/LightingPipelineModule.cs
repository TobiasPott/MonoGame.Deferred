using DeferredEngine.Entities;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Lighting
{
    public class LightingPipelineModule : PipelineModule
    {
        public static int g_UseDepthStencilLightCulling = 1; //None, Depth, Depth+Stencil

        private bool _useDepthStencilLightCulling;
        private bool _viewProjectionHasChanged;

        private PipelineMatrices _matrices;
        private BlendState _lightBlendState;

        public PointLightPipelineModule PointLightRenderModule;
        public DirectionalLightPipelineModule DirectionalLightRenderModule;
        public DepthReconstructPipelineModule DepthPipelineModule;



        private LightingBufferTarget _lightingBufferTarget;
        public LightingBufferTarget LightingBufferTarget { set { _lightingBufferTarget = value; } }


        public LightingPipelineModule()
            : base()
        {
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
        public void DrawLights(EntitySceneGroup scene, Vector3 cameraOrigin)
        {
            //Reconstruct Depth
            if (LightingPipelineModule.g_UseDepthStencilLightCulling > 0)
            {
                _graphicsDevice.SetRenderTarget(_lightingBufferTarget.Diffuse);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, new Color(0, 0, 0, 0.0f), 1, 0);
                _graphicsDevice.Clear(ClearOptions.Stencil, new Color(0, 0, 0, 0.0f), 1, 0);
                //ReconstructDepth();
                DepthPipelineModule.ReconstructDepth();
                _useDepthStencilLightCulling = true;
            }
            else
            {
                if (_useDepthStencilLightCulling)
                {
                    _useDepthStencilLightCulling = false;
                    _graphicsDevice.SetRenderTarget(_lightingBufferTarget.Diffuse);
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

            _graphicsDevice.SetRenderTargets(_lightingBufferTarget.Bindings);
            _graphicsDevice.Clear(ClearOptions.Target, new Color(0, 0, 0, 0.0f), 1, 0);
            _graphicsDevice.BlendState = _lightBlendState;

            PointLightRenderModule.Draw(scene.PointLights, cameraOrigin, _matrices, _viewProjectionHasChanged);
            DirectionalLightRenderModule.DrawDirectionalLights(scene.DirectionalLights, cameraOrigin, _matrices, _viewProjectionHasChanged);

        }

        public override void Dispose()
        {
            _lightBlendState?.Dispose();
        }

    }
}
