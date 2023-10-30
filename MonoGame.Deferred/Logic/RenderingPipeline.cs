﻿using DeferredEngine.Entities;
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
    public enum DrawEvents
    {
        // BeforeAll
        ShadowMap,
        // PreGBuffer
        GBuffer,
        // PostGBuffer
        Decal,
        FxReflection,
        FxAmbientOcclusion,
        // PreLighting
        Lighting,
        // PostLighting
        Environment,
        // PreDefeerred
        Deferred,
        // PostDeferred
        // PreForward
        Forward,
        // PostForward
        // ScreenSpace
        FxTAA,
        Bloom,
        // AfterAll
    };

    public partial class RenderingPipeline : IDisposable
    {
        public event Action<DrawEvents> EventTriggered;
        public bool Enabled { get; set; } = true;



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
            _profiler = new PipelineProfiler();

            _moduleStack = new PipelineModuleStack();
            _moduleStack.Matrices = _matrices;
            _moduleStack.Frustum = _frustum;
            _moduleStack.Profiler = _profiler;

            _fxStack = new PipelineFxStack(content);
            _fxStack.Matrices = _matrices;
            _fxStack.Frustum = _frustum;
            _fxStack.Profiler = _profiler;

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
            SSReflectionFx.ModuleEnabled.Changed += SSR_Enabled_Changed;
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

        public void UpdateResolution()
        {
            this.SetResolution(RenderingSettings.Screen.g_Resolution);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool _redrawRequested = false;
        public void RequestRedraw(GameTime gameTime)
        {
            _redrawRequested = true;

            _moduleStack.Lighting.UpdateGameTime(gameTime);
            if (SSReflectionFx.g_Noise)
                _fxStack.SSReflection.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            _moduleStack.Environment.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
        }

        /// <summary>
        /// Update our function
        /// </summary>
        public void Update(Camera camera, DynamicMeshBatcher meshBatcher, EntityScene scene, GizmoDrawContext gizmoContext)
        {
            if (!this.Enabled)
                return;

            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();

            // Step: 04
            //Update our view projection matrices if the camera moved
            if (_redrawRequested)
            {
                UpdateViewProjection(camera);
                //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
                meshBatcher.FrustumCulling(_frustum.Frustum, _redrawRequested);
                // Compute the frustum corners for cheap view direction computation in shaders
                _frustum.UpdateVertices(_matrices.View, camera.Position);

                _moduleStack.Lighting.SetViewPosition(camera.Position);
                _moduleStack.Lighting.RequestRedraw();
                _moduleStack.Environment.SetViewPosition(camera.Position);
                _moduleStack.Environment.SetEnvironmentProbe(scene.EnvProbe);
                _moduleStack.Forward.SetupLighting(camera, scene.PointLights, _frustum.Frustum);
                _moduleStack.DistanceField.SetViewPosition(camera.Position);

                _fxStack.SSAmbientOcclusion.SetViewPosition(camera.Position);
            }

            //Performance Profiler
            _profiler.SampleTimestamp(TimestampIndices.Update_ViewProjection);


            // Step: 03
            //Update SDFs
            if (IsSDFUsed(scene.PointLights))
            {
                _moduleStack.DistanceField.UpdateSdfGenerator(scene.Entities);
                _moduleStack.DistanceField.UpdateDistanceFieldTransformations(scene.Entities);
            }
            //Performance Profiler
            _profiler.SampleTimestamp(TimestampIndices.Update_SDF);



            // Step: 15
            //Additional editor elements that overlay our screen
            if (gizmoContext.SelectedObject != null)
            {
                if (gizmoContext.SelectedObject is Decal decal)
                    _moduleStack.Decal.DrawOutlines(decal);
                else if (RenderingSettings.e_DrawBoundingBox && gizmoContext.SelectedObject is ModelEntity entity)
                    HelperGeometryManager.GetInstance().AddBoundingBox(entity);
            }


        }

        // ! ! ! ! ! ! ! ! ! ! !
        // ToDo: PRIO I: Reduce Draw method to only call profiler and .Draw calls from modules
        //              Setup and processing should be internalized and done before any drawing takes place

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public void Draw(DynamicMeshBatcher meshBatcher, EntityScene scene, GizmoDrawContext gizmoContext)
        {
            if (!this.Enabled)
                return;

            // Step: 12
            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.e_EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, gizmoContext, EditorLogic.Instance.HasMouseMoved);

            // Step: 01
            //Render SHADOW MAPS
            _moduleStack.ShadowMap.Draw(meshBatcher, scene);
            // Step: 02
            //Draw our meshes to the G Buffer
            _moduleStack.GBuffer.Draw(meshBatcher);
            // Step: 03
            //Deferred Decals
            _moduleStack.Decal.Draw(scene, null, _auxTargets[PipelineTargets.DECAL], _gBufferTarget.Albedo);
            // STAGE: PreLighting-SSFx
            // Step: 04
            //Draw Screen Space reflections to a different render target
            _fxStack.Draw(PipelineFxStage.SSReflection, _auxTargets[PipelineTargets.COMPOSE], null, _ssfxTargets.SSR_Main);
            // Step: 05
            //SSAO
            _fxStack.Draw(PipelineFxStage.SSAmbientOcclusion, null, null, _ssfxTargets.AO_Main);
            // Step: 06
            //Light the scene
            _moduleStack.Lighting.Draw(scene);
            // ToDo: PRIO II: Is Environment module actually part of lighting? (unsure ahout the sky part though)
            //              I mmight need to split it into Environment and Sky
            // Step: 07
            //Draw the environment cube map as a fullscreen effect on all meshes
            _moduleStack.Environment.Draw();
            // Step: 08
            //Compose the scene by combining our lighting data with the gbuffer data
            // ToDo: PRIO III: @tpott: hacky way to disable ssao when disabled on global scale (GUI is insufficient here)
            //      Add NotifiedProperty with wrapper property for UI
            _moduleStack.Deferred.Draw(null, null, _auxTargets[PipelineTargets.COMPOSE]);
            // Step: 09
            //Forward
            _moduleStack.Forward.Draw(meshBatcher, null, null, _auxTargets[PipelineTargets.COMPOSE]);
            // Step: 10
            // Compose the image and add information from previous frames to apply temporal super sampling
            _fxStack.Draw(PipelineFxStage.TemporalAA, null, null, _auxTargets[PipelineTargets.COMPOSE]);
            // Step: 11
            //Do Bloom
            _fxStack.Draw(PipelineFxStage.Bloom, null, null, _auxTargets[PipelineTargets.COMPOSE]);
            _currentOutput = _fxStack.Draw(PipelineFxStage.PostProcessing, _auxTargets[PipelineTargets.COMPOSE], null, _auxTargets[PipelineTargets.OUTPUT]);
            _currentOutput = _fxStack.Draw(PipelineFxStage.ColorGrading, _auxTargets[PipelineTargets.OUTPUT], null, null);

            _profiler.Sample(TimestampIndices.Draw_Total);

            // Step: 13
            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            BlitToScreen(RenderingSettings.g_CurrentPass, null);

            //Performance Profiler
            _profiler.SampleTimestamp(TimestampIndices.Draw_FinalRender);


        }
        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public void DrawEditor(DynamicMeshBatcher meshBatcher, EntityScene scene, GizmoDrawContext gizmoContext)
        {
            this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.SDFDistance);
            this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.SDFVolume);

            // Step: 15
            //Additional editor elements that overlay our screen
            if (RenderingSettings.e_EnableSelection)
            {
                if (IdAndOutlineRenderModule.e_DrawOutlines)
                    this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.IdAndOutline);
                this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.Billboard | PipelineEditorPasses.TransformGizmo);
                //Draw debug/helper geometry
                this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.Helper);
            }
        }


        private bool IsSDFUsed(List<PointLight> pointLights)
        {
            if (!RenderingSettings.SDF.DrawDistance)
                return false;

            foreach (PointLight light in pointLights)
                if (light.HasChanged && light.CastSDFShadows)
                    return true;
            return false;
        }

        private void DrawEditorPasses(EntityScene scene, GizmoDrawContext gizmoContext, PipelineEditorPasses passes = PipelineEditorPasses.Billboard | PipelineEditorPasses.TransformGizmo)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            // render directly to the output buffer
            if (passes.HasFlag(PipelineEditorPasses.Billboard))
            {
                _moduleStack.Billboard.DrawEditorBillboards(scene, gizmoContext);
            }
            if (passes.HasFlag(PipelineEditorPasses.IdAndOutline))
            {
                _moduleStack.IdAndOutline.Blit(_moduleStack.IdAndOutline.Target, null, BlendState.Additive);
            }
            if (passes.HasFlag(PipelineEditorPasses.TransformGizmo))
            {
                _moduleStack.IdAndOutline.DrawTransformGizmos(gizmoContext, IdAndOutlineRenderModule.Pass.Color);
            }
            if (passes.HasFlag(PipelineEditorPasses.Helper))
            {
                _moduleStack.Helper.Draw();
            }
            if (passes.HasFlag(PipelineEditorPasses.SDFDistance))
            {
                _graphicsDevice.SetStates(DepthStencilStateOption.None);
                _moduleStack.DistanceField.DrawDistance();
            }
            if (passes.HasFlag(PipelineEditorPasses.SDFVolume))
            {
                _graphicsDevice.SetStates(DepthStencilStateOption.None);
                _moduleStack.DistanceField.DrawVolume();
            }
            //if(passes.HasFlag(PipelineEditorPasses.SDFVolume))
            //{
            //    if (RenderingSettings.SDF.DrawDebug && _moduleStack.DistanceField.AtlasTarget != null)
            //    {
            //        _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            //        _spriteBatch.Draw(_moduleStack.DistanceField.AtlasTarget, new Rectangle(0, RenderingSettings.Screen.g_Height - 200, RenderingSettings.Screen.g_Width, 200), Color.White);
            //        _spriteBatch.End();
            //    }
            //}
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
            //If the camera didn't do anything we don't need to update this stuff
            if (camera.HasChanged)
            {
                //View matrix
                _matrices.SetFromCamera(camera);

                //Temporal AA - alternate frames for temporal anti-aliasing
                if (_fxStack.TemporalAA.Enabled)
                {
                    _fxStack.TemporalAA.SwapOffFrame();
                    _matrices.ApplyViewProjectionJitter(_fxStack.TemporalAA.JitterMode, _fxStack.TemporalAA.IsOffFrame, _fxStack.TemporalAA.HaltonSequence);
                }
        ;
                _frustum.Frustum.Matrix = _matrices.StaticViewProjection;
            }

        }

        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        private void BlitToScreen(PipelineOutputPasses pass, RenderTarget2D destRT)
        {
            switch (pass)
            {
                case PipelineOutputPasses.Albedo:
                    BlitTo(_gBufferTarget.Albedo, destRT);
                    break;
                case PipelineOutputPasses.Normal:
                    BlitTo(_gBufferTarget.Normal, destRT);
                    break;
                case PipelineOutputPasses.Depth:
                    BlitTo(_gBufferTarget.Depth, destRT);
                    break;
                case PipelineOutputPasses.Diffuse:
                    BlitTo(_lightingBufferTarget.Diffuse, destRT);
                    break;
                case PipelineOutputPasses.Specular:
                    BlitTo(_lightingBufferTarget.Specular, destRT);
                    break;
                case PipelineOutputPasses.Volumetric:
                    BlitTo(_lightingBufferTarget.Volume, destRT);
                    break;
                case PipelineOutputPasses.SSAO:
                    BlitTo(_ssfxTargets.AO_Main, destRT);
                    break;
                case PipelineOutputPasses.SSBlur:
                    BlitTo(_ssfxTargets.AO_Blur_Final, destRT);
                    break;
                case PipelineOutputPasses.SSR:
                    BlitTo(_ssfxTargets.SSR_Main, destRT);
                    break;
                default:
                    break;
            }

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

