using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Logic
{
    public class MainSceneLogic
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private DemoAssets _assets;

        public Camera Camera;


        //mesh library, holds all the meshes and their materials
        public DynamicMeshBatcher MeshBatcher;

        private EntitySceneGroup _scene;
        public readonly List<ModelEntity> BasicEntities = new List<ModelEntity>();
        public readonly List<Decal> Decals = new List<Decal>();
        public readonly List<PointLight> PointLights = new List<PointLight>();
        public readonly List<Pipeline.Lighting.DirectionalLight> DirectionalLights = new List<Pipeline.Lighting.DirectionalLight>();
        public EnvironmentProbe EnvProbe;

        //Which render target are we currently displaying?
        private int _renderModeCycle;


        public EntitySceneGroup Scene => _scene;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Done after Load
        public void Initialize(DemoAssets assets, GraphicsDevice graphicsDevice)
        {
            _assets = assets;

            MeshBatcher = new DynamicMeshBatcher(graphicsDevice);
            MeshBatcher.BatchByMaterial = false;

            SetUpEditorScene(graphicsDevice);

            _scene = new EntitySceneGroup(BasicEntities, DirectionalLights, PointLights, Decals, EnvProbe);
        }

        //Load our default setup!
        private void SetUpEditorScene(GraphicsDevice graphics)
        {
            ////////////////////////////////////////////////////////////////////////
            // Camera

            //Set up our starting camera position

            // NOTE: Coordinate system depends on Camera.up,
            //       Right now z is going up, it's not depth!

            Camera = new Camera(position: new Vector3(-88, -11f, 4), lookat: new Vector3(38, 8, 32));

            EnvProbe = new EnvironmentProbe(new Vector3(-45, -5, 5));

            ////////////////////////////////////////////////////////////////////////
            // Static geometry

            // NOTE: If you don't pass a materialEffect it will use the default material from the object

            AddEntity(model: _assets.SponzaModel,
                position: Vector3.Zero, angleX: Math.PI / 2, angleY: 0, angleZ: 0, scale: 0.1f);//CHANGE BACK


            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    AddEntity(model: _assets.Plane,
                        materialEffect: ((x + 5 + y + 5) % 2 == 1) ? _assets.MirrorMaterial : _assets.MetalRough03Material,
                        position: new Vector3(30 + x * 4, y * 4 + 4, 0), angleX: 0, angleY: 0, angleZ: 0, scale: 2);
                }
            }

            AddEntity(model: _assets.StanfordDragonLowpoly,
                materialEffect: _assets.BaseMaterial,
                position: new Vector3(40, -10, 0), angleX: Math.PI / 2, angleY: 0, angleZ: 0, scale: 10);

            ////////////////////////////////////////////////////////////////////////
            // Dynamic geometry

            // NOTE: We first have to create a physics object and then apply said object to a rendered model
            // BEPU could use non-default meshes, but that is much much more expensive so I am using just default ones right now
            // ... so -> spheres, boxes etc.
            // For dynamic meshes I could use the same way i have static meshes, but use - MobileMesh - instead

            // NOTE: Our physics entity's position will be overwritten, so it doesn't matter
            // NOTE: If a physics object has mass it will move, otherwise it is static

            AddEntity(model: StaticAssets.Instance.IsoSphere,
                materialEffect: _assets.AlphaBlendRim,
                position: new Vector3(20, 0, 10), angleX: Math.PI / 2, angleY: 0, angleZ: 0, scale: 5);


            for (int i = 0; i < 10; i++)
            {
                MaterialEffect test = new MaterialEffect(_assets.SilverMaterial);
                test.Roughness = i / 9.0f + 0.1f;
                test.Metallic = 1;
                AddEntity(model: StaticAssets.Instance.IsoSphere,
                    materialEffect: test,
                    position: new Vector3(30 + i * 10, 0, 10), angleX: Math.PI / 2, angleY: 0, angleZ: 0, scale: 5);
            }

            ////////////////////////////////////////////////////////////////////////
            // Decals

            Decals.Add(new Decal(StaticAssets.Instance.IconDecal, new Vector3(-6, 22, 15), new Vector3((float)(-Math.PI / 2), 0, 0), Vector3.One * 10));

            ////////////////////////////////////////////////////////////////////////
            // Dynamic lights

            AddPointLight(position: new Vector3(-61, 0, 107),
                        radius: 150,
                        color: new Color(104, 163, 223),
                        intensity: 20,
                        castShadows: false,
                        shadowResolution: 1024,
                        isVolumetric: true);

            AddPointLight(position: new Vector3(15, 0, 107),
                        radius: 150,
                        color: new Color(104, 163, 223),
                        intensity: 30,
                        castShadows: false,
                        shadowResolution: 1024,
                        isVolumetric: true);

            AddPointLight(position: new Vector3(66, 0, 40),
                        radius: 120,
                        color: new Color(255, 248, 232),
                        intensity: 120,
                        castShadows: true,
                        shadowResolution: 1024,
                        softShadowBlurAmount: 0,
                        isVolumetric: false);

            AddDirectionalLight(direction: new Vector3(0.2f, 0.2f, -1),
                        intensity: 100,
                        color: Color.White,
                        position: Vector3.UnitZ * 2,
                        drawShadows: true,
                        shadowWorldSize: 450,
                        shadowDepth: 180,
                        shadowResolution: 1024,
                        shadowFilteringFiltering: Pipeline.Lighting.DirectionalLight.ShadowFilteringTypes.SoftPCF3x);
        }



        /// <summary>
        /// Main logic update function. Is called once per frame. Use this for all program logic and user inputs
        /// </summary>
        /// <param name="gameTime">Can use this to compute the delta between frames</param>
        /// <param name="isActive">The window status. If this is not the active window we shouldn't do anything</param>
        public void Update(GameTime gameTime, bool isActive)
        {
            if (!isActive) return;

            //Upd
            Input.Update(gameTime, Camera);


            //If we are currently typing stuff into the console we should ignore the following keyboard inputs
            if (DebugScreen.ConsoleOpen) return;

            //Starts the "editor mode" where we can manipulate objects
            if (Input.WasKeyPressed(Keys.Space))
            {
                RenderingSettings.e_IsEditorEnabled = !RenderingSettings.e_IsEditorEnabled;
            }

            //Switch which rendertargets we show
            if (Input.WasKeyPressed(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > Enum.GetNames(typeof(PipelineOutputPasses)).Length - 1) _renderModeCycle = 0;

                RenderingSettings.g_CurrentPass = (PipelineOutputPasses)_renderModeCycle;
            }
        }


        //Load content
        public void Load(ContentManager content)
        {
            //...
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Spawn a directional light (omni light). This light covers everything and comes from a single point from infinte distance. 
        /// Good for something like a sun
        /// </summary>
        /// <param name="direction">The direction the light is facing in world space coordinates</param>
        /// <param name="intensity"></param>
        /// <param name="color"></param>
        /// <param name="position">The position is only relevant if drawing shadows</param>
        /// <param name="drawShadows"></param>
        /// <param name="shadowWorldSize">WorldSize is the width/height of the view projection for shadow mapping</param>
        /// <param name="shadowDepth">FarClip for shadow mapping</param>
        /// <param name="shadowResolution"></param>
        /// <param name="shadowFilteringFiltering"></param>
        /// <returns></returns>
        private Pipeline.Lighting.DirectionalLight AddDirectionalLight(Vector3 direction, int intensity, Color color, Vector3 position = default(Vector3),
            bool drawShadows = false, float shadowWorldSize = 100, float shadowDepth = 100, int shadowResolution = 512,
            Pipeline.Lighting.DirectionalLight.ShadowFilteringTypes shadowFilteringFiltering = Pipeline.Lighting.DirectionalLight.ShadowFilteringTypes.Poisson)
        {
            Pipeline.Lighting.DirectionalLight light = new Pipeline.Lighting.DirectionalLight(color: color,
                intensity: intensity,
                direction: direction,
                position: position,
                castShadows: drawShadows,
                shadowSize: shadowWorldSize,
                shadowFarClip: shadowDepth,
                shadowMapResolution: shadowResolution,
                shadowFiltering: shadowFilteringFiltering);
            DirectionalLights.Add(light);
            return light;
        }

        //The function to use for new pointlights
        /// <summary>
        /// Add a point light to the list of drawn point lights
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="castShadows">will render shadow maps</param>
        /// <param name="isVolumetric">does it have a fog volume?</param>
        /// <param name="volumetricDensity">How dense is the volume?</param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <returns></returns>
        private PointLight AddPointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, bool isVolumetric = false, float volumetricDensity = 1, int shadowResolution = 256, int softShadowBlurAmount = 0)
        {
            PointLight light = new PointLight(position, radius, color, intensity, castShadows, isVolumetric, shadowResolution, softShadowBlurAmount, volumetricDensity);
            PointLights.Add(light);
            return light;
        }


        /// <summary>
        /// Create a basic rendered model without custom material, use materialEffect: for materials instead
        /// The material used is the one found in the imported model file, usually that means diffuse texture only
        /// </summary>
        /// <param name="model"></param>
        /// <param name="position"></param>
        /// <param name="angleX"></param>
        /// <param name="angleY"></param>
        /// <param name="angleZ"></param>
        /// <param name="scale"></param>
        /// <returns>returns the basicEntity we created</returns>
        private ModelEntity AddEntity(ModelDefinition model, Vector3 position, double angleX, double angleY, double angleZ, float scale)
        {
            ModelEntity entity = new ModelEntity(model,
                null,
                position: position,
                eulerAngles: new Vector3((float)angleX, (float)angleY, (float)angleZ),
                scale: Vector3.One * scale,
                library: MeshBatcher);
            BasicEntities.Add(entity);

            return entity;
        }

        /// <summary>
        /// Create a basic rendered model with custom material
        /// </summary>
        /// <param name="model"></param>
        /// <param name="materialEffect">custom material</param>
        /// <param name="position"></param>
        /// <param name="angleX"></param>
        /// <param name="angleY"></param>
        /// <param name="angleZ"></param>
        /// <param name="scale"></param>
        /// <returns>returns the basicEntity we created</returns>
        private ModelEntity AddEntity(ModelDefinition model, MaterialEffect materialEffect, Vector3 position, double angleX, double angleY, double angleZ, float scale)
        {
            ModelEntity entity = new ModelEntity(model,
                materialEffect,
                position: position,
                eulerAngles: new Vector3((float)angleX, (float)angleY, (float)angleZ),
                scale: Vector3.One * scale,
                library: MeshBatcher);
            BasicEntities.Add(entity);

            return entity;
        }

    }
}
