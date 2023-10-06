using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Entities
{
    public class Decal : TransformableObject
    {
        public override Vector3 Position
        {
            get { return _position; }
            set { _position = value;
                UpdateWorldMatrix();
            }
        }

        public sealed override Matrix RotationMatrix
        {
            get { return _rotationMatrix; }
            set { _rotationMatrix = value;
                UpdateWorldMatrix();
            }
        }

        public override Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value; 
                UpdateWorldMatrix();
            }
        }

        public Matrix InverseWorld;
        public Texture2D Texture;

        public Decal(Texture2D texture, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale) : 
            this(texture, position, Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                  Matrix.CreateRotationZ((float)angleZ), scale)
        { }

        public Decal(Texture2D texture, Vector3 position, Matrix rotationMatrix, Vector3 scale)
        {
            Texture = texture;
            Position = position;
            RotationMatrix = rotationMatrix;

            _scale = scale;
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            UpdateWorldMatrix();
        }

        public void UpdateWorldMatrix()
        {
            _world = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            InverseWorld = Matrix.Invert(World);
        }
    }
}
