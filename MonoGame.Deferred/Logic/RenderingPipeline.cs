using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Pipeline.Utilities;
using DeferredEngine.Recources;
using DeferredEngine.Rendering.Helper.HelperGeometry;
using DeferredEngine.Rendering.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System;
using System.Collections.Generic;
using DirectionalLight = DeferredEngine.Pipeline.Lighting.DirectionalLight;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Rendering
{


    [Flags()]
    public enum EditorPasses
    {
        Billboard = 1,
        IdAndOutline = 2,
    }

    public partial class RenderingPipeline : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Graphics & Helpers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        //Projection Matrices and derivates used in shaders
        private PipelineMatrices _matrices;
        private PipelineModuleStack _moduleStack;
        private PipelineFxStack _fxStack;
        private PipelineProfiler _profiler;

        //Used for the view space directions in our shaders. Far edges of our view frustum
        private FrustumCornerVertices _frustumCorners = new FrustumCornerVertices();

        //Bounding Frusta of our view projection, to calculate which objects are inside the view
        private BoundingFrustum _boundingFrustum;

        //Checkvariables to see which console variables have changed from the frame before
        //private float _g_FarClip;
        private int _supersampling = 1;
        private bool _prevSSReflectionEnabled = true;
        private bool _g_SSReflectionNoise;

        //Render targets
        private GBufferTarget _gBufferTarget;
        private LightingBufferTarget _lightingBufferTarget;
        private PipelineTargets _auxTargets;
        private SSFxTargets _ssfxTargets;

        // Final output
        private RenderTarget2D _currentOutput;

        //Cubemap
        private RenderTargetCube _renderTargetCubeMap;


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  BASE FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Initialize variables
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            _matrices = new PipelineMatrices();

            _moduleStack = new PipelineModuleStack();
            _moduleStack.DepthReconstruct.Matrices = _matrices;
            _profiler = new PipelineProfiler();

            _fxStack = new PipelineFxStack(content);
            _fxStack.Matrices = _matrices;

        }

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);


            _gBufferTarget = new GBufferTarget(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);
            _lightingBufferTarget = new LightingBufferTarget(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);
            _auxTargets = new PipelineTargets(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);
            _ssfxTargets = new SSFxTargets(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);

            _moduleStack.Initialize(graphicsDevice, _spriteBatch);
            _moduleStack.GBuffer.GBufferTarget = _gBufferTarget;

            _fxStack.Initialize(graphicsDevice, _spriteBatch);
            _fxStack.SSFxTargets = _ssfxTargets;

            _boundingFrustum = new BoundingFrustum(_matrices.ViewProjection);

            SetUpRenderTargets(RenderingSettings.g_ScreenResolution);

            RenderingSettings.g_FarClip.Changed += FarClip_OnChanged;
            RenderingSettings.g_FarClip.Set(500);
        }

        private void FarClip_OnChanged(float farClip)
        {
            _moduleStack.FarClip = farClip;
            _fxStack.FarClip = farClip;
        }

        /// <summary>
        /// Update our function
        /// </summary>
        public void Update(GameTime gameTime, bool isActive, EntitySceneGroup scene)
        {
            if (!isActive)
                return;

            _moduleStack.DistanceField.UpdateSdfGenerator(scene.Entities);
            _moduleStack.Lighting.UpdateGameTime(gameTime);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public ObjectHoverContext Draw(Camera camera, DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, GizmoDrawContext gizmoContext, GameTime gameTime)
        {
            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();

            // Step: 01
            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges();

            // Step: 02
            //Render ShadowMaps
            _moduleStack.ShadowMap.Draw(meshBatcher, scene);
            //Performance Profile
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Shadows);

            // Step: 03
            //Update SDFs
            if (IsSDFUsed(scene.PointLights))
            {
                _moduleStack.DistanceField.UpdateDistanceFieldTransformations(scene.Entities);
            }

            // Step: 04
            //Update our view projection matrices if the camera moved
            UpdateViewProjection(meshBatcher, camera);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SUpdate_ViewProjection);


            // Step: 05
            //Draw our meshes to the G Buffer
            _moduleStack.GBuffer.Draw(meshBatcher, _matrices);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_GBuffer);


            // Step: 06
            //Deferred Decals
            DrawDecals(scene.Decals);

            // Step: 07
            //Draw Screen Space reflections to a different render target
            RenderTarget2D ssrTargetMap = _ssfxTargets.GetSSReflectionRenderTargets(_fxStack.TemporaAA.Enabled, _fxStack.TemporaAA.IsOffFrame);
            _fxStack.SSReflection.TargetMap = ssrTargetMap ?? _auxTargets[PipelineTargets.COMPOSE];
            if (SSReflectionFx.g_Noise)
                _fxStack.SSReflection.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            _fxStack.Draw(PipelineFxStage.SSReflection, null, null, _ssfxTargets.SSR_Main);
            // Profiler sample
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSR);


            // Step: 08
            //SSAO
            _fxStack.SSAmbientOcclusion.SetCameraAndMatrices(camera.Position, _matrices);
            _fxStack.Draw(PipelineFxStage.SSAmbientOcclusion, null, null, _ssfxTargets.AO_Main);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSAO);

            // Step: 09
            ////Upsample/blur our SSAO / screen space shadows
            //_fxStack.SSAmbientOcclusion.DrawSSAOToBlur();
            //_fxStack.SSAmbientOcclusion.DrawSSAOBilateralBlur();
            //Performance Profiler
            //_profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_AO_BilateralBlur);

            // Step: 10
            //Light the scene
            //_moduleStack.Lighting.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _matrices);
            _moduleStack.Lighting.DrawLights(scene, camera.Position, _lightingBufferTarget.Bindings, _lightingBufferTarget.Diffuse);

            // Step: 11
            //Draw the environment cube map as a fullscreen effect on all meshes
            DrawEnvironmentMap(scene.EnvProbe, camera, gameTime);

            // Step: 12
            //Compose the scene by combining our lighting data with the gbuffer data
            _currentOutput = DrawDeferredCompose(null, null, _auxTargets[PipelineTargets.COMPOSE]);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Compose);


            // Step: 13
            //Forward
            _currentOutput = DrawForward(_currentOutput, meshBatcher, camera, scene.PointLights);

            // Step: 14
            //Compose the image and add information from previous frames to apply temporal super sampling
            _ssfxTargets.GetTemporalAARenderTargets(_fxStack.TemporaAA.IsOffFrame, out RenderTarget2D taaDestRT, out RenderTarget2D taaPreviousRT);
            _currentOutput = _fxStack.Draw(PipelineFxStage.TemporalAA, _currentOutput, taaPreviousRT, taaDestRT);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_CombineTAA);


            // Step: 15
            //Do Bloom
            _currentOutput = _fxStack.Draw(PipelineFxStage.Bloom, _currentOutput, null, _ssfxTargets.Bloom_Main);

            // Step: 16
            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.e_IsEditorEnabled && RenderingStats.e_EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, _matrices, gizmoContext, EditorLogic.Instance.HasMouseMoved);

            // Step: 17
            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode(_currentOutput);

            // Step: 18
            //Draw signed distance field functions
            _moduleStack.DistanceField.Draw(camera);

            // Step: 19
            //Additional editor elements that overlay our screen
            RenderEditorOverlays(gizmoContext, scene);

            // Step: 20
            //Draw debug geometry
            _moduleStack.Helper.ViewProjection = _matrices.StaticViewProjection;
            _moduleStack.Helper.Draw();

            // Step: 21
            //Set up the frustum culling for the next frame
            meshBatcher.FrustumCullingFinalizeFrame();

            //Performance Profiler
            _profiler.Sample(ref PipelineSamples.SDraw_TotalRender);

            //return data we have recovered from the editor id, so we know what entity gets hovered/clicked on and can manipulate in the update function
            return new ObjectHoverContext
            {
                HoveredId = _moduleStack.IdAndOutline.HoveredId,
                ViewMatrix = _matrices.View,
                ProjectionMatrix = _matrices.Projection
            };

        }

        private bool IsSDFUsed(List<PointLight> pointLights)
        {
            for (var index = 0; index < pointLights.Count; index++)
            {
                if (pointLights[index].CastSDFShadows)
                {
                    return true;
                }
            }
            return false;
        }

        private void RenderEditorOverlays(GizmoDrawContext gizmoContext, EntitySceneGroup scene)
        {
            if (RenderingSettings.e_IsEditorEnabled && RenderingStats.e_EnableSelection)
            {
                if (IdAndOutlineRenderModule.e_DrawOutlines)
                    _graphicsDevice.Blit(_spriteBatch, _moduleStack.IdAndOutline.GetRenderTarget2D(), null, BlendState.Additive);

                this.DrawEditorPasses(scene, _matrices, gizmoContext, EditorPasses.Billboard | EditorPasses.IdAndOutline);

                if (gizmoContext.SelectedObject != null)
                {
                    if (gizmoContext.SelectedObject is Decal decal)
                    {
                        _moduleStack.Decal.DrawOutlines(decal, _matrices);
                    }
                    if (RenderingSettings.e_DrawBoundingBox
                        && gizmoContext.SelectedObject is ModelEntity entity)
                    {
                        HelperGeometryManager.GetInstance().AddBoundingBox(entity);
                    }
                }
            }

            if (RenderingSettings.SDF.DrawDebug && _moduleStack.DistanceField.GetAtlas() != null)
            {
                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
                _spriteBatch.Draw(_moduleStack.DistanceField.GetAtlas(), new Rectangle(0, RenderingSettings.g_ScreenHeight - 200, RenderingSettings.g_ScreenWidth, 200), Color.White);
                _spriteBatch.End();
            }

        }

        public void DrawEditorPasses(EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext gizmoContext,
            EditorPasses passes = EditorPasses.Billboard | EditorPasses.IdAndOutline)
        {
            // render directly to the output buffer
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            if (passes.HasFlag(EditorPasses.Billboard))
                _moduleStack.Billboard.DrawEditorBillboards(scene, matrices, gizmoContext);
            if (passes.HasFlag(EditorPasses.IdAndOutline))
                _moduleStack.IdAndOutline.DrawTransformGizmos(matrices, gizmoContext, IdAndOutlineRenderModule.Pass.Color);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //  DEFERRED RENDERING FUNCTIONS, IN ORDER OF USAGE
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset our stat counting for this frame
        /// </summary>
        private void ResetStats()
        {
            RenderingStats.MaterialDraws = 0;
            RenderingStats.MeshDraws = 0;
            RenderingStats.LightsDrawn = 0;
            RenderingStats.shadowMaps = 0;
            RenderingStats.activeShadowMaps = 0;
            RenderingStats.EmissiveMeshDraws = 0;

            _profiler.Reset();
        }

        /// <summary>
        /// Check whether any GameSettings have changed that need setup
        /// </summary>
        /// <param name="dirLights"></param>
        private void CheckRenderChanges()
        {

            if (_g_SSReflectionNoise != SSReflectionFx.g_Noise)
            {
                _g_SSReflectionNoise = SSReflectionFx.g_Noise;
                if (!_g_SSReflectionNoise) _fxStack.SSReflection.Time = 0.0f;
            }


            // clear SSReflection buffer if disabled
            if (_prevSSReflectionEnabled != SSReflectionFx.g_Enabled)
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.SSR_Main);
                _graphicsDevice.Clear(new Color(0, 0, 0, 0.0f));
                
                _prevSSReflectionEnabled = SSReflectionFx.g_Enabled;
            }

            //Performance Profiler
            _profiler.Timestamp();

        }

        /// <summary>
        /// Create the projection matrices
        /// </summary>
        private void UpdateViewProjection(DynamicMeshBatcher meshBatcher, Camera camera)
        {
            // ToDo: @tpott: This boolean flag controls general update and draw, though it should only determine if matrices and such should be updated
            //      if a frame should be drawn is a conditioned layered on top of this and may be required regardless of camera change
            bool viewProjectionHasChanged = camera.HasChanged;

            //If the camera didn't do anything we don't need to update this stuff
            if (viewProjectionHasChanged)
            {
                //View matrix
                _matrices.SetFromCamera(camera);
                _moduleStack.PointLight.InverseView = _matrices.InverseView;

                //Temporal AA - alternate frames for temporal anti-aliasing
                if (_fxStack.TemporaAA.Enabled)
                {
                    viewProjectionHasChanged = true;
                    _fxStack.TemporaAA.SwapOffFrame();
                    _matrices.ApplyViewProjectionJitter(_fxStack.TemporaAA.JitterMode, _fxStack.TemporaAA.IsOffFrame, _fxStack.TemporaAA.HaltonSequence);
                }

                _moduleStack.Lighting.UpdateViewProjection(_boundingFrustum, viewProjectionHasChanged, _matrices);

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_matrices.StaticViewProjection);
                else _boundingFrustum.Matrix = _matrices.StaticViewProjection;

                // Compute the frustum corners for cheap view direction computation in shaders
                ComputeFrustumCorners(_boundingFrustum, camera);
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshBatcher.FrustumCulling(_boundingFrustum, viewProjectionHasChanged, camera.Position);

        }

        /// <summary>
        /// From https://jcoluna.wordpress.com/2011/01/18/xna-4-0-light-pre-pass/
        /// Compute the frustum corners for a camera.
        /// Its used to reconstruct the pixel position using only the depth value.
        /// Read here for more information
        /// http://mynameismjp.wordpress.com/2009/03/10/reconstructing-position-from-depth/
        /// </summary>
        private void ComputeFrustumCorners(BoundingFrustum cameraFrustum, Camera camera)
        {
            cameraFrustum.GetCorners(_frustumCorners.WorldSpace);
            Extensions.Transform(_frustumCorners.WorldSpace, _matrices.View, _frustumCorners.ViewSpace); //put the frustum into view space

            /*this part is used for volume projection*/
            //World Space Corners - Camera Position
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _frustumCorners.WorldSpaceFrustum[i] = _frustumCorners.WorldSpace[i + 4] - camera.Position;
                _frustumCorners.ViewSpaceFrustum[i] = _frustumCorners.ViewSpace[i + 4];
            }
            // swap 2 <-> 3
            (_frustumCorners.WorldSpaceFrustum[2], _frustumCorners.WorldSpaceFrustum[3]) = (_frustumCorners.WorldSpaceFrustum[3], _frustumCorners.WorldSpaceFrustum[2]);

            _moduleStack.DistanceField.FrustumCornersWorldSpace = _frustumCorners.WorldSpaceFrustum;
            _moduleStack.Environment.FrustumCornersWS = _frustumCorners.WorldSpaceFrustum;

            //View Space Corners
            // swap 2 <-> 3
            (_frustumCorners.ViewSpaceFrustum[2], _frustumCorners.ViewSpaceFrustum[3]) = (_frustumCorners.ViewSpaceFrustum[3], _frustumCorners.ViewSpaceFrustum[2]);

            _moduleStack.DepthReconstruct.FrustumCorners = _frustumCorners.ViewSpaceFrustum;
            //_moduleStack.Lighting.FrustumCorners = _frustumCorners.ViewSpaceFrustum;
            _moduleStack.DirectionalLight.FrustumCorners = _frustumCorners.ViewSpaceFrustum;
            _fxStack.FrustumCorners = _frustumCorners.ViewSpaceFrustum;
        }

        /// <summary>
        /// Draw deferred Decals
        /// </summary>
        private void DrawDecals(List<Decal> decals)
        {
            if (!DecalRenderModule.g_EnableDecals) return;

            //First copy albedo to decal offtarget
            _graphicsDevice.Blit(_spriteBatch, _gBufferTarget.Albedo, _auxTargets[PipelineTargets.DECAL], _supersampling);
            _graphicsDevice.Blit(_spriteBatch, _auxTargets[PipelineTargets.DECAL], _gBufferTarget.Albedo, _supersampling);

            _moduleStack.Decal.Draw(decals, _matrices);
        }

        /// <summary>
        /// Apply our environment cubemap to the renderer
        /// </summary>
        private void DrawEnvironmentMap(EnvironmentProbe envProbe, Camera camera, GameTime gameTime)
        {
            if (!RenderingSettings.EnvironmentMapping.Enabled) return;

            _moduleStack.Environment.SetEnvironmentProbe(envProbe);
            _moduleStack.Environment.DrawEnvironmentMap(camera, _matrices.View, gameTime);

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_EnvironmentMap);

        }

        /// <summary>
        /// Compose the render by combining the albedo channel with the light channels
        /// </summary>
        private RenderTarget2D DrawDeferredCompose(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            // ToDo: @tpott: hacky way to disable ssao when disabled on global scale (GUI is insufficient here)
            _moduleStack.Deferred.UseSSAOMap = SSAmbientOcclustionFx.g_ssao_draw;
            _moduleStack.Deferred.Draw(destRT);

            return destRT;
        }


        private RenderTarget2D DrawForward(RenderTarget2D destRT, DynamicMeshBatcher meshBatcher, Camera camera, List<PointLight> pointLights)
        {
            if (!ForwardPipelineModule.g_EnableForward)
                return destRT;

            _graphicsDevice.SetRenderTarget(destRT);
            _moduleStack.DepthReconstruct.ReconstructDepth();
            _moduleStack.Forward.SetupLighting(camera, pointLights, _boundingFrustum);
            _moduleStack.Forward.Draw(meshBatcher, _matrices);
            return destRT;
        }


        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        private void RenderMode(RenderTarget2D currentTarget)
        {
            switch (RenderingSettings.g_RenderMode)
            {
                case RenderModes.Albedo:
                    BlitTo(_gBufferTarget.Albedo);
                    break;
                case RenderModes.Normal:
                    BlitTo(_gBufferTarget.Normal);
                    break;
                case RenderModes.Depth:
                    BlitTo(_gBufferTarget.Depth);
                    break;
                case RenderModes.Diffuse:
                    BlitTo(_lightingBufferTarget.Diffuse);
                    break;
                case RenderModes.Specular:
                    BlitTo(_lightingBufferTarget.Specular);
                    break;
                case RenderModes.Volumetric:
                    BlitTo(_lightingBufferTarget.Volume);
                    break;
                case RenderModes.SSAO:
                    BlitTo(_ssfxTargets.AO_Main);
                    break;
                case RenderModes.SSBlur:
                    BlitTo(_ssfxTargets.AO_Blur_Final);
                    break;
                case RenderModes.SSR:
                    BlitTo(_ssfxTargets.SSR_Main);
                    break;
                case RenderModes.HDR:
                    BlitTo(currentTarget);
                    break;
                default:
                    _fxStack.Draw(PipelineFxStage.PostProcessing, currentTarget, null, _auxTargets[PipelineTargets.OUTPUT]);
                    _fxStack.Draw(PipelineFxStage.ColorGrading, null, null, _auxTargets[PipelineTargets.OUTPUT]);
                    break;
            }

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_FinalRender);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void SetUpRenderTargets(Vector2 resolution) => SetUpRenderTargets((int)resolution.X, (int)resolution.Y);
        private void SetUpRenderTargets(int width, int height)
        {
            int targetWidth = (int)(width * _supersampling);
            int targetHeight = (int)(height * _supersampling);

            Vector2 targetResolution = new Vector2(targetWidth, targetHeight);

            // Update multi render target size
            _gBufferTarget.Resize(targetWidth, targetHeight);
            _lightingBufferTarget.Resize(targetWidth, targetHeight);
            _auxTargets.Resize(targetWidth, targetHeight);

            _moduleStack.PointLight.Resolution = targetResolution;

            _moduleStack.Billboard.AspectRatio = (float)targetWidth / targetHeight;
            _moduleStack.IdAndOutline.SetUpRenderTarget(width, height);

            _fxStack.TemporaAA.Resolution = targetResolution;
            _fxStack.SSReflection.Resolution = targetResolution;

            _moduleStack.Environment.Resolution = targetResolution;

            ///////////////////
            // HALF RESOLUTION

            targetWidth /= 2;
            targetHeight /= 2;

            Vector2 aspectRatios = new Vector2(Math.Min(1.0f, targetWidth / (float)targetHeight), Math.Min(1.0f, targetHeight / (float)targetWidth));
            _fxStack.SSAmbientOcclusion.InverseResolution = new Vector2(1.0f / targetWidth, 1.0f / targetHeight);
            _fxStack.SSAmbientOcclusion.AspectRatios = aspectRatios;


            UpdateRenderMapBindings();
        }

        private void UpdateRenderMapBindings()
        {
            _moduleStack.SetGBufferParams(_gBufferTarget);
            // update directional light module
            _moduleStack.DirectionalLight.SetScreenSpaceShadowMap(_ssfxTargets.AO_Blur_Final);

            _moduleStack.Environment.SSRMap = _ssfxTargets.SSR_Main;

            _moduleStack.Deferred.SetLightingParams(_lightingBufferTarget);
            _moduleStack.Deferred.SetSSAOMap(_ssfxTargets.AO_Blur_Final);

            _fxStack.SetGBufferParams(_gBufferTarget);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void BlitTo(Texture2D source, RenderTarget2D destRT = null)
        {
            _graphicsDevice.Blit(_spriteBatch, source, destRT, _supersampling);
        }


        public void Dispose()
        {
            _graphicsDevice?.Dispose();
            _spriteBatch?.Dispose();

            _moduleStack?.Dispose();
            _fxStack?.Dispose();

            _gBufferTarget?.Dispose();
            _lightingBufferTarget?.Dispose();
            _auxTargets?.Dispose();

            _currentOutput?.Dispose();
            _renderTargetCubeMap?.Dispose();
        }

    }

}

