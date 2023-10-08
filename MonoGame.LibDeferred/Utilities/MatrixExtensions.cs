using Microsoft.Xna.Framework;

namespace MonoGame.Ext
{
    public static class MatrixExtensions
    {
        public static Matrix CreateRotationXYZ(Vector3 angles)
            => Matrix.CreateRotationX(angles.X) * Matrix.CreateRotationY(angles.Y) * Matrix.CreateRotationZ(angles.Z);

        public static Matrix CreateRotationXYZ(float angleX, float angleY, float angleZ)
            => Matrix.CreateRotationX(angleX) * Matrix.CreateRotationY(angleY) * Matrix.CreateRotationZ(angleZ);

    }
}
