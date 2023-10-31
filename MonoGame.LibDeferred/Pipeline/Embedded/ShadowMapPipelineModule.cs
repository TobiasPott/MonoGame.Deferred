using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DirectionalLight = DeferredEngine.Pipeline.Lighting.DirectionalLight;

namespace DeferredEngine.Pipeline
{

    public class ShadowMapPipelineModule : PipelineModule, IRenderModule
    {
        public static float ShadowBias = 0.005f;

        private ShadowPasses _pass;

        private BoundingFrustum _boundingFrustumShadow;
        private readonly ShadowMapFxSetup _fxSetup = new ShadowMapFxSetup();


        private enum ShadowPasses
        {
            Directional,
            Omnidirectional,
            OmnidirectionalAlpha
        };

        public ShadowMapPipelineModule()
            : base() { }

        //public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        //{
        //    base.Initialize(graphicsDevice, spriteBatch);
        //}

        public void Draw(DynamicMeshBatcher meshBatcher, EntityScene scene)
        {
            List<PointLight> pointLights = scene.PointLights;
            List<Pipeline.Lighting.DirectionalLight> dirLights = scene.DirectionalLights;

            _pass = ShadowPasses.Omnidirectional;

            //Go through all our point lights
            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLight light = pointLights[index];

                if (!light.IsEnabled) continue;

                //If we don't see the light we shouldn't update. This is actually wrong, can lead to mistakes,
                //if we implement it like this we should rerender once we enter visible space again.
                //if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint)
                //{
                //    continue;
                //}

                if (light.CastShadows)
                {
                    //A poing light has 6 shadow maps, add that to our stat counter. These are total shadow maps, not updated ones
                    RenderingStats.shadowMaps += 6;

                    //Update if we didn't initialize yet or if we are dynamic
                    if (light.HasChanged || light.ShadowMap == null)
                    {
                        CreateShadowCubeMap(light, meshBatcher);

                        light.HasChanged = false;
                    }
                }
            }

            _pass = ShadowPasses.Directional;

            for (int index = 0; index < dirLights.Count; index++)
            {
                Pipeline.Lighting.DirectionalLight light = dirLights[index];
                if (!light.IsEnabled) continue;

                if (light.CastShadows)
                {
                    RenderingStats.shadowMaps += 1;

                    CreateShadowMapDirectionalLight(light, meshBatcher);

                    light.HasChanged = false;

                }

            }

            //Performance Profile
            this.Profiler?.SampleTimestamp(TimestampIndices.Draw_Shadows);
        }

        /// <summary>
        /// Create the shadow map for each cubemapside, then combine into one cubemap
        /// </summary>
        private void CreateShadowCubeMap(PointLight light, DynamicMeshBatcher meshBatcher)
        {
            int shadowResolution = light.ShadowResolution;
            //For VSM we need 2 channels, -> Vector2
            //todo: check if we need preserve contents
            if (light.ShadowMap == null)
                // ToDo: Create Rendertarget definition for shadow map 'cube' (most likely also for directional lights)
                light.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution * 6, false, SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            Matrix lightViewProjection = Matrix.Identity;
            CubeMapFace cubeMapFace;

            if (light.HasChanged)
            {
                _graphicsDevice.SetRenderTarget(light.ShadowMap);

                Matrix lightView = Matrix.Identity;


                _graphicsDevice.SetRenderTarget(light.ShadowMap);
                _graphicsDevice.Clear(Color.Black);

                for (int i = 0; i < 6; i++)
                {
                    cubeMapFace = (CubeMapFace)i;
                    light.GetLightViewMatrices(cubeMapFace, ref lightView, ref lightViewProjection);
                    // render the scene to all cubemap faces

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    meshBatcher.FrustumCulling(_boundingFrustumShadow, true);

                    // Rendering!
                    _fxSetup.Param_FarClip.SetValue(light.Radius);
                    _fxSetup.Param_LightPositionWS.SetValue(light.Position);

                    _graphicsDevice.Viewport = new Viewport(0, shadowResolution * (int)cubeMapFace, shadowResolution, shadowResolution);
                    _graphicsDevice.ScissorRectangle = new Rectangle(0, shadowResolution * (int)cubeMapFace, shadowResolution, shadowResolution);

                    //For shadowmaps we need to find out whether any object has moved and if so if it is rendered. If yes, redraw the whole frame, if no don't do anything

                    if (meshBatcher.CheckRequiresRedraw(RenderType.ShadowOmnidirectional, true, light.HasChanged))
                        meshBatcher.Draw(renderType: RenderType.ShadowOmnidirectional,
                            viewProjection: lightViewProjection,
                            view: null,
                            RenderContext.Default,
                            this);

                }
            }
            else
            {
                // set target to light shadow map
                _graphicsDevice.SetRenderTarget(light.ShadowMap);

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace)i;
                    lightViewProjection = light.GetViewProjection(cubeMapFace);

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    bool hasAnyObjectMoved = meshBatcher.FrustumCulling(_boundingFrustumShadow, false);

                    if (!hasAnyObjectMoved) continue;

                    _graphicsDevice.Viewport = new Viewport(0, shadowResolution * (int)cubeMapFace, shadowResolution, shadowResolution);
                    _graphicsDevice.ScissorRectangle = new Rectangle(0, shadowResolution * (int)cubeMapFace, shadowResolution, shadowResolution);

                    if (meshBatcher.CheckRequiresRedraw(RenderType.ShadowOmnidirectional, light.HasChanged, true))
                        meshBatcher.Draw(renderType: RenderType.ShadowOmnidirectional,
                        viewProjection: lightViewProjection,
                        view: null,
                        RenderContext.Default,
                        this);
                }
            }
        }

        /// <summary>
        /// Only one shadow map needed for a directional light
        /// </summary>
        private void CreateShadowMapDirectionalLight(DirectionalLight light,  DynamicMeshBatcher meshBatcher)
        {
            int shadowResolution = light.ShadowResolution;
            //Create a renderTarget if we don't have one yet
            if (light.ShadowMap == null)
            {
                //if (lightSource.ShadowFiltering != DirectionalLightSource.ShadowFilteringTypes.VSM)
                //{
                light.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                    SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
                //else //For a VSM shadowMap we need 2 components
                //{
                //    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                //       SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
            }

            if (light.HasChanged)
            {
                // update light view projection from itself (position, direction, size and far clip)
                light.UpdateViewProjection();
                if (_boundingFrustumShadow == null)
                    _boundingFrustumShadow = new BoundingFrustum(light.Matrices.ViewProjection);
                else
                    _boundingFrustumShadow.Matrix = light.Matrices.ViewProjection;

                _graphicsDevice.SetRenderTarget(light.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshBatcher.FrustumCulling(_boundingFrustumShadow, true);

                // Rendering!
                _fxSetup.Param_FarClip.SetValue(light.ShadowFarClip);
                _fxSetup.Param_SizeBias.SetValue(ShadowMapPipelineModule.ShadowBias * 2048 / light.ShadowResolution);


                if (meshBatcher.CheckRequiresRedraw(RenderType.ShadowOmnidirectional, true, light.HasChanged))
                    meshBatcher.Draw(RenderType.ShadowLinear, light.Matrices.ViewProjection, light.Matrices.View, RenderContext.Default, this);
            }
            else
            {
                _boundingFrustumShadow = new BoundingFrustum(light.Matrices.ViewProjection);

                bool hasAnyObjectMoved = meshBatcher.FrustumCulling(_boundingFrustumShadow, false);

                if (!hasAnyObjectMoved) return;

                meshBatcher.FrustumCulling(_boundingFrustumShadow, true);

                _graphicsDevice.SetRenderTarget(light.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                _fxSetup.Param_FarClip.SetValue(light.ShadowFarClip);
                _fxSetup.Param_SizeBias.SetValue(ShadowMapPipelineModule.ShadowBias * 2048 / light.ShadowResolution);

                if (meshBatcher.CheckRequiresRedraw(RenderType.ShadowLinear, false, true))
                    meshBatcher.Draw(RenderType.ShadowLinear, light.Matrices.ViewProjection, light.Matrices.View, RenderContext.Default, this);
            }

            //Blur!
            //if (lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM)
            //{
            //    lightSource.ShadowMap = _gaussianBlur.DrawGaussianBlur(lightSource.ShadowMap);
            //}

        }



        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            _fxSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            switch (_pass)
            {
                case ShadowPasses.Directional:
                    _fxSetup.Param_WorldView.SetValue(localWorldMatrix * (Matrix)view);
                    _fxSetup.Pass_LinearPass.Apply();
                    break;
                case ShadowPasses.Omnidirectional:
                    _fxSetup.Param_World.SetValue(localWorldMatrix);
                    _fxSetup.Pass_DistancePass.Apply();
                    break;
                case ShadowPasses.OmnidirectionalAlpha:
                    _fxSetup.Param_World.SetValue(localWorldMatrix);
                    _fxSetup.Pass_DistanceAlphaPass.Apply();
                    break;
            }
        }

        public void SetMaterialSettings(MaterialEffect material)
        {
            //Check if we have a mask texture
            if (material.HasMask)
            {
                _pass = ShadowPasses.OmnidirectionalAlpha;
                _fxSetup.Param_MaskTexture.SetValue(material.Mask);
            }
            else
            {
                _pass = ShadowPasses.Omnidirectional;
            }
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }
    }
}
