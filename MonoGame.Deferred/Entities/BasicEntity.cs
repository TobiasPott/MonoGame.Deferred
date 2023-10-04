using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{

    public sealed class BasicEntity : EntityBase
    {
        //Avoid nesting, but i could also just provide the ModelDefinition instead
        public readonly ModelDefinition ModelDefinition;
        public readonly MaterialEffect Material;

        public override TransformableObject Clone
        {
            get
            {
                //Not very clean...
                return new BasicEntity(ModelDefinition, Material, Position, RotationMatrix, Scale);
            }
        }

        public BasicEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale, MeshMaterialLibrary library = null)
            : base(modelbb.BoundingBox, modelbb.BoundingBoxOffset)
        {
            ModelDefinition = modelbb;

            Material = material;
            Position = position;
            Scale = scale;

            RotationMatrix = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) * Matrix.CreateRotationZ((float)angleZ);

            if (library != null)
                RegisterInLibrary(library);

            WorldTransform.World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            WorldTransform.Scale = Scale;
            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
        }

        public BasicEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, Matrix rotationMatrix, Vector3 scale)
            : base(modelbb.BoundingBox, modelbb.BoundingBoxOffset)
        {
            ModelDefinition = modelbb;

            Material = material;
            Position = position;
            RotationMatrix = rotationMatrix;
            Scale = scale;
            RotationMatrix = rotationMatrix;

            WorldTransform.World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            WorldTransform.Scale = Scale;
            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
        }

        public void RegisterInLibrary(MeshMaterialLibrary library)
        {
            library.Register(Material, ModelDefinition.Model, WorldTransform);
        }

        public void Dispose(MeshMaterialLibrary library)
        {
            library.DeleteFromRegistry(this);
        }

        public override void ApplyTransformation()
        {
            //Has something changed?
            WorldTransform.Scale = Scale;
            Matrix scaleMatrix = Matrix.CreateScale(Scale);
            _worldOldMatrix = scaleMatrix * RotationMatrix * Matrix.CreateTranslation(Position);

            WorldTransform.Scale = Scale;
            WorldTransform.World = _worldOldMatrix;

            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
        
        }

    }
}
