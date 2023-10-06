using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public class TransformMatrix
    {
        public bool HasChanged = true;
        public readonly int Id;

        public Vector3 Scale;
        public Matrix World;
        public Matrix InverseWorld;

        public TransformMatrix(Matrix world, int id)
        {
            World = world;
            Id = id;
        }

    }
}
