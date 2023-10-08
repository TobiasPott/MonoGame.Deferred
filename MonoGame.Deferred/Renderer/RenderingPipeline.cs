using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using DeferredEngine.Renderer.PostProcessing;
using DeferredEngine.Renderer.RenderModules;
using DeferredEngine.Renderer.RenderModules.DeferredLighting;
using DeferredEngine.Renderer.RenderModules.SDF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System;
using System.Collections.Generic;
using System.Diagnostics;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public partial class RenderingPipeline : IDisposable
    {
        #region VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Graphics & Helpers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private FullscreenTriangleBuffer FullscreenTarget { get => FullscreenTriangleBuffer.Instance; }

        private EditorRender _editorRender;

        private PointLightRenderModule _pointLightRenderModule;
        private LightAccumulationModule _lightAccumulationModule;
        private ShadowMapRenderModule _shadowMapRenderModule;
        private GBufferRenderModule _gBufferRenderModule;
        private EnvironmentProbeRenderModule _environmentProbeRenderModule;
        private DecalRenderModule _decalRenderModule;
        private ForwardRenderModule _forwardRenderModule;
        private HelperGeometryRenderModule _helperGeometryRenderModule;
        private DistanceFieldRenderModule _distanceFieldRenderModule;


        private TemporalAntialiasingFx _taaFx;
        private BloomFx _bloomFx;
        private ColorGradingFx _colorGradingFx;

        //Assets
        private Assets _assets;

        //View Projection
        private bool _viewProjectionHasChanged;
        private Vector2 _inverseResolution;

        //Temporal Anti Aliasing
        private bool _temporalAAOffFrame = true;
        private Vector3[] _haltonSequence;
        private int _haltonSequenceIndex = -1;
        private const int HaltonSequenceLength = 16;

        //Projection Matrices and derivates used in shaders
        private Matrix _view;
        private Matrix _inverseView;
        private Matrix _viewIT;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _staticViewProjection;
        private Matrix _inverseViewProjection;
        private Matrix _previousViewProjection;
        private Matrix _currentViewToPreviousViewProjection;

        //Bounding Frusta of our view projection, to calculate which objects are inside the view
        private BoundingFrustum _boundingFrustum;

        //Used for the view space directions in our shaders. Far edges of our view frustum
        private readonly Vector3[] _cornersWorldSpace = new Vector3[8];
        private readonly Vector3[] _cornersViewSpace = new Vector3[8];
        private readonly Vector3[] _currentFrustumCorners = new Vector3[4];

        //Checkvariables to see which console variables have changed from the frame before
        private float _g_FarClip;
        private float _supersampling = 1;
        private int _forceShadowFiltering;
        private bool _forceShadowSS;
        private bool _ssr = true;
        private bool _g_SSReflectionNoise;

        //SDF
        // ToDo: @tpott: Move out of the pipeline as some sort of extension as it is only touched by the generator and the module
        private List<SignedDistanceField> _sdfDefinitions;


        //Render targets
        private GBufferTarget _gBufferTarget;
        private LightingBufferTarget _lightingBufferTarget;

        private RenderTarget2D _renderTargetDecalOffTarget;

        private RenderTarget2D _renderTargetComposed;
        private RenderTarget2D _renderTargetBloom;

        //TAA
        private RenderTarget2D _renderTargetTAA_1;
        private RenderTarget2D _renderTargetTAA_2;

        private RenderTarget2D _renderTargetSSR;

        private RenderTarget2D _renderTargetSSAOEffect;

        private RenderTarget2D _renderTargetScreenSpaceEffectUpsampleBlurVertical;

        private RenderTarget2D _renderTargetScreenSpaceEffectUpsampleBlurHorizontal;
        private RenderTarget2D _renderTargetScreenSpaceEffectBlurFinal;

        private RenderTarget2D _renderTargetOutput;

        private RenderTarget2D _currentOutput;

        //Cubemap
        private RenderTargetCube _renderTargetCubeMap;

        //Performance Profiler

        private readonly Stopwatch _performanceTimer = new Stopwatch();
        private long _performancePreviousTime;

        #endregion

        #region FUNCTIONS

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region BASE FUNCTIONS

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  BASE FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize variables
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content, ShaderManager shaderManager)
        {
            _inverseResolution = Vector2.One / RenderingSettings.g_ScreenResolution;

            _pointLightRenderModule = new PointLightRenderModule(shaderManager);
            _lightAccumulationModule = new LightAccumulationModule(shaderManager) { PointLightRenderModule = _pointLightRenderModule };
            _shadowMapRenderModule = new ShadowMapRenderModule(content, "Shaders/Shadow/ShadowMap");
            _gBufferRenderModule = new GBufferRenderModule(content, "Shaders/GbufferSetup/ClearGBuffer", "Shaders/GbufferSetup/Gbuffer");
            _forwardRenderModule = new ForwardRenderModule(content, "Shaders/forward/forward");
            _environmentProbeRenderModule = new EnvironmentProbeRenderModule(content, "Shaders/Deferred/DeferredEnvironmentMap");

            _bloomFx = new BloomFx(content);
            _taaFx = new TemporalAntialiasingFx(content);
            _colorGradingFx = new ColorGradingFx(content);

            _decalRenderModule = new DecalRenderModule(shaderManager);
            _helperGeometryRenderModule = new HelperGeometryRenderModule(content, "Shaders/Editor/LineEffect");
            _distanceFieldRenderModule = new DistanceFieldRenderModule(shaderManager, "Shaders/SignedDistanceFields/volumeProjection");

        }

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="assets"></param>
        public void Initialize(GraphicsDevice graphicsDevice, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);


            _gBufferTarget = new GBufferTarget(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);
            _lightingBufferTarget = new LightingBufferTarget(graphicsDevice, RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight);

            _editorRender = new EditorRender();
            _editorRender.Initialize(graphicsDevice, assets);


            _lightAccumulationModule.Initialize(graphicsDevice, assets);
            _gBufferRenderModule.Initialize(graphicsDevice);
            _forwardRenderModule.Initialize(graphicsDevice);

            _environmentProbeRenderModule.Initialize(graphicsDevice);
            _shadowMapRenderModule.Initialize(graphicsDevice);
            _decalRenderModule.Initialize(graphicsDevice);
            _helperGeometryRenderModule.Initialize(graphicsDevice);

            _bloomFx.Initialize(_graphicsDevice, RenderingSettings.g_ScreenResolution);
            _taaFx.Initialize(graphicsDevice, FullscreenTriangleBuffer.Instance);
            _colorGradingFx.Initialize(graphicsDevice, FullscreenTriangleBuffer.Instance);

            _assets = assets;
            //Apply some base settings to overwrite shader defaults with game settings defaults
            RenderingSettings.ApplySettings();

            Shaders.SSR.Param_NoiseMap.SetValue(_assets.NoiseMap);
            SetUpRenderTargets(RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight, false);

        }

        /// <summary>
        /// Update our function
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="isActive"></param>
        public void Update(GameTime gameTime, bool isActive, SdfGenerator sdfGenerator, List<ModelEntity> entities)
        {
            if (!isActive) return;
            _editorRender.Update(gameTime);

            //SDF Updating
            sdfGenerator.Update(entities, _graphicsDevice, _distanceFieldRenderModule, ref _sdfDefinitions);

        }

        #endregion

        #region RENDER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        /// <param name="camera">view point of the renderer</param>
        /// <param name="meshMaterialLibrary">a class that has stored all our mesh data</param>
        /// <param name="entities">entities and their properties</param>
        /// <param name="pointLights"></param>
        /// <param name="directionalLights"></param>
        /// <param name="gizmoContext">The data passed from our editor logic</param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public EditorLogic.EditorReceivedData Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<ModelEntity> entities, List<Decal> decals, List<DeferredPointLight> pointLights, List<DeferredDirectionalLight> directionalLights, EnvironmentProbe envSample, GizmoDrawContext gizmoContext, GameTime gameTime)
        {
            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges(directionalLights);

            //Render ShadowMaps
            DrawShadowMaps(meshMaterialLibrary, entities, pointLights, directionalLights, camera);

            //Update SDFs
            if (IsSDFUsed(pointLights))
            {
                _distanceFieldRenderModule.UpdateDistanceFieldTransformations(entities, _sdfDefinitions, _environmentProbeRenderModule, _graphicsDevice, _spriteBatch, _lightAccumulationModule);
            }
            //Render EnvironmentMaps
            //We do this either when pressing C or at the start of the program (_renderTargetCube == null) or when the game settings want us to do it every frame
            if (envSample.NeedsUpdate || RenderingSettings.g_envmapupdateeveryframe)
            {
                DrawCubeMap(envSample.Position, meshMaterialLibrary, entities, pointLights, directionalLights, envSample, 300, gameTime, camera);
                envSample.NeedsUpdate = false;
            }

            //Update our view projection matrices if the camera moved
            UpdateViewProjection(meshMaterialLibrary, entities, camera);

            //Draw our meshes to the G Buffer
            DrawGBuffer(meshMaterialLibrary);

            //Deferred Decals
            DrawDecals(decals);

            //Draw Screen Space reflections to a different render target
            DrawScreenSpaceReflections(gameTime);

            //SSAO
            DrawScreenSpaceAmbientOcclusion(camera);

            //Screen space shadows for directional lights to an offscreen render target
            DrawScreenSpaceDirectionalShadow(directionalLights);

            //Upsample/blur our SSAO / screen space shadows
            DrawBilateralBlur();

            //Light the scene
            _lightAccumulationModule.DrawLights(pointLights, directionalLights, camera.Position, gameTime, _lightingBufferTarget.Bindings, _lightingBufferTarget.Diffuse);

            //Draw the environment cube map as a fullscreen effect on all meshes
            DrawEnvironmentMap(envSample, camera, gameTime);

            //Compose the scene by combining our lighting data with the gbuffer data
            _currentOutput = Compose(); //-> output _renderTargetComposed

            //Forward
            _currentOutput = DrawForward(_currentOutput, meshMaterialLibrary, camera, pointLights);

            //Compose the image and add information from previous frames to apply temporal super sampling
            _currentOutput = TonemapAndCombineTemporalAntialiasing(_currentOutput); // -> output: _temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1

            //Do Bloom
            _currentOutput = DrawBloom(_currentOutput); // -> output: _renderTargetBloom

            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.e_IsEditorEnabled && RenderingStats.e_EnableSelection)
                _editorRender.DrawIds(meshMaterialLibrary, decals, pointLights, directionalLights, envSample, _staticViewProjection, _view, gizmoContext);

            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode(_currentOutput);

            //Draw signed distance field functions
            DrawSignedDistanceFieldFunctions(camera);

            //Additional editor elements that overlay our screen

            RenderEditorOverlays(gizmoContext, meshMaterialLibrary, decals, pointLights, directionalLights, envSample);

            //Draw debug geometry
            _helperGeometryRenderModule.ViewProjection = _staticViewProjection;
            _helperGeometryRenderModule.Draw();

            //Set up the frustum culling for the next frame
            meshMaterialLibrary.FrustumCullingFinalizeFrame(entities);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileTotalRender = performanceCurrentTime;
            }

            //return data we have recovered from the editor id, so we know what entity gets hovered/clicked on and can manipulate in the update function
            return new EditorLogic.EditorReceivedData
            {
                HoveredId = _editorRender.GetHoveredId(),
                ViewMatrix = _view,
                ProjectionMatrix = _projection
            };
        }

        private bool IsSDFUsed(List<DeferredPointLight> pointLights)
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

        private void RenderEditorOverlays(GizmoDrawContext gizmoContext,
            MeshMaterialLibrary meshMaterialLibrary,
            List<Decal> decals,
            List<DeferredPointLight> pointLights,
            List<DeferredDirectionalLight> directionalLights,
            EnvironmentProbe envSample)
        {

            if (RenderingSettings.e_IsEditorEnabled && RenderingStats.e_EnableSelection)
            {
                if (RenderingSettings.e_drawoutlines)
                    DrawTextureToScreenToFullScreen(_editorRender.GetOutlines(), BlendState.Additive);

                _editorRender.DrawEditorElements(meshMaterialLibrary, decals, pointLights, directionalLights, envSample, _staticViewProjection, _view, gizmoContext);


                if (gizmoContext.SelectedObject != null)
                {
                    if (gizmoContext.SelectedObject is Decal)
                    {
                        _decalRenderModule.DrawOutlines(gizmoContext.SelectedObject as Decal, _staticViewProjection, _view);
                    }

                    if (RenderingSettings.e_drawboundingbox)
                        if (gizmoContext.SelectedObject is ModelEntity)
                        {
                            HelperGeometryManager.GetInstance()
                                .AddBoundingBox(gizmoContext.SelectedObject as ModelEntity);
                        }
                }
            }

            if (RenderingSettings.sdf_debug && _distanceFieldRenderModule.GetAtlas() != null)
            {
                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
                _spriteBatch.Draw(_distanceFieldRenderModule.GetAtlas(), new Rectangle(0, RenderingSettings.g_ScreenHeight - 200, RenderingSettings.g_ScreenWidth, 200), Color.White);
                _spriteBatch.End();
            }

        }

        /// <summary>
        /// Another draw function, but this time for cubemaps. Doesn't need all the stuff we have in the main draw function
        /// </summary>
        /// <param name="origin">from where do we render the cubemap</param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        /// <param name="pointLights"></param>
        /// <param name="dirLights"></param>
        /// <param name="farPlane"></param>
        /// <param name="gameTime"></param>
        /// <param name="camera"></param>
        private void DrawCubeMap(Vector3 origin,
            MeshMaterialLibrary meshMaterialLibrary,
            List<ModelEntity> entities,
            List<DeferredPointLight> pointLights,
            List<DeferredDirectionalLight> dirLights,
            EnvironmentProbe envSample,
            float farPlane, GameTime gameTime, Camera camera)
        {
            //If our cubemap is not yet initialized, create a new one
            if (_renderTargetCubeMap == null)
            {
                //Create a new cube map
                _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, RenderingSettings.g_envmapresolution, true, SurfaceFormat.HalfVector4,
                    DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                //Set this cubemap in the shader of the environment map
                _environmentProbeRenderModule.Cubemap = _renderTargetCubeMap;
            }

            //Set up all the base rendertargets with the resolution of our cubemap
            SetUpRenderTargets(RenderingSettings.g_envmapresolution, RenderingSettings.g_envmapresolution, true);

            //We don't want to use SSAO in this cubemap
            Shaders.DeferredCompose.Param_UseSSAO.SetValue(false);

            //Create our projection, which is a basic pyramid
            _projection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, farPlane);

            //Now we need to actually render for each cubemapface (6 direcetions)
            for (int i = 0; i < 6; i++)
            {
                // render the scene to all cubemap faces
                CubeMapFace cubeMapFace = (CubeMapFace)i;
                switch (cubeMapFace)
                {
                    case CubeMapFace.NegativeX:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Left, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.NegativeY:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Down, Vector3.Forward);
                            break;
                        }
                    case CubeMapFace.NegativeZ:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Backward, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveX:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Right, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveY:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Up, Vector3.Backward);
                            break;
                        }
                    case CubeMapFace.PositiveZ:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Forward, Vector3.Up);
                            break;
                        }
                }

                //Create our projection matrices
                _inverseView = Matrix.Invert(_view);
                _viewProjection = _view * _projection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);
                _viewIT = Matrix.Transpose(_inverseView);

                //Pass these values to our shader
                Shaders.SSAO.Param_InverseViewProjection.SetValue(_inverseView);
                _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                //yep we changed
                _viewProjectionHasChanged = true;

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_viewProjection);
                else _boundingFrustum.Matrix = _viewProjection;
                ComputeFrustumCorners(_boundingFrustum, camera);

                _lightAccumulationModule.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _view, _inverseView, _viewIT, _projection, _viewProjection, _inverseViewProjection);

                //Base stuff, for description look in Draw()
                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, true, origin);

                DrawGBuffer(meshMaterialLibrary);

                bool volumeEnabled = RenderingSettings.g_VolumetricLights;
                RenderingSettings.g_VolumetricLights = false;
                _lightAccumulationModule.DrawLights(pointLights, dirLights, origin, gameTime, _lightingBufferTarget.Bindings, _lightingBufferTarget.Diffuse);

                _environmentProbeRenderModule.DrawSky(_graphicsDevice, FullscreenTarget);

                RenderingSettings.g_VolumetricLights = volumeEnabled;

                //We don't use temporal AA obviously for the cubemap
                bool tempAa = RenderingSettings.g_taa;
                RenderingSettings.g_taa = false;

                Compose();

                RenderingSettings.g_taa = tempAa;
                DrawTextureToScreenToCube(_renderTargetComposed, _renderTargetCubeMap, cubeMapFace);
            }
            Shaders.DeferredCompose.Param_UseSSAO.SetValue(RenderingSettings.g_ssao_draw);

            //Change RTs back to normal
            SetUpRenderTargets(RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight, true);

            //Our camera has changed we need to reinitialize stuff because we used a different camera in the cubemap render
            camera.HasChanged = true;

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawCubeMap = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        #endregion

        #region DEFERRED RENDERING FUNCTIONS
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

            //Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                _performanceTimer.Restart();
                _performancePreviousTime = 0;
            }
            else if (_performanceTimer.IsRunning)
            {
                _performanceTimer.Stop();
            }
        }

        /// <summary>
        /// Check whether any GameSettings have changed that need setup
        /// </summary>
        /// <param name="dirLights"></param>
        private void CheckRenderChanges(List<DeferredDirectionalLight> dirLights)
        {
            if (Math.Abs(_g_FarClip - RenderingSettings.g_farplane) > 0.0001f)
            {
                _g_FarClip = RenderingSettings.g_farplane;
                _gBufferRenderModule.FarClip = _g_FarClip;
                _decalRenderModule.FarClip = _g_FarClip;
                _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_FarClip.SetValue(_g_FarClip);
                Shaders.Billboard.Param_FarClip.SetValue(_g_FarClip);
                Shaders.SSR.Param_FarClip.SetValue(_g_FarClip);
                Shaders.ReconstructDepth.Param_FarClip.SetValue(_g_FarClip);
            }

            if (_g_SSReflectionNoise != RenderingSettings.g_SSReflectionNoise)
            {
                _g_SSReflectionNoise = RenderingSettings.g_SSReflectionNoise;
                if (!_g_SSReflectionNoise) Shaders.SSR.Param_Time.SetValue(0.0f);
            }

            if (_forceShadowFiltering != RenderingSettings.g_shadowforcefiltering)
            {
                _forceShadowFiltering = RenderingSettings.g_shadowforcefiltering;

                for (var index = 0; index < dirLights.Count; index++)
                {
                    DeferredDirectionalLight light = dirLights[index];
                    if (light.ShadowMap != null) light.ShadowMap.Dispose();
                    light.ShadowMap = null;

                    light.ShadowFiltering = (DeferredDirectionalLight.ShadowFilteringTypes)(_forceShadowFiltering - 1);

                    light.HasChanged = true;
                }
            }

            if (_forceShadowSS != RenderingSettings.g_shadowforcescreenspace)
            {
                _forceShadowSS = RenderingSettings.g_shadowforcescreenspace;

                for (var index = 0; index < dirLights.Count; index++)
                {
                    DeferredDirectionalLight light = dirLights[index];
                    light.ScreenSpaceShadowBlur = _forceShadowSS;

                    light.HasChanged = true;
                }
            }

            if (_ssr != RenderingSettings.g_SSReflection)
            {
                _graphicsDevice.SetRenderTarget(_renderTargetSSR);
                _graphicsDevice.Clear(new Color(0, 0, 0, 0.0f));

                _ssr = RenderingSettings.g_SSReflection;
            }

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileRenderChanges = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw our shadow maps from the individual lights. Check if something has changed first, otherwise leave as it is
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        /// <param name="pointLights"></param>
        /// <param name="dirLights"></param>
        /// <param name="camera"></param>
        private void DrawShadowMaps(MeshMaterialLibrary meshMaterialLibrary,
            List<ModelEntity> entities,
            List<DeferredPointLight> pointLights,
            List<DeferredDirectionalLight> dirLights,
            Camera camera)
        {
            //Don't render for the first frame, we need a guideline first
            if (_boundingFrustum == null) UpdateViewProjection(meshMaterialLibrary, entities, camera);

            _shadowMapRenderModule.Draw(meshMaterialLibrary, entities, pointLights, dirLights, camera);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawShadows = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Create the projection matrices
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        /// <param name="camera"></param>
        private void UpdateViewProjection(
            MeshMaterialLibrary meshMaterialLibrary,
            List<ModelEntity> entities,
            Camera camera)
        {
            _viewProjectionHasChanged = camera.HasChanged;

            //alternate frames with temporal aa
            if (RenderingSettings.g_taa)
            {
                _viewProjectionHasChanged = true;
                _temporalAAOffFrame = !_temporalAAOffFrame;
            }

            //If the camera didn't do anything we don't need to update this stuff
            if (_viewProjectionHasChanged)
            {
                //We have processed the change, now setup for next frame as false
                camera.HasChanged = false;
                camera.HasMoved = false;

                //View matrix
                _view = Matrix.CreateLookAt(camera.Position, camera.Lookat, camera.Up);
                _inverseView = Matrix.Invert(_view);

                _viewIT = Matrix.Transpose(_inverseView);

                _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                _projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView, RenderingSettings.g_ScreenAspect, 1, RenderingSettings.g_farplane);

                _gBufferRenderModule.Camera = camera.Position;

                _viewProjection = _view * _projection;

                //this is the unjittered viewProjection. For some effects we don't want the jittered one
                _staticViewProjection = _viewProjection;

                //Transformation for TAA - from current view back to the old view projection
                _currentViewToPreviousViewProjection = Matrix.Invert(_view) * _previousViewProjection;

                //Temporal AA
                if (RenderingSettings.g_taa)
                {
                    switch (RenderingSettings.g_taa_jittermode)
                    {
                        case 0: //2 frames, just basic translation. Worst taa implementation. Not good with the continous integration used
                            {
                                Vector2 translation = Vector2.One * (_temporalAAOffFrame ? 0.5f : -0.5f);
                                _viewProjection = _viewProjection * (translation / RenderingSettings.g_ScreenResolution).ToMatrixTranslationXY();
                            }
                            break;
                        case 1: // Just random translation
                            {
                                float randomAngle = FastRand.NextAngle();
                                Vector2 translation = (new Vector2((float)Math.Sin(randomAngle), (float)Math.Cos(randomAngle)) / RenderingSettings.g_ScreenResolution) * 0.5f; ;
                                _viewProjection = _viewProjection * translation.ToMatrixTranslationXY();

                            }
                            break;
                        case 2: // Halton sequence, default
                            {
                                Vector3 translation = GetHaltonSequence();
                                _viewProjection = _viewProjection * Matrix.CreateTranslation(translation);
                            }
                            break;
                    }
                }

                _previousViewProjection = _viewProjection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_staticViewProjection);
                else _boundingFrustum.Matrix = _staticViewProjection;

                // Compute the frustum corners for cheap view direction computation in shaders
                ComputeFrustumCorners(_boundingFrustum, camera);
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, _viewProjectionHasChanged, camera.Position);

            _lightAccumulationModule.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _view, _inverseView, _viewIT, _projection, _viewProjection, _inverseViewProjection);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileUpdateViewProjection = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// The halton sequence is a good way to create good distribution
        /// I use a 2,3 sequence
        /// https://en.wikipedia.org/wiki/Halton_sequence
        /// </summary>
        /// <returns></returns>
        private Vector3 GetHaltonSequence()
        {
            //First time? Create the sequence
            if (_haltonSequence == null)
            {
                _haltonSequence = new Vector3[HaltonSequenceLength];
                for (int index = 0; index < HaltonSequenceLength; index++)
                {
                    for (int baseValue = 2; baseValue <= 3; baseValue++)
                    {
                        float result = 0;
                        float f = 1;
                        int i = index + 1;

                        while (i > 0)
                        {
                            f = f / baseValue;
                            result = result + f * (i % baseValue);
                            i = i / baseValue; //floor / int()
                        }

                        if (baseValue == 2)
                            _haltonSequence[index].X = (result - 0.5f) * 2 * _inverseResolution.X;
                        else
                            _haltonSequence[index].Y = (result - 0.5f) * 2 * _inverseResolution.Y;
                    }
                }
            }
            _haltonSequenceIndex++;
            if (_haltonSequenceIndex >= HaltonSequenceLength) _haltonSequenceIndex = 0;
            return _haltonSequence[_haltonSequenceIndex];
        }

        /// <summary>
        /// From https://jcoluna.wordpress.com/2011/01/18/xna-4-0-light-pre-pass/
        /// Compute the frustum corners for a camera.
        /// Its used to reconstruct the pixel position using only the depth value.
        /// Read here for more information
        /// http://mynameismjp.wordpress.com/2009/03/10/reconstructing-position-from-depth/
        /// </summary>
        /// <param name="cameraFrustum"></param>
        private void ComputeFrustumCorners(BoundingFrustum cameraFrustum, Camera camera)
        {
            cameraFrustum.GetCorners(_cornersWorldSpace);

            /*this part is used for volume projection*/
            //World Space Corners - Camera Position
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _currentFrustumCorners[i] = _cornersWorldSpace[i + 4] - camera.Position;
            }
            Vector3 temp = _currentFrustumCorners[3];
            _currentFrustumCorners[3] = _currentFrustumCorners[2];
            _currentFrustumCorners[2] = temp;

            _distanceFieldRenderModule.FrustumCornersWorldSpace = _currentFrustumCorners;
            _environmentProbeRenderModule.FrustumCornersWS = _currentFrustumCorners;

            //View Space Corners
            //this is the inverse of our camera transform
            Vector3.Transform(_cornersWorldSpace, ref _view, _cornersViewSpace); //put the frustum into view space
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _currentFrustumCorners[i] = _cornersViewSpace[i + 4];
            }
            temp = _currentFrustumCorners[3];
            _currentFrustumCorners[3] = _currentFrustumCorners[2];
            _currentFrustumCorners[2] = temp;

            Shaders.SSR.Param_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.SSAO.Param_FrustumCorners.SetValue(_currentFrustumCorners);
            _taaFx.FrustumCorners = _currentFrustumCorners;
            Shaders.ReconstructDepth.Param_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.DeferredDirectionalLight.Param_FrustumCorners.SetValue(_currentFrustumCorners);
        }

        /// <summary>
        /// Draw all our meshes to the GBuffer - albedo, normal, depth - for further computation
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        private void DrawGBuffer(MeshMaterialLibrary meshMaterialLibrary)
        {
            _gBufferRenderModule.Draw(_gBufferTarget.Bindings, meshMaterialLibrary, _viewProjection, _view);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw deferred Decals
        /// </summary>
        /// <param name="decals"></param>
        private void DrawDecals(List<Decal> decals)
        {
            if (!RenderingSettings.g_EnableDecals) return;

            //First copy albedo to decal offtarget
            DrawTextureToScreenToFullScreen(_gBufferTarget.Albedo, BlendState.Opaque, _renderTargetDecalOffTarget);

            DrawTextureToScreenToFullScreen(_renderTargetDecalOffTarget, BlendState.Opaque, _gBufferTarget.Albedo);

            _decalRenderModule.Draw(decals, _view, _viewProjection, _inverseView);
        }

        /// <summary>
        /// Draw Screen Space Reflections
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawScreenSpaceReflections(GameTime gameTime)
        {
            if (!RenderingSettings.g_SSReflection) return;


            //todo: more samples for more reflective materials!
            _graphicsDevice.SetRenderTarget(_renderTargetSSR);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (RenderingSettings.g_taa)
            {
                Shaders.SSR.Param_TargetMap.SetValue(_temporalAAOffFrame ? _renderTargetTAA_1 : _renderTargetTAA_2);
            }
            else
            {
                Shaders.SSR.Param_TargetMap.SetValue(_renderTargetComposed);
            }

            if (RenderingSettings.g_SSReflectionNoise)
                Shaders.SSR.Param_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            Shaders.SSR.Param_Projection.SetValue(_projection);

            Shaders.SSR.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);

            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawSSR = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

        }

        /// <summary>
        /// Draw SSAO to a different rendertarget
        /// </summary>
        /// <param name="camera"></param>
        private void DrawScreenSpaceAmbientOcclusion(Camera camera)
        {
            if (!RenderingSettings.g_ssao_draw) return;

            _graphicsDevice.SetRenderTarget(_renderTargetSSAOEffect);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.SSAO.Param_InverseViewProjection.SetValue(_inverseViewProjection);
            Shaders.SSAO.Param_Projection.SetValue(_projection);
            Shaders.SSAO.Param_ViewProjection.SetValue(_viewProjection);
            Shaders.SSAO.Param_CameraPosition.SetValue(camera.Position);

            Shaders.SSAO.Effect.CurrentTechnique = Shaders.SSAO.Technique_SSAO;
            Shaders.SSAO.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawScreenSpaceEffect = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Screen space blur for directional lights
        /// </summary>
        /// <param name="dirLights"></param>
        private void DrawScreenSpaceDirectionalShadow(List<DeferredDirectionalLight> dirLights)
        {
            if (_viewProjectionHasChanged)
            {
                Shaders.DeferredDirectionalLight.Param_ViewProjection.SetValue(_viewProjection);
                Shaders.DeferredDirectionalLight.Param_InverseViewProjection.SetValue(_inverseViewProjection);

            }
            for (var index = 0; index < dirLights.Count; index++)
            {
                DeferredDirectionalLight light = dirLights[index];
                if (light.CastShadows && light.ScreenSpaceShadowBlur)
                {
                    throw new NotImplementedException();

                    /*
                    //Draw our map!
                    _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

                    Shaders.deferredDirectionalLightParameter_LightDirection.SetValue(light.Direction);

                    if (_viewProjectionHasChanged)
                    {
                        light.DirectionViewSpace = Vector3.Transform(light.Direction, _viewIT);
                        light.LightViewProjection_ViewSpace = _inverseView * light.LightViewProjection;
                        light.LightView_ViewSpace = _inverseView * light.LightView;
                    }

                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(light
                        .LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameterLightView.SetValue(light.LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(light.ShadowMap);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int) light.ShadowFiltering);
                    Shaders.deferredDirectionalLightParameter_ShadowMapSize.SetValue((float) light.ShadowResolution);

                    Shaders.deferredDirectionalLightShadowOnly.Passes[0].Apply();

                    _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
                    */
                }
            }

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawScreenSpaceDirectionalShadow = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Bilateral blur, to upsample our undersampled SSAO
        /// </summary>
        private void DrawBilateralBlur()
        {
            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

            _spriteBatch.Begin(0, BlendState.Additive);

            _spriteBatch.Draw(_renderTargetSSAOEffect, RenderingSettings.g_ScreenRect, Color.Red);

            _spriteBatch.End();

            if (RenderingSettings.g_ssao_blur && RenderingSettings.g_ssao_draw)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);

                Shaders.SSAO.Param_InverseResolution.SetValue(new Vector2(1.0f / _renderTargetScreenSpaceEffectUpsampleBlurVertical.Width, 1.0f / _renderTargetScreenSpaceEffectUpsampleBlurVertical.Height) * 2);
                Shaders.SSAO.Param_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
                Shaders.SSAO.Technique_BlurVertical.Passes[0].Apply();

                FullscreenTarget.Draw(_graphicsDevice);

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                Shaders.SSAO.Param_InverseResolution.SetValue(new Vector2(1.0f / _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Width, 1.0f / _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Height) * 0.5f);
                Shaders.SSAO.Param_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);
                Shaders.SSAO.Technique_BlurHorizontal.Passes[0].Apply();

                FullscreenTarget.Draw(_graphicsDevice);

            }
            else
            {
                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp);

                _spriteBatch.Draw(_renderTargetScreenSpaceEffectUpsampleBlurVertical, new Rectangle(0, 0, _renderTargetScreenSpaceEffectBlurFinal.Width, _renderTargetScreenSpaceEffectBlurFinal.Height), Color.White);

                _spriteBatch.End();
            }

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawBilateralBlur = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Apply our environment cubemap to the renderer
        /// </summary>
        private void DrawEnvironmentMap(EnvironmentProbe envSample, Camera camera, GameTime gameTime)
        {
            if (!RenderingSettings.g_environmentmapping) return;

            _environmentProbeRenderModule.DrawEnvironmentMap(camera, _view, FullscreenTarget, envSample, gameTime, RenderingSettings.g_SSReflection_FireflyReduction, RenderingSettings.g_SSReflection_FireflyThreshold);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawEnvironmentMap = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Compose the render by combining the albedo channel with the light channels
        /// </summary>
        private RenderTarget2D Compose()
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _graphicsDevice.SetRenderTarget(_renderTargetComposed);
            _graphicsDevice.BlendState = BlendState.Opaque;

            //combine!
            Shaders.DeferredCompose.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileCompose = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

            return _renderTargetComposed;
        }

        private void ReconstructDepth()
        {
            if (_viewProjectionHasChanged)
                Shaders.ReconstructDepth.Param_Projection.SetValue(_projection);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.ReconstructDepth.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);
        }

        private RenderTarget2D DrawForward(RenderTarget2D input, MeshMaterialLibrary meshMaterialLibrary, Camera camera, List<DeferredPointLight> pointLights)
        {
            if (!RenderingSettings.g_EnableForward) return input;

            _graphicsDevice.SetRenderTarget(input);
            ReconstructDepth();

            return _forwardRenderModule.Draw(input, meshMaterialLibrary, _viewProjection, camera, pointLights, _boundingFrustum);
        }

        private RenderTarget2D DrawBloom(RenderTarget2D input)
        {
            if (RenderingSettings.g_BloomEnable)
            {
                Texture2D bloom = _bloomFx.Draw(input, RenderingSettings.g_ScreenResolution);

                _graphicsDevice.SetRenderTargets(_renderTargetBloom);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                _spriteBatch.Draw(input, RenderingSettings.g_ScreenRect, Color.White);
                _spriteBatch.Draw(bloom, RenderingSettings.g_ScreenRect, Color.White);

                _spriteBatch.End();

                return _renderTargetBloom;
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
            if (!RenderingSettings.g_taa) return input;

            RenderTarget2D output = _temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1;
            _taaFx.UseTonemap = RenderingSettings.g_taa_tonemapped;
            _taaFx.CurrentViewToPreviousViewProjection = _currentViewToPreviousViewProjection;
            _taaFx.Draw(currentFrame: input, previousFrames: _temporalAAOffFrame ? _renderTargetTAA_1 : _renderTargetTAA_2, output: output);

            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileCombineTemporalAntialiasing = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

            return RenderingSettings.g_taa_tonemapped ? input : output;
        }

        private void DrawSignedDistanceFieldFunctions(Camera camera)
        {
            if (!RenderingSettings.sdf_drawdistance) return;

            _distanceFieldRenderModule.Draw(_graphicsDevice, camera);
        }

        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        /// <param name="currentOutput"></param>
        /// <param name="editorData"></param>
        private void RenderMode(RenderTarget2D currentInput)
        {
            switch (RenderingSettings.g_rendermode)
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
                    DrawTextureToScreenToFullScreen(_renderTargetSSAOEffect);
                    break;
                case RenderModes.SSBlur:
                    DrawTextureToScreenToFullScreen(_renderTargetScreenSpaceEffectBlurFinal);
                    break;
                case RenderModes.SSR:
                    DrawTextureToScreenToFullScreen(_renderTargetSSR);
                    break;
                case RenderModes.HDR:
                    DrawTextureToScreenToFullScreen(currentInput);
                    break;
                default:
                    DrawPostProcessing(currentInput);
                    break;
            }


            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                RenderingStats.d_profileDrawFinalRender = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Add some post processing to the image
        /// </summary>
        /// <param name="currentInput"></param>
        private void DrawPostProcessing(RenderTarget2D currentInput)
        {
            if (!RenderingSettings.g_PostProcessing) return;

            RenderTarget2D destinationRenderTarget;

            destinationRenderTarget = _renderTargetOutput;

            Shaders.PostProcssing.Param_ScreenTexture.SetValue(currentInput);
            _graphicsDevice.SetRenderTarget(destinationRenderTarget);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.PostProcssing.Effect.CurrentTechnique.Passes[0].Apply();
            FullscreenTarget.Draw(_graphicsDevice);

            if (RenderingSettings.g_ColorGrading)
                destinationRenderTarget = _colorGradingFx.Draw(destinationRenderTarget);

            DrawTextureToScreenToFullScreen(destinationRenderTarget);
        }
        #endregion

        #endregion

        #region RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update the resolution of our rendertargets
        /// </summary>
        public void UpdateResolution()
        {
            _inverseResolution = Vector2.One / RenderingSettings.g_ScreenResolution;
            _haltonSequence = null;

            SetUpRenderTargets(RenderingSettings.g_ScreenWidth, RenderingSettings.g_ScreenHeight, false);
        }

        private void SetUpRenderTargets(int width, int height, bool onlyEssentials)
        {
            //Discard first
            if (_renderTargetComposed != null)
            {
                _renderTargetDecalOffTarget.Dispose();
                _renderTargetComposed.Dispose();
                _renderTargetBloom.Dispose();
                _renderTargetOutput.Dispose();

                _renderTargetScreenSpaceEffectUpsampleBlurVertical.Dispose();

                if (!onlyEssentials)
                {
                    _renderTargetTAA_1.Dispose();
                    _renderTargetTAA_2.Dispose();
                    _renderTargetSSAOEffect.Dispose();
                    _renderTargetSSR.Dispose();

                    _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Dispose();
                    _renderTargetScreenSpaceEffectBlurFinal.Dispose();
                }
            }

            float ssmultiplier = _supersampling;

            int targetWidth = (int)(width * ssmultiplier);
            int targetHeight = (int)(height * ssmultiplier);

            Shaders.Billboard.Param_AspectRatio.SetValue((float)targetWidth / targetHeight);

            // Update multi render target size
            _gBufferTarget.Resize(targetWidth, targetHeight);
            _lightingBufferTarget.Resize(targetWidth, targetHeight);

            _renderTargetDecalOffTarget = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_Resolution.SetValue(new Vector2(targetWidth, targetHeight));

            _renderTargetComposed = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetBloom = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetScreenSpaceEffectUpsampleBlurVertical = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetOutput = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            if (!onlyEssentials)
            {
                _editorRender.SetUpRenderTarget(width, height);

                _renderTargetTAA_1 = new RenderTarget2D(_graphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                _renderTargetTAA_2 = new RenderTarget2D(_graphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                _taaFx.Resolution = new Vector2(targetWidth, targetHeight);

                Shaders.SSR.Param_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                _environmentProbeRenderModule.Resolution = new Vector2(targetWidth, targetHeight);
                _renderTargetSSR = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);



                ///////////////////
                // HALF RESOLUTION

                targetWidth /= 2;
                targetHeight /= 2;

                _renderTargetScreenSpaceEffectUpsampleBlurHorizontal = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetScreenSpaceEffectBlurFinal = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetSSAOEffect = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.HalfSingle, DepthFormat.None, 0,
                    RenderTargetUsage.DiscardContents);



                Shaders.SSAO.Param_InverseResolution.SetValue(new Vector2(1.0f / targetWidth,
                    1.0f / targetHeight));


                Vector2 aspectRatio = new Vector2(Math.Min(1.0f, targetWidth / (float)targetHeight), Math.Min(1.0f, targetHeight / (float)targetWidth));

                Shaders.SSAO.Param_AspectRatio.SetValue(aspectRatio);

            }

            UpdateRenderMapBindings(onlyEssentials);
        }

        private void UpdateRenderMapBindings(bool onlyEssentials)
        {
            Shaders.Billboard.Param_DepthMap.SetValue(_gBufferTarget.Depth);

            Shaders.ReconstructDepth.Param_DepthMap.SetValue(_gBufferTarget.Depth);

            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_AlbedoMap.SetValue(_gBufferTarget.Albedo);
            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_DepthMap.SetValue(_gBufferTarget.Depth);
            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_NormalMap.SetValue(_gBufferTarget.Normal);

            Shaders.DeferredDirectionalLight.Param_AlbedoMap.SetValue(_gBufferTarget.Albedo);
            Shaders.DeferredDirectionalLight.Param_DepthMap.SetValue(_gBufferTarget.Depth);
            Shaders.DeferredDirectionalLight.Param_NormalMap.SetValue(_gBufferTarget.Normal);

            Shaders.DeferredDirectionalLight.Param_SSShadowMap.SetValue(onlyEssentials ? _renderTargetScreenSpaceEffectUpsampleBlurVertical : _renderTargetScreenSpaceEffectBlurFinal);

            _environmentProbeRenderModule.AlbedoMap = _gBufferTarget.Albedo;
            _environmentProbeRenderModule.NormalMap = _gBufferTarget.Normal;
            _environmentProbeRenderModule.SSRMap = _renderTargetSSR;
            _environmentProbeRenderModule.DepthMap = _gBufferTarget.Depth;

            _decalRenderModule.DepthMap = _gBufferTarget.Depth;

            _distanceFieldRenderModule.DepthMap = _gBufferTarget.Depth;


            Shaders.DeferredCompose.Param_ColorMap.SetValue(_gBufferTarget.Albedo);
            Shaders.DeferredCompose.Param_NormalMap.SetValue(_gBufferTarget.Normal);
            Shaders.DeferredCompose.Param_diffuseLightMap.SetValue(_lightingBufferTarget.Diffuse);
            Shaders.DeferredCompose.Param_specularLightMap.SetValue(_lightingBufferTarget.Specular);
            Shaders.DeferredCompose.Param_volumeLightMap.SetValue(_lightingBufferTarget.Volume);
            Shaders.DeferredCompose.Param_SSAOMap.SetValue(_renderTargetScreenSpaceEffectBlurFinal);

            Shaders.SSAO.Param_NormalMap.SetValue(_gBufferTarget.Normal);

            Shaders.SSAO.Param_DepthMap.SetValue(_gBufferTarget.Depth);
            Shaders.SSAO.Param_SSAOMap.SetValue(_renderTargetSSAOEffect);

            Shaders.SSR.Param_DepthMap.SetValue(_gBufferTarget.Depth);
            Shaders.SSR.Param_NormalMap.SetValue(_gBufferTarget.Normal);

            _taaFx.DepthMap = _gBufferTarget.Depth;
        }

        #endregion

        #region HELPER FUNCTIONS
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

            int height;
            int width;
            if (Math.Abs(texture.Width / (float)texture.Height - RenderingSettings.g_ScreenWidth / (float)RenderingSettings.g_ScreenHeight) < 0.001)
            //If same aspectratio
            {
                height = RenderingSettings.g_ScreenHeight;
                width = RenderingSettings.g_ScreenWidth;
            }
            else
            {
                if (RenderingSettings.g_ScreenHeight < RenderingSettings.g_ScreenWidth)
                {
                    //Should be squared!
                    height = RenderingSettings.g_ScreenHeight;
                    width = RenderingSettings.g_ScreenHeight;
                }
                else
                {
                    height = RenderingSettings.g_ScreenWidth;
                    width = RenderingSettings.g_ScreenWidth;
                }
            }
            _graphicsDevice.SetRenderTarget(output);
            _spriteBatch.Begin(0, blendState, _supersampling > 1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
            _spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();
        }

        #endregion

        #endregion

        public void Dispose()
        {
            _graphicsDevice?.Dispose();
            _spriteBatch?.Dispose();
            _bloomFx?.Dispose();
            _lightAccumulationModule?.Dispose();
            _gBufferRenderModule?.Dispose();
            _taaFx?.Dispose();
            _environmentProbeRenderModule?.Dispose();
            _decalRenderModule?.Dispose();
            _assets?.Dispose();
            _gBufferTarget?.Dispose();
            _lightingBufferTarget?.Dispose();
            _renderTargetDecalOffTarget?.Dispose();
            _renderTargetComposed?.Dispose();
            _renderTargetBloom?.Dispose();
            _renderTargetTAA_1?.Dispose();
            _renderTargetTAA_2?.Dispose();
            _renderTargetSSR?.Dispose();
            _renderTargetSSAOEffect?.Dispose();
            _renderTargetScreenSpaceEffectUpsampleBlurVertical?.Dispose();
            _renderTargetScreenSpaceEffectUpsampleBlurHorizontal?.Dispose();
            _renderTargetScreenSpaceEffectBlurFinal?.Dispose();
            _renderTargetOutput?.Dispose();
            _currentOutput?.Dispose();
            _renderTargetCubeMap?.Dispose();
        }
    }

}

