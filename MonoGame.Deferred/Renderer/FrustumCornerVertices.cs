using Microsoft.Xna.Framework;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public class FrustumCornerVertices
    {
        //Used for the view space directions in our shaders. Far edges of our view frustum
        public readonly Vector3[] WorldSpace = new Vector3[8];
        public readonly Vector3[] ViewSpace = new Vector3[8];
        public readonly Vector3[] WorldSpaceFrustum = new Vector3[4];
        public readonly Vector3[] ViewSpaceFrustum = new Vector3[4];
    }

}

