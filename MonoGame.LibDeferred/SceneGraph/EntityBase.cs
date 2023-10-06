using DeferredEngine.Recources.Helper;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public abstract class EntityBase : TransformableObject
    {

        public readonly BoundingBox BoundingBox;
        public readonly Vector3 BoundingBoxOffset;

        public EntityBase(BoundingBox bBox, Vector3 bBoxOffset)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            Position = Vector3.Zero;
            RotationMatrix = Matrix.Identity;
            Scale = Vector3.One;

            BoundingBox = bBox;
            BoundingBoxOffset = bBoxOffset;
        }
        public EntityBase(Vector3 position, Matrix rotation, Vector3 scale)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            Position = position;
            RotationMatrix = rotation;
            Scale = scale;

            BoundingBox = new BoundingBox(-Vector3.One, Vector3.One);
            BoundingBoxOffset = Vector3.Zero;
        }

    }
}
