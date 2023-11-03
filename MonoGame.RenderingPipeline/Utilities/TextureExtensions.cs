using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace MonoGame.Ext
{
    public static class TextureExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetAspect(this Texture2D texture) => (float)texture.Width / (float)texture.Height;


    }
}
