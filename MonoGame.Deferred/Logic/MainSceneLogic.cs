using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Demo
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
        private readonly EntityScene _scene = new EntityScene();

        //Which render target are we currently displaying?
        private int _renderModeCycle;


        public EntityScene Scene => _scene;

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

            MeshBatcher = new DynamicMeshBatcher(graphicsDevice)
            {
                BatchByMaterial = false
            };

            SetupSponzaSampleSscenne();
        }

        //Load our default setup!
        private void SetupSponzaSampleSscenne()
        {
            ////////////////////////////////////////////////////////////////////////
            // Camera
            //Set up our starting camera position
            // NOTE: Coordinate system depends on Camera.up,
            //       Right now z is going up, it's not depth!
            Camera = new Camera(position: new Vector3(-88, -11f, 4), lookat: new Vector3(38, 8, 32));

            ////////////////////////////////////////////////////////////////////////
            // Static geometry
            // NOTE: If you don't pass a materialEffect it will use the default material from the object
            AddEntity(_assets.SponzaModel, null,
                Vector3.Zero, new Vector3((float)Math.PI / 2, 0, 0), Vector3.One * 0.1f, MeshBatcher);//CHANGE BACK


            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    bool isMirror = ((x + 5 + y + 5) % 2 == 1);
                    AddEntity(_assets.Plane, isMirror ? _assets.MirrorMaterial : _assets.MetalRough03Material,
                        new Vector3(30 + x * 4, y * 4 + 4, 0), new Vector3(0, 0, 0), Vector3.One * 2, MeshBatcher);
                }
            }

            AddEntity(_assets.StanfordDragonLowpoly,
                _assets.BaseMaterial,
                new Vector3(40, -10, 0), new Vector3((float)Math.PI / 2, 0, 0), Vector3.One * 10, MeshBatcher);

            ////////////////////////////////////////////////////////////////////////
            // Dynamic geometry

            // NOTE: We first have to create a physics object and then apply said object to a rendered model
            // BEPU could use non-default meshes, but that is much much more expensive so I am using just default ones right now
            // ... so -> spheres, boxes etc.
            // For dynamic meshes I could use the same way i have static meshes, but use - MobileMesh - instead

            // NOTE: Our physics entity's position will be overwritten, so it doesn't matter
            // NOTE: If a physics object has mass it will move, otherwise it is static

            AddEntity(StaticAssets.Instance.IsoSphere,
                _assets.AlphaBlendRim,
                new Vector3(20, 0, 10), new Vector3((float)Math.PI / 2, 0, 0), Vector3.One * 5, MeshBatcher);


            for (int i = 0; i < 10; i++)
            {
                MaterialBase test = new MaterialBase(_assets.SilverMaterial)
                {
                    Roughness = i / 9.0f + 0.1f,
                    Metallic = 1
                };
                AddEntity(StaticAssets.Instance.IsoSphere,
                    test,
                    new Vector3(30 + i * 10, 0, 10), new Vector3((float)(Math.PI / 2.0f), 0, 0), Vector3.One * 5, MeshBatcher);
            }

            ////////////////////////////////////////////////////////////////////////
            // Decals

            _scene.Decals.Add(new Decal(StaticAssets.Instance.IconDecal, new Vector3(-6, 22, 15), new Vector3((float)(-Math.PI / 2), 0, 0), Vector3.One * 10));

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
                if (_renderModeCycle > Enum.GetNames(typeof(DeferredRenderingPasses)).Length - 1) _renderModeCycle = 0;

                RenderingSettings.g_CurrentPass = (DeferredRenderingPasses)_renderModeCycle;
            }
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
        private Pipeline.Lighting.DirectionalLight AddDirectionalLight(Vector3 direction, int intensity, Color color, Vector3 position = default,
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
            _scene.DirectionalLights.Add(light);
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
            _scene.PointLights.Add(light);
            return light;
        }


        /// <summary>
        /// Create a basic rendered model with custom material
        /// </summary>
        /// <returns>returns the basicEntity we created</returns>
        private ModelEntity AddEntity(ModelDefinition model, MaterialBase materialEffect,
            Vector3 position, Vector3 angles, Vector3 scale, DynamicMeshBatcher batcher)
        {
            return _scene.Add(model, materialEffect, position, angles, scale, batcher);
        }

    }
}
