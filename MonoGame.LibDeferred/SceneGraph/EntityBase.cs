using DeferredEngine.Recources.Helper;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public abstract class EntityBase: TransformableObject
    {

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



        public readonly BoundingBox BoundingBox;
        public readonly Vector3 BoundingBoxOffset;
        public readonly TransformMatrix WorldTransform;


        public EntityBase(BoundingBox bBox, Vector3 bBoxOffset)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            _position = Vector3.Zero;
            _rotationMatrix = Matrix.Identity; 
            _scale = Vector3.One;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);

            BoundingBox = bBox;
            BoundingBoxOffset = bBoxOffset;
        }
        public EntityBase(Vector3 position, Matrix rotation, Vector3 scale)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            _position = position;
            _rotationMatrix = rotation;
            _scale = scale;
            WorldTransform = new TransformMatrix(Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position), Id);

            BoundingBox = new BoundingBox(-Vector3.One, Vector3.One);
            BoundingBoxOffset = Vector3.Zero;
        }


        public abstract void ApplyTransformation();

    }
}
