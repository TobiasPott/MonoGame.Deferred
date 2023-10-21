using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using DeferredEngine.Renderer.PostProcessing;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System;
using System.Collections.Generic;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
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
        private FullscreenTriangleBuffer FullscreenTarget { get => FullscreenTriangleBuffer.Instance; }

        //Projection Matrices and derivates used in shaders
        private PipelineMatrices _matrices;
        private PipelineModuleStack _moduleStack;
        private PipelineProfiler _profiler;

        //Used for the view space directions in our shaders. Far edges of our view frustum
        private FrustumCornerVertices _frustumCorners = new FrustumCornerVertices();


        private TemporalAAFx _taaFx;
        private BloomFx _bloomFx;
        private ColorGradingFx _colorGradingFx;

        //View Projection
        private bool _viewProjectionHasChanged;

        //Bounding Frusta of our view projection, to calculate which objects are inside the view
        private BoundingFrustum _boundingFrustum;

        //Checkvariables to see which console variables have changed from the frame before
        private float _g_FarClip;
        private float _supersampling = 1;
        private bool _ssr = true;
        private bool _g_SSReflectionNoise;

        //Render targets
        private GBufferTarget _gBufferTarget;
        private LightingBufferTarget _lightingBufferTarget;
        private MRT.PipelineTargets _auxTargets;

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

            _moduleStack = new PipelineModuleStack(content);
            _profiler = new PipelineProfiler();

            _bloomFx = new BloomFx(content);
            _taaFx = new TemporalAAFx() { Matrices = _matrices };
            _colorGradingFx = new ColorGradingFx(content);

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
            _auxTargets = new MRT.PipelineTargets(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);

            _moduleStack.Initialize(graphicsDevice, _spriteBatch);
            _moduleStack.GBuffer.GBufferTarget = _gBufferTarget;


            _bloomFx.Initialize(graphicsDevice, RenderingSettings.g_ScreenResolution);
            _taaFx.Initialize(graphicsDevice, FullscreenTriangleBuffer.Instance);
            _colorGradingFx.Initialize(graphicsDevice, FullscreenTriangleBuffer.Instance);


            _boundingFrustum = new BoundingFrustum(_matrices.ViewProjection);

            //Apply some base settings to overwrite shader defaults with game settings defaults
            RenderingSettings.ApplySettings();

            Shaders.SSR.Param_NoiseMap.SetValue(StaticAssets.Instance.NoiseMap);
            SetUpRenderTargets(RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight, false);

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

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges(scene);

            //Render ShadowMaps
            DrawShadowMaps(meshBatcher, scene, camera);

            //Update SDFs
            if (IsSDFUsed(scene.PointLights))
            {
                _moduleStack.DistanceField.UpdateDistanceFieldTransformations(scene.Entities);
            }

            //Update our view projection matrices if the camera moved
            UpdateViewProjection(meshBatcher, camera);

            //Draw our meshes to the G Buffer
            DrawGBuffer(meshBatcher);

            //Deferred Decals
            DrawDecals(scene.Decals);

            //Draw Screen Space reflections to a different render target
            DrawScreenSpaceReflections(gameTime);

            //SSAO
            DrawScreenSpaceAmbientOcclusion(camera);

            //Upsample/blur our SSAO / screen space shadows
            DrawBilateralBlur();

            //Light the scene
            _moduleStack.Lighting.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _matrices);
            _moduleStack.Lighting.DrawLights(scene, camera.Position, _lightingBufferTarget.Bindings, _lightingBufferTarget.Diffuse);

            //Draw the environment cube map as a fullscreen effect on all meshes
            DrawEnvironmentMap(scene.EnvProbe, camera, gameTime);

            //Compose the scene by combining our lighting data with the gbuffer data
            _currentOutput = Compose(_auxTargets[MRT.COMPOSE]);

            //Forward
            _currentOutput = DrawForward(_currentOutput, meshBatcher, camera, scene.PointLights);

            //Compose the image and add information from previous frames to apply temporal super sampling
            _currentOutput = TonemapAndCombineTemporalAntialiasing(_currentOutput);

            //Do Bloom
            _currentOutput = DrawBloom(_currentOutput);

            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.e_IsEditorEnabled && RenderingStats.e_EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, _matrices, gizmoContext, EditorLogic.Instance.HasMouseMoved);

            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode(_currentOutput);

            //Draw signed distance field functions
            DrawSDFs(camera);

            //Additional editor elements that overlay our screen

            RenderEditorOverlays(gizmoContext, scene);

            //Draw debug geometry
            _moduleStack.Helper.ViewProjection = _matrices.StaticViewProjection;
            _moduleStack.Helper.Draw();

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
                    DrawTextureToScreenToFullScreen(_moduleStack.IdAndOutline.GetRenderTarget2D(), BlendState.Additive);

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
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

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
        private void CheckRenderChanges(EntitySceneGroup scene)
        {
            List<Pipeline.Lighting.DirectionalLight> dirLights = scene.DirectionalLights;
            if (Math.Abs(_g_FarClip - RenderingSettings.g_FarPlane) > 0.0001f)
            {
                _g_FarClip = RenderingSettings.g_FarPlane;
                _moduleStack.FarClip = _g_FarClip;

                Shaders.SSR.Param_FarClip.SetValue(_g_FarClip);
                Shaders.ReconstructDepth.Param_FarClip.SetValue(_g_FarClip);
            }

            if (_g_SSReflectionNoise != RenderingSettings.g_SSReflectionNoise)
            {
                _g_SSReflectionNoise = RenderingSettings.g_SSReflectionNoise;
                if (!_g_SSReflectionNoise) Shaders.SSR.Param_Time.SetValue(0.0f);
            }

            if (_ssr != RenderingSettings.g_SSReflection)
            {
                _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_REFLECTION]);
                _graphicsDevice.Clear(new Color(0, 0, 0, 0.0f));

                _ssr = RenderingSettings.g_SSReflection;
            }

            //Performance Profiler
            _profiler.Timestamp();

        }

        /// <summary>
        /// Draw our shadow maps from the individual lights. Check if something has changed first, otherwise leave as it is
        /// </summary>
        private void DrawShadowMaps(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, Camera camera)
        {
            //Don't render for the first frame, we need a guideline first
            if (_boundingFrustum == null)
                UpdateViewProjection(meshBatcher, camera);

            _moduleStack.ShadowMap.Draw(meshBatcher, scene);

            //Performance Profile
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Shadows);
        }

        /// <summary>
        /// Create the projection matrices
        /// </summary>
        private void UpdateViewProjection(DynamicMeshBatcher meshBatcher, Camera camera)
        {
            _viewProjectionHasChanged = camera.HasChanged;

            //If the camera didn't do anything we don't need to update this stuff
            if (_viewProjectionHasChanged)
            {
                //View matrix
                _matrices.SetFromCamera(camera);

                _moduleStack.PointLight.InverseView = _matrices.InverseView;

                //Temporal AA - alternate frames for temporal anti-aliasing
                if (_taaFx?.Enabled ?? false)
                {
                    _viewProjectionHasChanged = true;
                    _taaFx.SwapOffFrame();
                    _taaFx.UpdateViewProjection(_matrices);
                }


                _matrices.PreviousViewProjection = _matrices.ViewProjection;
                _matrices.InverseViewProjection = Matrix.Invert(_matrices.ViewProjection);

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_matrices.StaticViewProjection);
                else _boundingFrustum.Matrix = _matrices.StaticViewProjection;

                // Compute the frustum corners for cheap view direction computation in shaders
                ComputeFrustumCorners(_boundingFrustum, camera);
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshBatcher.FrustumCulling(_boundingFrustum, _viewProjectionHasChanged, camera.Position);

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SUpdate_ViewProjection);
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
            Vector3.Transform(_frustumCorners.WorldSpace, ref _matrices.View, _frustumCorners.ViewSpace); //put the frustum into view space

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

            Shaders.SSR.Param_FrustumCorners.SetValue(_frustumCorners.ViewSpaceFrustum);
            Shaders.SSAO.Param_FrustumCorners.SetValue(_frustumCorners.ViewSpaceFrustum);
            Shaders.ReconstructDepth.Param_FrustumCorners.SetValue(_frustumCorners.ViewSpaceFrustum);
            _moduleStack.DirectionalLight.SetFrustumCorners(_frustumCorners.ViewSpaceFrustum);
            _taaFx.FrustumCorners = _frustumCorners.ViewSpaceFrustum;
        }

        /// <summary>
        /// Draw all our meshes to the GBuffer - albedo, normal, depth - for further computation
        /// </summary>
        private void DrawGBuffer(DynamicMeshBatcher meshBatcher)
        {
            _moduleStack.GBuffer.Draw(meshBatcher, _matrices);

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_GBuffer);
        }

        /// <summary>
        /// Draw deferred Decals
        /// </summary>
        private void DrawDecals(List<Decal> decals)
        {
            if (!DecalRenderModule.g_EnableDecals) return;

            //First copy albedo to decal offtarget
            DrawTextureToScreenToFullScreen(_gBufferTarget.Albedo, BlendState.Opaque, _auxTargets[MRT.DECAL]);

            DrawTextureToScreenToFullScreen(_auxTargets[MRT.DECAL], BlendState.Opaque, _gBufferTarget.Albedo);

            _moduleStack.Decal.Draw(decals, _matrices);
        }

        /// <summary>
        /// Draw Screen Space Reflections
        /// </summary>
        private void DrawScreenSpaceReflections(GameTime gameTime)
        {
            if (!RenderingSettings.g_SSReflection) return;

            //todo: more samples for more reflective materials!
            _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_REFLECTION]);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (_taaFx.Enabled)
            {
                Shaders.SSR.Param_TargetMap.SetValue(_taaFx.IsOffFrame ? _auxTargets[MRT.SSFX_TAA_1] : _auxTargets[MRT.SSFX_TAA_2]);
            }
            else
            {
                Shaders.SSR.Param_TargetMap.SetValue(_auxTargets[MRT.COMPOSE]);
            }

            if (RenderingSettings.g_SSReflectionNoise)
                Shaders.SSR.Param_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            Shaders.SSR.Param_Projection.SetValue(_matrices.Projection);

            Shaders.SSR.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);

            // Profiler sample
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx_SSR);

        }

        /// <summary>
        /// Draw SSAO to a different rendertarget
        /// </summary>
        /// <param name="camera"></param>
        private void DrawScreenSpaceAmbientOcclusion(Camera camera)
        {
            if (!RenderingSettings.g_ssao_draw) return;

            _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_AMBIENTOCCLUSION]);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.SSAO.SetCameraAndMatrices(camera.Position, _matrices);

            Shaders.SSAO.Effect.CurrentTechnique = Shaders.SSAO.Technique_SSAO;
            Shaders.SSAO.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);


            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_SSFx);
        }

        /// <summary>
        /// Bilateral blur, to upsample our undersampled SSAO
        /// </summary>
        private void DrawBilateralBlur()
        {
            _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_BLUR_VERTICAL]);

            _spriteBatch.Begin(0, BlendState.Additive);

            _spriteBatch.Draw(_auxTargets[MRT.SSFX_AMBIENTOCCLUSION], RenderingSettings.g_ScreenRect, Color.Red);

            _spriteBatch.End();

            if (RenderingSettings.g_ssao_blur && RenderingSettings.g_ssao_draw)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_BLUR_HORIZONTAL]);

                Shaders.SSAO.Param_InverseResolution.SetValue(new Vector2(1.0f / _auxTargets[MRT.SSFX_BLUR_VERTICAL].Width, 1.0f / _auxTargets[MRT.SSFX_BLUR_VERTICAL].Height) * 2);
                Shaders.SSAO.Param_SSAOMap.SetValue(_auxTargets[MRT.SSFX_BLUR_VERTICAL]);
                Shaders.SSAO.Technique_BlurVertical.Passes[0].Apply();

                FullscreenTarget.Draw(_graphicsDevice);

                _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_BLUR_FINAL]);

                Shaders.SSAO.Param_InverseResolution.SetValue(new Vector2(1.0f / _auxTargets[MRT.SSFX_BLUR_HORIZONTAL].Width, 1.0f / _auxTargets[MRT.SSFX_BLUR_HORIZONTAL].Height) * 0.5f);
                Shaders.SSAO.Param_SSAOMap.SetValue(_auxTargets[MRT.SSFX_BLUR_HORIZONTAL]);
                Shaders.SSAO.Technique_BlurHorizontal.Passes[0].Apply();

                FullscreenTarget.Draw(_graphicsDevice);

            }
            else
            {
                _graphicsDevice.SetRenderTarget(_auxTargets[MRT.SSFX_BLUR_FINAL]);

                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp);

                _spriteBatch.Draw(_auxTargets[MRT.SSFX_BLUR_VERTICAL], new Rectangle(0, 0, _auxTargets[MRT.SSFX_BLUR_FINAL].Width, _auxTargets[MRT.SSFX_BLUR_FINAL].Height), Color.White);

                _spriteBatch.End();
            }

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_BilateralBlur);
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
        private RenderTarget2D Compose(RenderTarget2D destination)
        {
            // ToDo: @tpott: hacky way to disable ssao when disabled on global scale (GUI is insufficient here)
            _moduleStack.Deferred.UseSSAOMap = RenderingSettings.g_ssao_draw;
            _moduleStack.Deferred.Draw(destination);

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_Compose);

            return destination;
        }

        private void ReconstructDepth()
        {
            if (_viewProjectionHasChanged)
                Shaders.ReconstructDepth.Param_Projection.SetValue(_matrices.Projection);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.ReconstructDepth.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);
        }

        private RenderTarget2D DrawForward(RenderTarget2D input, DynamicMeshBatcher meshBatcher, Camera camera, List<PointLight> pointLights)
        {
            if (!ForwardPipelineModule.g_EnableForward)
                return input;

            _graphicsDevice.SetRenderTarget(input);
            ReconstructDepth();

            _moduleStack.Forward.PrepareDraw(camera, pointLights, _boundingFrustum);
            return _moduleStack.Forward.Draw(meshBatcher, input, _matrices);
        }

        private RenderTarget2D DrawBloom(RenderTarget2D input)
        {
            if (_bloomFx.Enabled)
            {
                Texture2D bloom = _bloomFx.Draw(input);

                _graphicsDevice.SetRenderTargets(_auxTargets[MRT.BLOOM]);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                _spriteBatch.Draw(input, RenderingSettings.g_ScreenRect, Color.White);
                _spriteBatch.Draw(bloom, RenderingSettings.g_ScreenRect, Color.White);

                _spriteBatch.End();

                return _auxTargets[MRT.BLOOM];
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D TonemapAndCombineTemporalAntialiasing(RenderTarget2D input)
        {
            if (!_taaFx.Enabled) return input;

            RenderTarget2D output = !_taaFx.IsOffFrame ? _auxTargets[MRT.SSFX_TAA_1] : _auxTargets[MRT.SSFX_TAA_2];
            _taaFx.Draw(input,
                    _taaFx.IsOffFrame ? _auxTargets[MRT.SSFX_TAA_1] : _auxTargets[MRT.SSFX_TAA_2],
                output);

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_CombineTAA);

            return RenderingSettings.TAA.UseTonemapping ? input : output;
        }

        private void DrawSDFs(Camera camera)
        {
            if (!RenderingSettings.SDF.DrawDistance)
                return;
            _moduleStack.DistanceField.Draw(camera);
        }

        /// <summary>
        /// Add some post processing to the image
        /// </summary>
        /// <param name="currentInput"></param>
        private void DrawPostProcessing(RenderTarget2D currentInput)
        {
            if (!RenderingSettings.g_PostProcessing) return;

            RenderTarget2D destinationRenderTarget = _auxTargets[MRT.OUTPUT];

            Shaders.PostProcssing.Param_ScreenTexture.SetValue(currentInput);
            _graphicsDevice.SetRenderTarget(destinationRenderTarget);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.PostProcssing.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);

            if (_colorGradingFx.Enabled)
                destinationRenderTarget = _colorGradingFx.Draw(destinationRenderTarget);

            DrawTextureToScreenToFullScreen(destinationRenderTarget);
        }

        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        /// <param name="currentOutput"></param>
        /// <param name="editorData"></param>
        private void RenderMode(RenderTarget2D currentInput)
        {
            switch (RenderingSettings.g_RenderMode)
            {
                case RenderModes.Albedo:
                    DrawTextureToScreenToFullScreen(_gBufferTarget.Albedo);
                    break;
                case RenderModes.Normal:
                    DrawTextureToScreenToFullScreen(_gBufferTarget.Normal);
                    break;
                case RenderModes.Depth:
                    DrawTextureToScreenToFullScreen(_gBufferTarget.Depth);
                    break;
                case RenderModes.Diffuse:
                    DrawTextureToScreenToFullScreen(_lightingBufferTarget.Diffuse);
                    break;
                case RenderModes.Specular:
                    DrawTextureToScreenToFullScreen(_lightingBufferTarget.Specular);
                    break;
                case RenderModes.Volumetric:
                    DrawTextureToScreenToFullScreen(_lightingBufferTarget.Volume);
                    break;
                case RenderModes.SSAO:
                    DrawTextureToScreenToFullScreen(_auxTargets[MRT.SSFX_AMBIENTOCCLUSION]);
                    break;
                case RenderModes.SSBlur:
                    DrawTextureToScreenToFullScreen(_auxTargets[MRT.SSFX_BLUR_FINAL]);
                    break;
                case RenderModes.SSR:
                    DrawTextureToScreenToFullScreen(_auxTargets[MRT.SSFX_REFLECTION]);
                    break;
                case RenderModes.HDR:
                    DrawTextureToScreenToFullScreen(currentInput);
                    break;
                default:
                    DrawPostProcessing(currentInput);
                    break;
            }

            //Performance Profiler
            _profiler.SampleTimestamp(ref PipelineSamples.SDraw_FinalRender);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void SetUpRenderTargets(int width, int height, bool onlyEssentials)
        {
            float ssmultiplier = _supersampling;

            int targetWidth = (int)(width * ssmultiplier);
            int targetHeight = (int)(height * ssmultiplier);

            //Shaders.Billboard.Param_AspectRatio.SetValue((float)targetWidth / targetHeight);

            // Update multi render target size
            _gBufferTarget.Resize(targetWidth, targetHeight);
            _lightingBufferTarget.Resize(targetWidth, targetHeight);
            _auxTargets.Resize(targetWidth, targetHeight);

            _moduleStack.PointLight.Resolution = new Vector2(targetWidth, targetHeight);

            if (!onlyEssentials)
            {
                _moduleStack.Billboard.AspectRatio = (float)targetWidth / targetHeight;
                _moduleStack.IdAndOutline.SetUpRenderTarget(width, height);

                _taaFx.Resolution = new Vector2(targetWidth, targetHeight);

                Shaders.SSR.Param_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                _moduleStack.Environment.Resolution = new Vector2(targetWidth, targetHeight);

                ///////////////////
                // HALF RESOLUTION

                targetWidth /= 2;
                targetHeight /= 2;

                Shaders.SSAO.Param_InverseResolution.SetValue(new Vector2(1.0f / targetWidth, 1.0f / targetHeight));


                Vector2 aspectRatio = new Vector2(Math.Min(1.0f, targetWidth / (float)targetHeight), Math.Min(1.0f, targetHeight / (float)targetWidth));

                Shaders.SSAO.Param_AspectRatio.SetValue(aspectRatio);

            }

            UpdateRenderMapBindings(onlyEssentials);
        }

        private void UpdateRenderMapBindings(bool onlyEssentials)
        {
            _moduleStack.Billboard.DepthMap = _gBufferTarget.Depth;

            Shaders.ReconstructDepth.Param_DepthMap.SetValue(_gBufferTarget.Depth);

            _moduleStack.SetGBufferParams(_gBufferTarget);
            // update directional light module
            _moduleStack.DirectionalLight.SetScreenSpaceShadowMap(onlyEssentials ? _auxTargets[MRT.SSFX_BLUR_VERTICAL] : _auxTargets[MRT.SSFX_BLUR_FINAL]);

            _moduleStack.Environment.SSRMap = _auxTargets[MRT.SSFX_REFLECTION];

            _moduleStack.Deferred.SetLightingParams(_lightingBufferTarget);
            _moduleStack.Deferred.SetSSAOMap(_auxTargets[MRT.SSFX_BLUR_FINAL]);

            Shaders.SSAO.Param_NormalMap.SetValue(_gBufferTarget.Normal);
            Shaders.SSAO.Param_DepthMap.SetValue(_gBufferTarget.Depth);
            Shaders.SSAO.Param_SSAOMap.SetValue(_auxTargets[MRT.SSFX_AMBIENTOCCLUSION]);

            Shaders.SSR.Param_NormalMap.SetValue(_gBufferTarget.Normal);
            Shaders.SSR.Param_DepthMap.SetValue(_gBufferTarget.Depth);

            _taaFx.DepthMap = _gBufferTarget.Depth;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void DrawTextureToScreenToCube(RenderTarget2D texture, RenderTargetCube target, CubeMapFace? face)
        {

            if (face != null) _graphicsDevice.SetRenderTarget(target, (CubeMapFace)face);
            // _graphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), Color.White);
            _spriteBatch.End();
        }
        private void DrawTextureToScreenToFullScreen(Texture2D texture, BlendState blendState = null, RenderTarget2D output = null)
        {
            if (blendState == null) blendState = BlendState.Opaque;

            RenderingSettings.GetDestinationRectangle(texture.GetAspect(), out Rectangle destRectangle);
            _graphicsDevice.SetRenderTarget(output);
            _spriteBatch.Begin(0, blendState, _supersampling > 1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
            _spriteBatch.Draw(texture, destRectangle, Color.White);
            _spriteBatch.End();
        }



        public void Dispose()
        {
            _graphicsDevice?.Dispose();
            _spriteBatch?.Dispose();

            _bloomFx?.Dispose();
            _taaFx?.Dispose();
            _colorGradingFx?.Dispose();

            _moduleStack?.Dispose();

            _gBufferTarget?.Dispose();
            _lightingBufferTarget?.Dispose();
            _auxTargets?.Dispose();

            _currentOutput?.Dispose();
            _renderTargetCubeMap?.Dispose();
        }
    }

}

