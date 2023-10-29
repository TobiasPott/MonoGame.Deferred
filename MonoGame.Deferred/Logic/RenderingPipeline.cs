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
        public event Action<DrawEvents> EventTriggered;

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
        private PipelineFrustum _frustum = new PipelineFrustum();

        //Render targets
        private GBufferTarget _gBufferTarget;
        private LightingBufferTarget _lightingBufferTarget;
        private PipelineTargets _auxTargets;
        private SSFxTargets _ssfxTargets;

        // Final output
        private RenderTarget2D _currentOutput;

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public ObjectHoverContext CurrentHoverContext => new ObjectHoverContext(_moduleStack.IdAndOutline.HoveredId, _matrices);


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
            _moduleStack.Frustum = _frustum;
            _profiler = new PipelineProfiler();

            _fxStack = new PipelineFxStack(content);
            _fxStack.Matrices = _matrices;
            _fxStack.Frustum = _frustum;

        }

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);

            Vector2 resolution = RenderingSettings.Screen.g_Resolution;
            _gBufferTarget = new GBufferTarget(graphicsDevice, resolution);
            _lightingBufferTarget = new LightingBufferTarget(graphicsDevice, resolution);
            _auxTargets = new PipelineTargets(graphicsDevice, resolution);
            _ssfxTargets = new SSFxTargets(graphicsDevice, resolution);

            _moduleStack.Initialize(graphicsDevice, _spriteBatch);
            _moduleStack.GBufferTarget = _gBufferTarget;
            _moduleStack.LightingBufferTarget = _lightingBufferTarget;
            _moduleStack.SSFxTargets = _ssfxTargets;

            _fxStack.Initialize(graphicsDevice, _spriteBatch);
            _fxStack.GBufferTarget = _gBufferTarget;
            _fxStack.SSFxTargets = _ssfxTargets;

            // update directional light module
            SetResolution(resolution);


            RenderingSettings.g_FarClip.Changed += FarClip_OnChanged;
            RenderingSettings.g_FarClip.Set(500);
            SSReflectionFx.g_Enabled.Changed += SSR_Enabled_Changed;
            RenderingSettings.Bloom.Threshold.Set(0.0f);
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
            _frustum.FarClip = farClip;
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
            if (SSReflectionFx.g_Noise)
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
                meshBatcher.FrustumCulling(_frustum.Frustum, camera.HasChanged);
                // Compute the frustum corners for cheap view direction computation in shaders
                _frustum.UpdateVertices(_matrices.View, camera.Position);
            }

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SUpdate_ViewProjection);


            // Step: 03
            //Update SDFs
            if (IsSDFUsed(scene.PointLights))
                _moduleStack.DistanceField.UpdateDistanceFieldTransformations(scene.Entities);

        }

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public void Draw(Camera camera, DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, GizmoDrawContext gizmoContext)
        {
            // Step: 01
            //Render ShadowMaps
            _moduleStack.ShadowMap.Draw(meshBatcher, scene);
            //Performance Profile
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Shadows);

            // Step: 02
            //Draw our meshes to the G Buffer
            _moduleStack.GBuffer.Draw(meshBatcher);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_GBuffer);


            // Step: 03
            //Deferred Decals
            if (DecalRenderModule.g_EnableDecals)
            {
                //First copy albedo to decal offtarget
                _moduleStack.Decal.Blit(_gBufferTarget.Albedo, _auxTargets[PipelineTargets.DECAL]);
                _moduleStack.Decal.Blit(_auxTargets[PipelineTargets.DECAL], _gBufferTarget.Albedo);
                _moduleStack.Decal.Draw(scene);
            }

            // STAGE: PreLighting-SSFx
            // Step: 04
            //Draw Screen Space reflections to a different render target
            RenderTarget2D ssrTargetMap = _ssfxTargets.GetSSReflectionRenderTargets(_fxStack.TemporalAA.Enabled, _fxStack.TemporalAA.IsOffFrame);
            _fxStack.SSReflection.TargetMap = ssrTargetMap ?? _auxTargets[PipelineTargets.COMPOSE];
            _fxStack.Draw(PipelineFxStage.SSReflection, null, null, _ssfxTargets.SSR_Main);
            // Profiler sample
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSR);

            // STAGE: PreLighting-SSFx
            // Step: 05
            //SSAO
            _fxStack.SSAmbientOcclusion.SetViewPosition(camera.Position);
            _fxStack.Draw(PipelineFxStage.SSAmbientOcclusion, null, null, _ssfxTargets.AO_Main);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSAO);

            // STAGE: Lighting
            // Step: 06
            //Light the scene
            //// ToDo: PRIO I: Extract camera.HasChanged and Position and move to reference inside the module
            _moduleStack.Lighting.Draw(scene, camera.Position, camera.HasChanged);

            // ToDo: PRIO II: Is Environment module actually part of lighting? (unsure ahout the sky part though)
            //              I mmight need to split it into Environment and Sky
            // Step: 07
            //Draw the environment cube map as a fullscreen effect on all meshes
            if (RenderingSettings.Environment.Enabled)
            {
                _moduleStack.Environment.SetEnvironmentProbe(scene.EnvProbe);
                _moduleStack.Environment.SetViewPosition(camera.Position);
                _moduleStack.Environment.Draw();
                //Performance Profiler
                _profiler.SampleTimestamp(ref PipelineSamples.SDraw_EnvironmentMap);
            }


            // Step: 08
            //Compose the scene by combining our lighting data with the gbuffer data
            // ToDo: PRIO III: @tpott: hacky way to disable ssao when disabled on global scale (GUI is insufficient here)
            //      Add NotifiedProperty with wrapper property for UI
            _moduleStack.Deferred.UseSSAOMap = SSAmbientOcclustionFx.g_ssao_draw;
            _moduleStack.Deferred.Draw(_auxTargets[PipelineTargets.COMPOSE]);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Compose);

            // Step: 09
            //Forward
            if (ForwardPipelineModule.g_EnableForward)
            {
                _graphicsDevice.SetRenderTarget(_auxTargets[PipelineTargets.COMPOSE]);
                _moduleStack.DepthReconstruct.ReconstructDepth();
                _moduleStack.Forward.SetupLighting(camera, scene.PointLights, _frustum.Frustum);
                _moduleStack.Forward.Draw(meshBatcher);
            }

            // Step: 10
            //Compose the image and add information from previous frames to apply temporal super sampling
            _ssfxTargets.GetTemporalAARenderTargets(_fxStack.TemporalAA.IsOffFrame, out RenderTarget2D taaDestRT, out RenderTarget2D taaPreviousRT);
            _currentOutput = _fxStack.Draw(PipelineFxStage.TemporalAA, _auxTargets[PipelineTargets.COMPOSE], taaPreviousRT, taaDestRT);
            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_CombineTAA);

            // Step: 11
            //Do Bloom
            _currentOutput = _fxStack.Draw(PipelineFxStage.Bloom, _currentOutput, null, _ssfxTargets.Bloom_Main);


            // Step: 14
            //Draw signed distance field functions
            _moduleStack.DistanceField.SetViewPosition(camera.Position);
            _moduleStack.DistanceField.Draw();

            //Performance Profiler
            _profiler.Sample(ref PipelineSamples.SDraw_TotalRender);

            // Step: 12
            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.e_IsEditorEnabled && RenderingSettings.e_EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, gizmoContext, EditorLogic.Instance.HasMouseMoved);

            // Step: 13
            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            DrawPipelinePass(RenderingSettings.g_CurrentPass, _currentOutput, null, _auxTargets[PipelineTargets.OUTPUT]);

            // Step: 15
            //Additional editor elements that overlay our screen
            DrawEditorOverlays(gizmoContext, scene);

        }

        //Render modes
        public enum DrawEvents
        {
            ShadowMap,
            GBuffer,
            Decal,
            FxReflection,
            FxAmbientOcclusion,
            Lighting,
            Environment,
            Deferred,
            Forward,
            FxTAA,
            Bloom,
        };

        private bool IsSDFUsed(List<PointLight> pointLights)
        {
            if (!RenderingSettings.SDF.DrawDistance)
                return false;

            foreach (PointLight light in pointLights)
                if (light.HasChanged && light.CastSDFShadows)
                    return true;
            return false;
        }

        private void DrawEditorOverlays(GizmoDrawContext gizmoContext, EntitySceneGroup scene)
        {
            if (RenderingSettings.e_IsEditorEnabled && RenderingSettings.e_EnableSelection)
            {
                if (IdAndOutlineRenderModule.e_DrawOutlines)
                    _moduleStack.IdAndOutline.Blit(_moduleStack.IdAndOutline.Target, null, BlendState.Additive);

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

            if (RenderingSettings.SDF.DrawDebug && _moduleStack.DistanceField.AtlasTarget != null)
            {
                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
                _spriteBatch.Draw(_moduleStack.DistanceField.AtlasTarget, new Rectangle(0, RenderingSettings.Screen.g_Height - 200, RenderingSettings.Screen.g_Width, 200), Color.White);
                _spriteBatch.End();
            }

            // Step: 20
            //Draw debug geometry
            DrawEditorPasses(scene, gizmoContext, EditorPasses.Helper);

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
                if (_fxStack.TemporalAA.Enabled)
                {
                    hasChanged = true;
                    _fxStack.TemporalAA.SwapOffFrame();
                    _matrices.ApplyViewProjectionJitter(_fxStack.TemporalAA.JitterMode, _fxStack.TemporalAA.IsOffFrame, _fxStack.TemporalAA.HaltonSequence);
                }
        ;
                _frustum.Frustum.Matrix = _matrices.StaticViewProjection;
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void SetResolution(Vector2 resolution)
        {
            // Update multi render target size
            _gBufferTarget.Resize(resolution);
            _lightingBufferTarget.Resize(resolution);
            _auxTargets.Resize(resolution);
            _ssfxTargets.Resize(resolution);

            _moduleStack.Resolution = resolution;
            _fxStack.Resolution = resolution;
        }

        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        private void DrawPipelinePass(PipelineOutputPasses pass, RenderTarget2D sourceRT, RenderTarget2D previousRT, RenderTarget2D destRT)
        {
            switch (pass)
            {
                case PipelineOutputPasses.Albedo:
                    BlitTo(_gBufferTarget.Albedo);
                    break;
                case PipelineOutputPasses.Normal:
                    BlitTo(_gBufferTarget.Normal);
                    break;
                case PipelineOutputPasses.Depth:
                    BlitTo(_gBufferTarget.Depth);
                    break;
                case PipelineOutputPasses.Diffuse:
                    BlitTo(_lightingBufferTarget.Diffuse);
                    break;
                case PipelineOutputPasses.Specular:
                    BlitTo(_lightingBufferTarget.Specular);
                    break;
                case PipelineOutputPasses.Volumetric:
                    BlitTo(_lightingBufferTarget.Volume);
                    break;
                case PipelineOutputPasses.SSAO:
                    BlitTo(_ssfxTargets.AO_Main);
                    break;
                case PipelineOutputPasses.SSBlur:
                    BlitTo(_ssfxTargets.AO_Blur_Final);
                    break;
                case PipelineOutputPasses.SSR:
                    BlitTo(_ssfxTargets.SSR_Main);
                    break;
                case PipelineOutputPasses.HDR:
                    BlitTo(sourceRT);
                    break;
                default:
                    // ToDo: PRIO IV: PostProcessing should blit source to dest if disabled
                    _fxStack.Draw(PipelineFxStage.PostProcessing, sourceRT, null, destRT);
                    _fxStack.Draw(PipelineFxStage.ColorGrading, destRT, null, null);
                    break;
            }

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_FinalRender);
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

