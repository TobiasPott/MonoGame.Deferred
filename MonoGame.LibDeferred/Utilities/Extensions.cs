﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace MonoGame.Ext
{
    public static class Extensions
    {
        public static void Transform(this Vector3[] sourceArray, Matrix matrix, Vector3[] destinationArray)
        {
            Vector3.Transform(sourceArray, ref matrix, destinationArray);
        }



        public static void SetValue(this EffectParameter param, int[] value)
        {
            for (var i = 0; i < value.Length; i++)
                param.Elements[i].SetValue(value[i]);
        }

        public static Vector3 Xyz(this Vector4 vec3)
        {
            return new Vector3(vec3.X, vec3.Y, vec3.Z);
        }

        public static Vector2 Xy(this Vector4 vec3)
        {
            return new Vector2(vec3.X, vec3.Y);
        }

        public static Vector3 Xyz(this HalfVector4 vec3)
        {
            return vec3.ToVector4().Xyz();
        }

        public static Vector3 Pow(this Vector3 vec3, float power)
        {
            return new Vector3((float)Math.Pow(vec3.X, power), (float)Math.Pow(vec3.Y, power), (float)Math.Pow(vec3.Z, power));
        }

        public static float Dot2(this Vector3 v)
        {
            return Vector3.Dot(v, v);
        }

    }
}
