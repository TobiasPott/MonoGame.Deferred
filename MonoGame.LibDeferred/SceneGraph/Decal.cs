using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Entities
{
    public class Decal : TransformableObject
    {
        public Texture2D Texture;

        public Decal(Texture2D texture, Vector3 position, Vector3 angles, Vector3 scale) :
            this(texture, position, MatrixExtensions.CreateRotationXYZ(angles), scale)
        { }
        public Decal(Texture2D texture, Vector3 position, Matrix rotationMatrix, Vector3 scale)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            Texture = texture;
            Position = position;
            RotationMatrix = rotationMatrix;
            Scale = scale;
        }

    }
}
