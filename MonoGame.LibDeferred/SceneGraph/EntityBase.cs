using DeferredEngine.Recources.Helper;
using MonoGame.Ext;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

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
