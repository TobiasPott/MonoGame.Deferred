using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class StaticAssets : IDisposable
    {
        #region Singleton & Static Load/Unload
        public static StaticAssets Instance { get; private set; }
        public static void InitClass(ContentManager content, GraphicsDevice graphicsDevice)
        {
            if (Instance == null)
                Instance = new StaticAssets(content, graphicsDevice);
        }
        public static void UnloadClass() => Instance?.Dispose();
        
        #endregion

        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;



        public Texture2D IconLight { get; protected set; }
        public Texture2D IconEnvmap { get; protected set; }
        public Texture2D IconDecal { get; protected set; }

        public Model EditorArrow3D { get; protected set; }
        public Model EditorArrow3DRound { get; protected set; }


        public Model Sphere { get; protected set; }
        public ModelMeshPart SphereMeshPart { get; protected set; }
        public ModelDefinition IsoSphere { get; protected set; }


        public Texture2D NoiseMap { get; protected set; }


        private StaticAssets(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _content = content;

            // Icons and UI Textures
            IconDecal = content.Load<Texture2D>("Art/Editor/icon_decal");
            IconLight = content.Load<Texture2D>("Art/Editor/icon_light");
            IconEnvmap = content.Load<Texture2D>("Art/Editor/icon_envmap");

            // Models and Meshes
            EditorArrow3D = content.Load<Model>("Art/Editor/Arrow");
            EditorArrow3DRound = content.Load<Model>("Art/Editor/ArrowRound");

            IsoSphere = new SdfModelDefinition(content, "Art/default/isosphere", graphicsDevice, true);
            Sphere = content.Load<Model>("Art/default/sphere");
            SphereMeshPart = Sphere.Meshes[0].MeshParts[0];
            // Textures and Maps
            NoiseMap = content.Load<Texture2D>("Shaders/noise_blur");
        }

        public void Dispose()
        {
            IconLight?.Dispose();
            IconEnvmap?.Dispose();
            IconDecal?.Dispose();
        }


    }

}
