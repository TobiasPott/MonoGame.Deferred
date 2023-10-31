using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Pipeline
{
    public enum DeferredRenderingPasses
    {
        Deferred = 0,
        Albedo,
        Normal,
        Depth,
        Diffuse,
        Specular,
        Volumetric,
        SSAO,
        SSBlur,
        SSR,
        // [Obsolete("HDR buffer is no longer directly available.")]
        // HDR,
        Final,
    }

    public abstract class DeferredRenderingPipeline : RenderingPipelineBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected bool _redrawRequested = false;

        //Projection Matrices and derivates used in shaders
        protected PipelineMatrices _matrices;
        protected PipelineModuleStack _moduleStack;
        protected PipelineFxStack _fxStack;
        protected PipelineFrustum _frustum = new PipelineFrustum();
        protected PipelineProfiler _profiler;

        //Render targets
        protected GBufferTarget _gBufferTarget;
        protected LightingBufferTarget _lightingBufferTarget;
        protected PipelineTargets _auxTargets;
        protected SSFxTargets _ssfxTargets;



        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  BASE FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual void RequestRedraw(GameTime gameTime)
        {
            _redrawRequested = true;

            _moduleStack.PointLight.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            _fxStack.SSReflection.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            _moduleStack.Environment.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
        }

        /// <summary>
        /// Initialize variables
        /// </summary>
        public override void Load(ContentManager content)
        {
            _matrices = new PipelineMatrices();
            _profiler = new PipelineProfiler();

            _moduleStack = new PipelineModuleStack
            {
                Matrices = _matrices,
                Frustum = _frustum,
                Profiler = _profiler
            };

            _fxStack = new PipelineFxStack(content)
            {
                Matrices = _matrices,
                Frustum = _frustum,
                Profiler = _profiler
            };

        }
        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            base.Initialize(graphicsDevice);

            Vector2 resolution = RenderingSettings.Screen.g_Resolution;
            _gBufferTarget = new GBufferTarget(graphicsDevice, resolution);
            _lightingBufferTarget = new LightingBufferTarget(graphicsDevice, resolution);
            _auxTargets = new PipelineTargets(graphicsDevice, resolution);
            _ssfxTargets = new SSFxTargets(graphicsDevice, resolution);

            _moduleStack.Initialize(graphicsDevice, _spriteBatch);
            _moduleStack.Resolution = resolution;
            _moduleStack.GBufferTarget = _gBufferTarget;
            _moduleStack.LightingBufferTarget = _lightingBufferTarget;
            _moduleStack.SSFxTargets = _ssfxTargets;

            _fxStack.Initialize(graphicsDevice, _spriteBatch);
            _fxStack.Resolution = resolution;
            _fxStack.GBufferTarget = _gBufferTarget;
            _fxStack.SSFxTargets = _ssfxTargets;
        }

        public virtual void SetResolution(Vector2 resolution)
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
        /// Update our function
        /// </summary>
        public override void Update(DynamicMeshBatcher meshBatcher, EntityScene scene, Camera camera)
        {
            if (!this.Enabled)
                return;

            // Step: 04
            //Update our view projection matrices if the camera moved
            if (_redrawRequested)
            {
                //View matrix
                camera.FarClip = RenderingSettings.Screen.g_FarClip;
                _matrices.SetFromCamera(camera);

                //Temporal AA - alternate frames for temporal anti-aliasing
                if (_fxStack.TemporalAA.Enabled)
                {
                    _fxStack.TemporalAA.SwapOffFrame();
                    _matrices.ApplyViewProjectionJitter(_fxStack.TemporalAA.JitterMode, _fxStack.TemporalAA.IsOffFrame, _fxStack.TemporalAA.HaltonSequence);
                }
                _fxStack.SSAmbientOcclusion.SetViewPosition(camera.Position);

                _frustum.Frustum.Matrix = _matrices.StaticViewProjection;
                // Compute the frustum corners for cheap view direction computation in shaders
                _frustum.UpdateVertices(_matrices.View, camera.Position);


                _moduleStack.Lighting.SetViewPosition(camera.Position);
                _moduleStack.Environment.SetViewPosition(camera.Position);
                _moduleStack.Lighting.RequestRedraw();
                _moduleStack.Environment.SetEnvironmentProbe(scene.EnvProbe);
                _moduleStack.Deferred.UseSSAOMap = _fxStack.SSAmbientOcclusion?.Enabled ?? false;
                _moduleStack.Forward.SetupLighting(camera.Position, scene.PointLights, _frustum.Frustum);
                _moduleStack.DistanceField.SetViewPosition(camera.Position);

                //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
                meshBatcher.FrustumCulling(_frustum.Frustum, _redrawRequested);

                //Performance Profiler
                _profiler.SampleTimestamp(ProfilerTimestamps.Update_ViewProjection);

                // Step: 03
                //Update SDFs
                if (IsSDFUsed(scene.PointLights))
                {
                    _moduleStack.DistanceField.UpdateSdfGenerator(scene.Entities);
                    _moduleStack.DistanceField.UpdateDistanceFieldTransformations(scene.Entities);
                }

                //Performance Profiler
                _profiler.SampleTimestamp(ProfilerTimestamps.Update_SDF);
            }

            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();

            _redrawRequested = false;
        }


        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public override void Draw(DynamicMeshBatcher meshBatcher, EntityScene scene)
        {
            if (!this.Enabled)
                return;

            // Step: 01
            // Deferred GBuffer
            _moduleStack.GBuffer.Draw(meshBatcher);
            // Deferred Decals
            _moduleStack.Decal.Draw(scene, null, _auxTargets[PipelineTargets.SWAP], _gBufferTarget.Albedo);

            // Step: 02
            //Render SHADOW MAPS
            _moduleStack.ShadowMap.Draw(meshBatcher, scene);
            // Step: 03
            //Light the scene
            _moduleStack.Lighting.Draw(scene);

            // ToDo: PRIO II: Is Environment module actually part of lighting? (unsure ahout the sky part though)
            //              I mmight need to split it into Environment and Sky
            // Step: 04
            //Draw the environment cube map as a fullscreen effect on all meshes
            //_moduleStack.Environment.DrawSky();
            _moduleStack.Environment.Draw();
            // Step: 05
            // Compose the scene by combining our lighting data with the gbuffer data
            _moduleStack.Deferred.Draw(null, null, _auxTargets[PipelineTargets.SWAP_HALF]);
            // Step: 06
            // Forward
            _moduleStack.Forward.Draw(meshBatcher, null, null, _auxTargets[PipelineTargets.SWAP_HALF]);
            // Step: 07
            // Post processing passes
            // SSAO
            _fxStack.Draw(PipelineFxStage.SSAmbientOcclusion, null, null, _ssfxTargets.AO_Main);
            // TAA
            _fxStack.Draw(PipelineFxStage.TemporalAA, null, null, _auxTargets[PipelineTargets.SWAP_HALF]);
            // SSR
            _fxStack.Draw(PipelineFxStage.SSReflection, null, null, _ssfxTargets.SSR_Main);
            // BLOOM
            _fxStack.Draw(PipelineFxStage.Bloom, null, null, _auxTargets[PipelineTargets.SWAP_HALF]);
            // POST PROCESSING
            _fxStack.Draw(PipelineFxStage.PostProcessing, _auxTargets[PipelineTargets.SWAP_HALF], null, _auxTargets[PipelineTargets.SWAP]);
            // COLOR GRADING
            _fxStack.Draw(PipelineFxStage.ColorGrading, _auxTargets[PipelineTargets.SWAP], null, _auxTargets[PipelineTargets.FINALCOLOR]);

            // Step: 08 Blit final color to screen (may blit to a 'viewport' section of the screen, or the full screen
            this.BlitTo(_auxTargets[PipelineTargets.FINALCOLOR], null, RenderingSettings.Screen.g_TargetRect);

            _profiler.Sample(ProfilerTimestamps.Draw_Total);

            // Step: 13
            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            BlitToScreen(RenderingSettings.g_CurrentPass, null);

            //Performance Profiler
            _profiler.SampleTimestamp(ProfilerTimestamps.Draw_FinalRender);
        }

        /// <summary>
        /// Draw the given pass to the destination target
        /// </summary>
        protected void BlitToScreen(DeferredRenderingPasses pass, RenderTarget2D destRT)
        {
            switch (pass)
            {
                case DeferredRenderingPasses.Albedo:
                    BlitTo(_gBufferTarget.Albedo, destRT);
                    break;
                case DeferredRenderingPasses.Normal:
                    BlitTo(_gBufferTarget.Normal, destRT);
                    break;
                case DeferredRenderingPasses.Depth:
                    BlitTo(_gBufferTarget.Depth, destRT);
                    break;
                case DeferredRenderingPasses.Diffuse:
                    BlitTo(_lightingBufferTarget.Diffuse, destRT);
                    break;
                case DeferredRenderingPasses.Specular:
                    BlitTo(_lightingBufferTarget.Specular, destRT);
                    break;
                case DeferredRenderingPasses.Volumetric:
                    BlitTo(_lightingBufferTarget.Volume, destRT);
                    break;
                case DeferredRenderingPasses.SSAO:
                    BlitTo(_ssfxTargets.AO_Main, destRT);
                    break;
                case DeferredRenderingPasses.SSBlur:
                    BlitTo(_ssfxTargets.AO_Blur_Final, destRT);
                    break;
                case DeferredRenderingPasses.SSR:
                    BlitTo(_ssfxTargets.SSR_Main, destRT);
                    break;
                default:
                    break;
            }

        }
        protected virtual bool IsSDFUsed(List<PointLight> pointLights)
        {
            if (!RenderingSettings.SDF.DrawDistance)
                return false;

            foreach (PointLight light in pointLights)
                if (light.HasChanged && light.CastSDFShadows)
                    return true;
            return false;
        }
        /// <summary>
        /// Reset our stat counting for this frame
        /// </summary>
        protected void ResetStats()
        {
            RenderingStats.ResetStats();
            _profiler.Reset();
        }

        public override void Dispose()
        {
            base.Dispose();

            _moduleStack?.Dispose();
            _fxStack?.Dispose();

            _gBufferTarget?.Dispose();
            _lightingBufferTarget?.Dispose();
            _auxTargets?.Dispose();
            _ssfxTargets?.Dispose();

        }
    }

}

