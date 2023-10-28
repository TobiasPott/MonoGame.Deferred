using Microsoft.Xna.Framework;
using MonoGame.Ext;

namespace DeferredEngine.Rendering
{
    public class BoundingFrustumWithVertices
    {
        //Used for the view space directions in our shaders. Far edges of our view frustum
        public readonly BoundingFrustum Frustum = new BoundingFrustum(Matrix.Identity);
        public readonly Vector3[] WorldSpace = new Vector3[8];
        public readonly Vector3[] ViewSpace = new Vector3[8];
        public readonly Vector3[] WorldSpaceFrustum = new Vector3[4];
        public readonly Vector3[] ViewSpaceFrustum = new Vector3[4];
        public float FarClip = 500;

        public void UpdateVertices(Matrix? view, Vector3? worldOffset = null)
        {
            Frustum.GetCorners(WorldSpace);
            if (view.HasValue)
                WorldSpace.Transform(view.Value, ViewSpace); //put the frustum into view space
            if (worldOffset.HasValue)
                /*this part is used for volume projection*/
                //World Space Corners - Camera Position
                for (int i = 0; i < 4; i++) //take only the 4 farthest points
                {
                    WorldSpaceFrustum[i] = WorldSpace[i + 4] - worldOffset.Value;
                    ViewSpaceFrustum[i] = ViewSpace[i + 4];
                }

            SwapCorners();
        }
        private void SwapCorners()
        {
            // swap 2 <-> 3
            (WorldSpaceFrustum[2], WorldSpaceFrustum[3]) = (WorldSpaceFrustum[3], WorldSpaceFrustum[2]);
            (ViewSpaceFrustum[2], ViewSpaceFrustum[3]) = (ViewSpaceFrustum[3], ViewSpaceFrustum[2]);
        }
    }

}

