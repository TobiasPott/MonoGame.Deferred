using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Windows.UI.Composition;

namespace DeferredEngine.Renderer.RenderModules
{
    public class ShadowMapPipelineModule : PipelineModule, IRenderModule
    {
        private ShadowPasses _pass;

        private BoundingFrustum _boundingFrustumShadow;


        private enum ShadowPasses
        {
            Directional,
            Omnidirectional,
            OmnidirectionalAlpha
        };

        public ShadowMapPipelineModule(ContentManager content, string shaderPath = "Shaders/Shadow/ShadowMap")
            : base(content, shaderPath) { }

        //public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        //{
        //    base.Initialize(graphicsDevice, spriteBatch);
        //}

        protected override void Load(ContentManager content, string shaderPath = "Shaders/Shadow/ShadowMap")
        { }

        public void Draw(DynamicMeshBatcher meshMaterialLibrary, EntitySceneGroup scene)
        {
            List<DeferredPointLight> pointLights = scene.PointLights;
            List<DeferredDirectionalLight> dirLights = scene.DirectionalLights;

            _pass = ShadowPasses.Omnidirectional;

            //Go through all our point lights
            for (int index = 0; index < pointLights.Count; index++)
            {
                DeferredPointLight light = pointLights[index];

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
                    if (!light.StaticShadows || light.ShadowMap == null)
                    {
                        CreateShadowCubeMap(light, light.ShadowResolution, meshMaterialLibrary);

                        light.HasChanged = false;
                    }
                }
            }

            _pass = ShadowPasses.Directional;

            for (int index = 0; index < dirLights.Count; index++)
            {
                DeferredDirectionalLight light = dirLights[index];
                if (!light.IsEnabled) continue;

                if (light.CastShadows)
                {
                    RenderingStats.shadowMaps += 1;

                    CreateShadowMapDirectionalLight(light, light.ShadowResolution, meshMaterialLibrary);

                    light.HasChanged = false;

                }

            }
        }

        /// <summary>
        /// Create the shadow map for each cubemapside, then combine into one cubemap
        /// </summary>
        private void CreateShadowCubeMap(DeferredPointLight light, int size, DynamicMeshBatcher meshMaterialLibrary)
        {
            //For VSM we need 2 channels, -> Vector2
            //todo: check if we need preserve contents
            if (light.ShadowMap == null)
                light.ShadowMap = new RenderTarget2D(_graphicsDevice, size, size * 6, false, SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            Matrix lightViewProjection = new Matrix();
            CubeMapFace cubeMapFace; // = CubeMapFace.NegativeX;

            if (light.HasChanged)
            {
                _graphicsDevice.SetRenderTarget(light.ShadowMap);

                Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, light.Radius);
                Matrix lightView; // = identity

                //Reset the blur array
                light.faceBlurCount = new int[6];

                _graphicsDevice.SetRenderTarget(light.ShadowMap);
                _graphicsDevice.Clear(Color.Black);

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace)i;
                    switch (cubeMapFace)
                    {
                        case CubeMapFace.PositiveX:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.UnitX, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveX = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeX:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position - Vector3.UnitX, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeX = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveY:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.UnitY, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveY = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeY:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position - Vector3.UnitY, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeY = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveZ:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.UnitZ, Vector3.UnitX);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveZ = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeZ:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position - Vector3.UnitZ, Vector3.UnitX);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeZ = lightViewProjection;
                                break;
                            }
                    }

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    meshMaterialLibrary.FrustumCulling(_boundingFrustumShadow, true, light.Position);

                    // Rendering!

                    _graphicsDevice.Viewport = new Viewport(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);

                    //_graphicsDevice.ScissorRectangle = new Rectangle(0, light.ShadowResolution* (int) cubeMapFace,  light.ShadowResolution, light.ShadowResolution);
                    Shaders.ShadowMap.Param_FarClip.SetValue(light.Radius);
                    Shaders.ShadowMap.Param_LightPositionWS.SetValue(light.Position);

                    _graphicsDevice.ScissorRectangle = new Rectangle(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);

                    meshMaterialLibrary.Draw(renderType: DynamicMeshBatcher.RenderType.ShadowOmnidirectional,
                        viewProjection: lightViewProjection,
                        view: null,
                        lightViewPointChanged: true,
                        hasAnyObjectMoved: light.HasChanged,
                        renderModule: this);

                }
            }
            else
            {
                bool draw = false;

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace)i;
                    lightViewProjection = light.GetViewProjection(cubeMapFace);

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(_boundingFrustumShadow, false, light.Position);

                    if (!hasAnyObjectMoved) continue;

                    if (!draw)
                    {

                        _graphicsDevice.SetRenderTarget(light.ShadowMap);
                        draw = true;
                    }

                    _graphicsDevice.Viewport = new Viewport(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);

                    //_graphicsDevice.Clear(Color.TransparentBlack);
                    //_graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 0, 0);
                    _graphicsDevice.ScissorRectangle = new Rectangle(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);

                    meshMaterialLibrary.Draw(renderType: DynamicMeshBatcher.RenderType.ShadowOmnidirectional,
                        viewProjection: lightViewProjection,
                        view: null,
                        lightViewPointChanged: light.HasChanged,
                        hasAnyObjectMoved: true,
                        renderModule: this);
                }
            }
        }

        /// <summary>
        /// Only one shadow map needed for a directional light
        /// </summary>
        private void CreateShadowMapDirectionalLight(DeferredDirectionalLight light, int shadowResolution, DynamicMeshBatcher meshMaterialLibrary)
        {
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
                Matrix lightProjection = Matrix.CreateOrthographic(light.ShadowSize, light.ShadowSize, -light.ShadowFarClip, light.ShadowFarClip);
                Matrix lightView = Matrix.CreateLookAt(light.Position, light.Position + light.Direction, Vector3.Down);

                light.LightView = lightView;
                light.LightViewProjection = lightView * lightProjection;

                _boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);

                _graphicsDevice.SetRenderTarget(light.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshMaterialLibrary.FrustumCulling(_boundingFrustumShadow, true, light.Position);

                // Rendering!
                Shaders.ShadowMap.Param_FarClip.SetValue(light.ShadowFarClip);
                Shaders.ShadowMap.Param_SizeBias.SetValue(RenderingSettings.ShadowBias * 2048 / light.ShadowResolution);

                meshMaterialLibrary.Draw(DynamicMeshBatcher.RenderType.ShadowLinear,
                    light.LightViewProjection, light.LightView, light.HasChanged, false, false, 0, renderModule: this);
            }
            else
            {
                _boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);

                bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: false, cameraPosition: light.Position);

                if (!hasAnyObjectMoved) return;

                meshMaterialLibrary.FrustumCulling(boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: true, cameraPosition: light.Position);

                _graphicsDevice.SetRenderTarget(light.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);
                Shaders.ShadowMap.Param_FarClip.SetValue(light.ShadowFarClip);
                Shaders.ShadowMap.Param_SizeBias.SetValue(RenderingSettings.ShadowBias * 2048 / light.ShadowResolution);

                meshMaterialLibrary.Draw(DynamicMeshBatcher.RenderType.ShadowLinear,
                    light.LightViewProjection, light.LightView, false, true, false, 0, renderModule: this);
            }

            //Blur!
            //if (lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM)
            //{
            //    lightSource.ShadowMap = _gaussianBlur.DrawGaussianBlur(lightSource.ShadowMap);
            //}

        }



        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Shaders.ShadowMap.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            switch (_pass)
            {
                case ShadowPasses.Directional:
                    Shaders.ShadowMap.Param_WorldView.SetValue(localWorldMatrix * (Matrix)view);
                    Shaders.ShadowMap.Pass_LinearPass.Apply();
                    break;
                case ShadowPasses.Omnidirectional:
                    Shaders.ShadowMap.Param_World.SetValue(localWorldMatrix);
                    Shaders.ShadowMap.Pass_DistancePass.Apply();
                    break;
                case ShadowPasses.OmnidirectionalAlpha:
                    Shaders.ShadowMap.Param_World.SetValue(localWorldMatrix);
                    Shaders.ShadowMap.Pass_DistanceAlphaPass.Apply();
                    break;
            }
        }

        public void SetMaterialSettings(MaterialEffect material, DynamicMeshBatcher.RenderType renderType)
        {
            if (renderType == DynamicMeshBatcher.RenderType.ShadowOmnidirectional)
            {
                //Check if we have a mask texture
                if (material.HasMask)
                {
                    _pass = ShadowPasses.OmnidirectionalAlpha;
                    Shaders.ShadowMap.Param_MaskTexture.SetValue(material.Mask);

                }
                else
                {
                    _pass = ShadowPasses.Omnidirectional;
                }

            }
        }

        public override void Dispose()
        {

        }
    }
}


namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {

        // Shadow Map
        public static class ShadowMap
        {
            public static Effect Effect = Globals.content.Load<Effect>("Shaders/Shadow/ShadowMap");

            //Linear = VS Depth -> used for directional lights
            public static EffectPass Pass_LinearPass = Effect.Techniques["DrawLinearDepth"].Passes[0];
            //Distance = distance(pixel, light) -> used for omnidirectional lights
            public static EffectPass Pass_DistancePass = Effect.Techniques["DrawDistanceDepth"].Passes[0];
            public static EffectPass Pass_DistanceAlphaPass = Effect.Techniques["DrawDistanceDepthAlpha"].Passes[0];

            public static EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            public static EffectParameter Param_WorldView = Effect.Parameters["WorldView"];
            public static EffectParameter Param_World = Effect.Parameters["World"];
            public static EffectParameter Param_LightPositionWS = Effect.Parameters["LightPositionWS"];
            public static EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static EffectParameter Param_SizeBias = Effect.Parameters["SizeBias"];
            public static EffectParameter Param_MaskTexture = Effect.Parameters["MaskTexture"];
        }


    }
}