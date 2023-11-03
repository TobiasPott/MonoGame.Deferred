using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{

    public sealed class ModelEntity : EntityBase
    {
        public readonly ModelDefinition ModelDefinition;
        public readonly MaterialBase Material;

        public readonly BoundingBox BoundingBox;
        public readonly Vector3 BoundingBoxOffset;


        public ModelEntity(ModelDefinition modelbb, MaterialBase material, Vector3 position, Vector3 eulerAngles, Vector3 scale, DynamicMeshBatcher batcher = null)
            : base(position, eulerAngles, scale)
        {
            BoundingBox = modelbb.BoundingBox;
            BoundingBoxOffset = modelbb.BoundingBoxOffset;

            ModelDefinition = modelbb;

            Material = material;

            batcher?.Register(Material, ModelDefinition.Model, this);
            this.UpdateMatrices();
        }

        protected override void UpdateMatrices()
        {
            _world = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            _inverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
            _worldHasChanged = false;
        }

    }
}
