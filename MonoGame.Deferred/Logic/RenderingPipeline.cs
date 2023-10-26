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

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Rendering
{


    [Flags()]
    public enum EditorPasses
    {
        Billboard = 1,
        IdAndOutline = 2,
        Helper = 4,
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
        private BoundingFrustumWithVertices _frustum = new BoundingFrustumWithVertices();

        //Render targets
        private GBufferTarget _gBufferTarget;
        private LightingBufferTarget _lightingBufferTarget;
        private PipelineTargets _auxTargets;
        private SSFxTargets _ssfxTargets;

        // Final output
        private RenderTarget2D _currentOutput;

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
            _moduleStack.Matrices = _matrices;
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

            _gBufferTarget = new GBufferTarget(graphicsDevice, RenderingSettings.Screen.g_Width, RenderingSettings.Screen.g_Height);
            _lightingBufferTarget = new LightingBufferTarget(graphicsDevice, RenderingSettings.Screen.g_Width, RenderingSettings.Screen.g_Height);
            _auxTargets = new PipelineTargets(graphicsDevice, RenderingSettings.Screen.g_Width, RenderingSettings.Screen.g_Height);
            _ssfxTargets = new SSFxTargets(graphicsDevice, RenderingSettings.Screen.g_Width, RenderingSettings.Screen.g_Height);

            _moduleStack.Initialize(graphicsDevice, _spriteBatch);
            _moduleStack.SetGBufferParams(_gBufferTarget);
            _moduleStack.LightingBufferTarget = _lightingBufferTarget;
            _moduleStack.SSFxTargets = _ssfxTargets;

            _fxStack.Initialize(graphicsDevice, _spriteBatch);
            _fxStack.SetGBufferParams(_gBufferTarget);
            _fxStack.SSFxTargets = _ssfxTargets;

            // update directional light module
            SetUpRenderTargets(RenderingSettings.Screen.g_Resolution);


            RenderingSettings.g_FarClip.Changed += FarClip_OnChanged;
            RenderingSettings.g_FarClip.Set(500);
            SSReflectionFx.gg_Enabled.Changed += SSR_Enabled_Changed;
            RenderingSettings.Bloom.Threshold = 0.0f;
        }

        private void SSR_Enabled_Changed(bool enabled)
        {
            // clear SSReflection buffer if disabled/enabled
            if (!enabled)
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.SSR_Main);
                _graphicsDevice.Clear(new Color(0, 0, 0, 0.0f));
            }
        }

        private void FarClip_OnChanged(float farClip)
        {
            _moduleStack.FarClip = farClip;
            _fxStack.FarClip = farClip;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update our function
        /// </summary>
        public void Update(Camera camera, DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, GameTime gameTime, bool isActive)
        {
            if (!isActive)
                return;

            _moduleStack.DistanceField.UpdateSdfGenerator(scene.Entities);
            _moduleStack.Lighting.UpdateGameTime(gameTime);
            if (SSReflectionFx.gg_Noise)
                _fxStack.SSReflection.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            _moduleStack.Environment.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;


            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();
            //Performance Profiler Reset
            _profiler.Timestamp();

            // Step: 04
            //Update our view projection matrices if the camera moved
            if (camera.HasChanged)
            {
                UpdateViewProjection(camera);
                //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
                meshBatcher.FrustumCulling(_frustum.Frustum, camera.HasChanged, camera.Position);
                // Compute the frustum corners for cheap view direction computation in shaders
                UpdateFrustumCorners(camera);
            }
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SUpdate_ViewProjection);

        }
        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public void Draw(Camera camera, DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, GizmoDrawContext gizmoContext)
        {
            // Step: 02
            //Render ShadowMaps
            _moduleStack.ShadowMap.Draw(meshBatcher, scene);
            //Performance Profile
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Shadows);

            // Step: 03
            //Update SDFs
            if (IsSDFUsed(scene.PointLights))
                _moduleStack.DistanceField.UpdateDistanceFieldTransformations(scene.Entities);

            // Step: 05
            //Draw our meshes to the G Buffer
            _moduleStack.GBuffer.Draw(meshBatcher);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_GBuffer);


            // Step: 06
            //Deferred Decals
            if (DecalRenderModule.g_EnableDecals)
            {
                //First copy albedo to decal offtarget
                _graphicsDevice.Blit(_spriteBatch, _gBufferTarget.Albedo, _auxTargets[PipelineTargets.DECAL]);
                _graphicsDevice.Blit(_spriteBatch, _auxTargets[PipelineTargets.DECAL], _gBufferTarget.Albedo);
                _moduleStack.Decal.Draw(scene.Decals);
            }

            // Step: 07
            //Draw Screen Space reflections to a different render target
            RenderTarget2D ssrTargetMap = _ssfxTargets.GetSSReflectionRenderTargets(_fxStack.TemporaAA.Enabled, _fxStack.TemporaAA.IsOffFrame);
            _fxStack.SSReflection.TargetMap = ssrTargetMap ?? _auxTargets[PipelineTargets.COMPOSE];
            _fxStack.Draw(PipelineFxStage.SSReflection, null, null, _ssfxTargets.SSR_Main);
            // Profiler sample
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSR);


            // Step: 08
            //SSAO
            _fxStack.SSAmbientOcclusion.SetViewPosition(camera.Position);
            _fxStack.Draw(PipelineFxStage.SSAmbientOcclusion, null, null, _ssfxTargets.AO_Main);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSAO);

            // Step: 10
            //Light the scene
            //_moduleStack.Lighting.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _matrices);
            _moduleStack.Lighting.DrawLights(scene, camera.Position, camera.HasChanged);

            // Step: 11
            //Draw the environment cube map as a fullscreen effect on all meshes
            if (RenderingSettings.EnvironmentMapping.Enabled)
            {
                _moduleStack.Environment.SetEnvironmentProbe(scene.EnvProbe);
                _moduleStack.Environment.DrawEnvironmentMap(camera);
                //Performance Profiler
                _profiler.SampleTimestamp(ref PipelineSamples.SDraw_EnvironmentMap);
            }


            // Step: 12
            //Compose the scene by combining our lighting data with the gbuffer data
            // ToDo: @tpott: hacky way to disable ssao when disabled on global scale (GUI is insufficient here)
            _moduleStack.Deferred.UseSSAOMap = SSAmbientOcclustionFx.g_ssao_draw;
            _moduleStack.Deferred.Draw(_auxTargets[PipelineTargets.COMPOSE]);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Compose);

            // Step: 13
            //Forward
            if (ForwardPipelineModule.g_EnableForward)
            {
                _graphicsDevice.SetRenderTarget(_auxTargets[PipelineTargets.COMPOSE]);
                _moduleStack.DepthReconstruct.ReconstructDepth();
                _moduleStack.Forward.SetupLighting(camera, scene.PointLights, _frustum.Frustum);
                _moduleStack.Forward.Draw(meshBatcher);
            }
            _currentOutput = _auxTargets[PipelineTargets.COMPOSE];


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
            if (RenderingSettings.e_IsEditorEnabled && RenderingSettings.e_EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, gizmoContext, EditorLogic.Instance.HasMouseMoved);

            // Step: 17
            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            DrawRenderMode(_currentOutput, null, _auxTargets[PipelineTargets.OUTPUT]);

            // Step: 18
            //Draw signed distance field functions
            _moduleStack.DistanceField.Draw(camera);

            // Step: 19
            //Additional editor elements that overlay our screen
            DrawEditorOverlays(gizmoContext, scene);

            // Step: 20
            //Draw debug geometry
            DrawEditorPasses(scene, gizmoContext, EditorPasses.Helper);


            // Step: 21
            //Set up the frustum culling for the next frame
            meshBatcher.FrustumCullingFinalizeFrame();

            //Performance Profiler
            _profiler.Sample(ref PipelineSamples.SDraw_TotalRender);

        }
        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public ObjectHoverContext GetHoverContext()
        {
            //return data we have recovered from the editor id, so we know what entity gets hovered/clicked on and can manipulate in the update function
            return new ObjectHoverContext(_moduleStack.IdAndOutline.HoveredId, _matrices);

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

        private void DrawEditorOverlays(GizmoDrawContext gizmoContext, EntitySceneGroup scene)
        {
            if (RenderingSettings.e_IsEditorEnabled && RenderingSettings.e_EnableSelection)
            {
                if (IdAndOutlineRenderModule.e_DrawOutlines)
                    _graphicsDevice.Blit(_spriteBatch, _moduleStack.IdAndOutline.GetRenderTarget2D(), null, BlendState.Additive);

                this.DrawEditorPasses(scene, gizmoContext, EditorPasses.Billboard | EditorPasses.IdAndOutline);

                if (gizmoContext.SelectedObject != null)
                {
                    if (gizmoContext.SelectedObject is Decal decal)
                    {
                        _moduleStack.Decal.DrawOutlines(decal);
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
                _spriteBatch.Draw(_moduleStack.DistanceField.GetAtlas(), new Rectangle(0, RenderingSettings.Screen.g_Height - 200, RenderingSettings.Screen.g_Width, 200), Color.White);
                _spriteBatch.End();
            }

        }

        private void DrawEditorPasses(EntitySceneGroup scene, GizmoDrawContext gizmoContext,
            EditorPasses passes = EditorPasses.Billboard | EditorPasses.IdAndOutline)
        {
            // render directly to the output buffer
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            if (passes.HasFlag(EditorPasses.Billboard))
                _moduleStack.Billboard.DrawEditorBillboards(scene, gizmoContext);
            if (passes.HasFlag(EditorPasses.IdAndOutline))
                _moduleStack.IdAndOutline.DrawTransformGizmos(gizmoContext, IdAndOutlineRenderModule.Pass.Color);
            if (passes.HasFlag(EditorPasses.Helper))
                _moduleStack.Helper.Draw();

        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //  DEFERRED RENDERING FUNCTIONS, IN ORDER OF USAGE
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset our stat counting for this frame
        /// </summary>
        private void ResetStats()
        {
            RenderingStats.ResetStats();
            _profiler.Reset();
        }

        /// <summary>
        /// Create the projection matrices
        /// </summary>
        private void UpdateViewProjection(Camera camera)
        {
            // ToDo: @tpott: This boolean flag controls general update and draw, though it should only determine if matrices and such should be updated
            //      if a frame should be drawn is a conditioned layered on top of this and may be required regardless of camera change
            bool hasChanged = camera.HasChanged;

            //If the camera didn't do anything we don't need to update this stuff
            if (camera.HasChanged)
            {
                //View matrix
                _matrices.SetFromCamera(camera);

                //Temporal AA - alternate frames for temporal anti-aliasing
                if (_fxStack.TemporaAA.Enabled)
                {
                    hasChanged = true;
                    _fxStack.TemporaAA.SwapOffFrame();
                    _matrices.ApplyViewProjectionJitter(_fxStack.TemporaAA.JitterMode, _fxStack.TemporaAA.IsOffFrame, _fxStack.TemporaAA.HaltonSequence);
                }

                _moduleStack.Lighting.UpdateViewProjection(_frustum.Frustum, hasChanged);
                _frustum.Frustum.Matrix = _matrices.StaticViewProjection;
            }

        }

        /// <summary>
        /// From https://jcoluna.wordpress.com/2011/01/18/xna-4-0-light-pre-pass/
        /// Compute the frustum corners for a camera.
        /// Its used to reconstruct the pixel position using only the depth value.
        /// Read here for more information
        /// http://mynameismjp.wordpress.com/2009/03/10/reconstructing-position-from-depth/
        /// </summary>
        private void UpdateFrustumCorners(Camera camera)
        {
            _frustum.UpdateVertices(_matrices.View, camera.Position);

            //World Space Corners
            _moduleStack.FrustumCornersWS = _frustum.WorldSpaceFrustum;
            //View Space Corners
            _moduleStack.FrustumCornersVS = _frustum.ViewSpaceFrustum;
            _fxStack.FrustumCorners = _frustum.ViewSpaceFrustum;
        }


        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        private void DrawRenderMode(RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
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
                    BlitTo(sourceRT);
                    break;
                default:
                    _fxStack.Draw(PipelineFxStage.PostProcessing, sourceRT, null, destRT);
                    _fxStack.Draw(PipelineFxStage.ColorGrading, destRT, null, null);
                    break;
            }

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_FinalRender);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void SetUpRenderTargets(Vector2 resolution)
        {
            int targetWidth = (int)(resolution.X);
            int targetHeight = (int)(resolution.Y);

            // Update multi render target size
            _gBufferTarget.Resize(targetWidth, targetHeight);
            _lightingBufferTarget.Resize(targetWidth, targetHeight);
            _auxTargets.Resize(targetWidth, targetHeight);

            _moduleStack.PointLight.Resolution = resolution;
            _moduleStack.Environment.Resolution = resolution;

            _moduleStack.Billboard.AspectRatio = (float)targetWidth / targetHeight;
            _moduleStack.IdAndOutline.SetUpRenderTarget(resolution);

            _fxStack.TemporaAA.Resolution = resolution;
            _fxStack.SSReflection.Resolution = resolution;


            ///////////////////
            // HALF RESOLUTION
            targetWidth /= 2;
            targetHeight /= 2;

            Vector2 aspectRatios = new Vector2(Math.Min(1.0f, targetWidth / (float)targetHeight), Math.Min(1.0f, targetHeight / (float)targetWidth));
            _fxStack.SSAmbientOcclusion.InverseResolution = new Vector2(1.0f / targetWidth, 1.0f / targetHeight);
            _fxStack.SSAmbientOcclusion.AspectRatios = aspectRatios;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void BlitTo(Texture2D source, RenderTarget2D destRT = null)
        {
            _graphicsDevice.Blit(_spriteBatch, source, destRT);
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
        }

    }

}

