using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeferredEngine.Renderer
{
    public partial class RenderingPipeline
    {
        [Obsolete("DrawCubeMap is heavy on performance and needs to be detached from the main pipeline. Do not use", true)]
        /// <summary>
        /// Another draw function, but this time for cubemaps. Doesn't need all the stuff we have in the main draw function
        /// </summary>
        /// <param name="origin">from where do we render the cubemap</param>
        /// ToDo: @tpott: Wrap this method into it's own module holding the required buffers and only render required passes/effects
        private void DrawCubeMap(Vector3 origin, DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, float farPlane, GameTime gameTime, Camera camera)
        {
            //If our cubemap is not yet initialized, create a new one
            if (_renderTargetCubeMap == null)
            {
                //Create a new cube map
                _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, RenderingSettings.EnvironmentMapping.MapResolution, true, SurfaceFormat.HalfVector4,
                    DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                //Set this cubemap in the shader of the environment map
                Shaders.Environment.Param_ReflectionCubeMap.SetValue(_renderTargetCubeMap);
            }

            //Set up all the base rendertargets with the resolution of our cubemap
            SetUpRenderTargets(RenderingSettings.EnvironmentMapping.MapResolution, RenderingSettings.EnvironmentMapping.MapResolution, true);

            //We don't want to use SSAO in this cubemap
            bool prevUseSSAO = _deferredModule.UseSSAOMap;
            _deferredModule.UseSSAOMap = false;

            //Create our projection, which is a basic pyramid
            _matrices.Projection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, farPlane);

            //Now we need to actually render for each cubemapface (6 direcetions)
            for (int i = 0; i < 6; i++)
            {
                // render the scene to all cubemap faces
                _matrices.SetFromCubeMapFace(origin, (CubeMapFace)i);
                // update our projection matrices
                _matrices.UpdateFromView();

                //Pass these values to our shader
                Shaders.SSAO.Param_InverseViewProjection.SetValue(_matrices.InverseView);
                _pointLightRenderModule.InverseView = _matrices.InverseView;

                //yep we changed
                _viewProjectionHasChanged = true;

                _boundingFrustum.Matrix = _matrices.ViewProjection;
                ComputeFrustumCorners(_boundingFrustum, camera);


                //Base stuff, for description look in Draw()
                meshBatcher.FrustumCulling(_boundingFrustum, true, origin);

                DrawGBuffer(meshBatcher);

                bool volumeEnabled = RenderingSettings.g_VolumetricLights;
                RenderingSettings.g_VolumetricLights = false;
                _lightingModule.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _matrices);
                _lightingModule.DrawLights(scene, origin, _lightingBufferTarget.Bindings, _lightingBufferTarget.Diffuse);

                _environmentModule.DrawSky();

                RenderingSettings.g_VolumetricLights = volumeEnabled;

                //We don't use temporal AA obviously for the cubemap
                bool tempAa = _taaFx.Enabled;
                _taaFx.Enabled = false;

                Compose(_auxTargets[MRT.COMPOSE]);

                _taaFx.Enabled = tempAa;
                DrawTextureToScreenToCube(_auxTargets[MRT.COMPOSE], _renderTargetCubeMap, (CubeMapFace?)i);
            }
            _deferredModule.UseSSAOMap = prevUseSSAO;

            //Change RTs back to normal
            SetUpRenderTargets(RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight, true);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawCubeMap = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

    }
}
