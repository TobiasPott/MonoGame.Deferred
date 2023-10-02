using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public class TransformMatrix
    {
        public Matrix InverseWorld;
        public bool Rendered = true;
        public bool HasChanged = true;
        public readonly int Id;

        public Vector3 Scale;

        public Matrix World;

        public TransformMatrix(Matrix world, int id)
        {
            World = world;
            Id = id;
        }

        public Vector3 TransformMatrixSubModel(Vector3 translateSub)
        {
            return Vector3.Transform(translateSub, World);
        }
    }
}
