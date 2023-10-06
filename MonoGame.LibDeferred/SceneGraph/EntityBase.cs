using DeferredEngine.Recources.Helper;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public abstract class EntityBase : TransformableObject
    {
        public override Vector3 Position
        {
            get => _position;
            set
            {
                base.Position = value;
                WorldTransform.HasChanged = true;
            }
        }
        public override Matrix RotationMatrix
        {
            get => _rotationMatrix;
            set
            {
                base.RotationMatrix = value;
                WorldTransform.HasChanged = true;
            }
        }
        public override Vector3 Scale
        {
            get => _scale;
            set
            {
                base.Scale = value;
                WorldTransform.HasChanged = true;
            }
        }



        public readonly BoundingBox BoundingBox;
        public readonly Vector3 BoundingBoxOffset;
        public readonly TransformMatrix WorldTransform;


        public EntityBase(BoundingBox bBox, Vector3 bBoxOffset)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);
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
            WorldTransform = new TransformMatrix(Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position), Id);
            Position = position;
            RotationMatrix = rotation;
            Scale = scale;

            BoundingBox = new BoundingBox(-Vector3.One, Vector3.One);
            BoundingBoxOffset = Vector3.Zero;
        }

        protected override void UpdateMatrices()
        {
            base.UpdateMatrices();
            WorldTransform.Scale = _scale;
            WorldTransform.World = _world;
            WorldTransform.InverseWorld = _inverseWorld;
        }
    }
}
