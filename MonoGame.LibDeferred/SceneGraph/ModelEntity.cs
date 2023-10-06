using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{

    public sealed class ModelEntity : EntityBase
    {
        public readonly ModelDefinition ModelDefinition;
        public readonly MaterialEffect Material;

        public ModelEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale, MeshMaterialLibrary library = null)
            : this(modelbb, material, position, new Vector3((float)angleX, (float)angleY, (float)angleZ), scale, library)
        { }
        public ModelEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, Vector3 rotationAngles, Vector3 scale, MeshMaterialLibrary library = null)
            : base(modelbb.BoundingBox, modelbb.BoundingBoxOffset)
        {
            ModelDefinition = modelbb;

            Material = material;

            Position = position;
            RotationMatrix = Extensions.CreateRotationXYZ(rotationAngles);
            Scale = scale;

            if (library != null)
                RegisterInLibrary(library);

            this.UpdateMatrices();
        }

        public void RegisterInLibrary(MeshMaterialLibrary library)
        {
            library.Register(Material, ModelDefinition.Model, WorldTransform);
        }

        public void Dispose(MeshMaterialLibrary library)
        {
            library.DeleteFromRegistry(this);
        }

    }
}
