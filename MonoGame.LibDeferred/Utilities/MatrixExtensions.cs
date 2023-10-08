using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace MonoGame.Ext
{
    public static class MatrixExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix ToMatrixTranslationXY(this Vector2 position2D)
            => Matrix.CreateTranslation(position2D.X, position2D.Y, 0);
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix ToMatrixTranslationXZ(this Vector2 position2D)
            => Matrix.CreateTranslation(position2D.X, 0, position2D.Y);
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix ToMatrixTranslationYZ(this Vector2 position2D)
            => Matrix.CreateTranslation(0, position2D.X, position2D.Y);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix ToMatrixRotationXYZ(this Vector3 angles)
            => Matrix.CreateRotationX(angles.X) * Matrix.CreateRotationY(angles.Y) * Matrix.CreateRotationZ(angles.Z);


    }
}
