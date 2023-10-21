using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using MonoGame.Ext;

namespace DeferredEngine.Entities
{
    public abstract class EntityBase : TransformableObject
    {

        public EntityBase()
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            Position = Vector3.Zero;
            RotationMatrix = Matrix.Identity;
            Scale = Vector3.One;
        }
        public EntityBase(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            Position = position;
            RotationMatrix = eulerAngles.ToMatrixRotationXYZ();
            Scale = scale;
        }




    }
}
