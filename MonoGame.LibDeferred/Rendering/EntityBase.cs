using DeferredEngine.Recources.Helper;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public abstract class EntityBase: TransformableObject
    {
        public readonly BoundingBox BoundingBox;
        public readonly Vector3 BoundingBoxOffset;

        protected int _id;

        protected Vector3 _position;
        public override Vector3 Position
        {
            get => _position;
            set
            {
                WorldTransform.HasChanged = true;
                _position = value;
            }
        }

        protected Vector3 _scale;
        public override Vector3 Scale
        {
            get => _scale;
            set
            {
                WorldTransform.HasChanged = true;
                _scale = value;
            }
        }

        public override int Id
        {
            get => _id;
            set => _id = value;
        }

        protected Matrix _rotationMatrix;
        public override Matrix RotationMatrix
        {
            get => _rotationMatrix;
            set
            {
                _rotationMatrix = value;
                WorldTransform.HasChanged = true;
            }
        }

        public override bool IsEnabled { get; set; }

        public override string Name { get; set; }


        public readonly TransformMatrix WorldTransform;
        protected Matrix _worldOldMatrix = Matrix.Identity;
        protected Matrix _worldNewMatrix = Matrix.Identity;


        public EntityBase(BoundingBox bBox, Vector3 bBoxOffset)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);

            BoundingBox = bBox;
            BoundingBoxOffset = bBoxOffset;
        }


    }
}
