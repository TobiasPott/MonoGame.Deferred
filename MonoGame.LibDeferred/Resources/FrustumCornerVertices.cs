using Microsoft.Xna.Framework;
using MonoGame.Ext;

namespace DeferredEngine.Recources
{
    public class FrustumCornerVertices
    {
        //Used for the view space directions in our shaders. Far edges of our view frustum
        public readonly Vector3[] WorldSpace = new Vector3[8];
        public readonly Vector3[] ViewSpace = new Vector3[8];
        public readonly Vector3[] WorldSpaceFrustum = new Vector3[4];
        public readonly Vector3[] ViewSpaceFrustum = new Vector3[4];


        public void FromFrustum(BoundingFrustum frustum, Matrix? view)
        {
            frustum.GetCorners(WorldSpace);
            if(view.HasValue)
                WorldSpace.Transform(view.Value, ViewSpace); //put the frustum into view space
        }
        public void UpdateFrustumCorners(Vector3 worldOffset)
        {
            /*this part is used for volume projection*/
            //World Space Corners - Camera Position
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                WorldSpaceFrustum[i] = WorldSpace[i + 4] - worldOffset;
                ViewSpaceFrustum[i] = ViewSpace[i + 4];
            }
        }
        public void SwapCorners()
        {
            // swap 2 <-> 3
            (WorldSpaceFrustum[2], WorldSpaceFrustum[3]) = (WorldSpaceFrustum[3], WorldSpaceFrustum[2]);
            (ViewSpaceFrustum[2], ViewSpaceFrustum[3]) = (ViewSpaceFrustum[3], ViewSpaceFrustum[2]);
        }
    }

}

