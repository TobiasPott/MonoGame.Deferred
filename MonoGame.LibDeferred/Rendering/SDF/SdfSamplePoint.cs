using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.RenderModules.Signed_Distance_Fields.SDF_Generator
{
    public struct SdfSamplePoint
    {
        public Vector3 p;
        public float sdf;

        public SdfSamplePoint(Vector3 position, float sdf)
        {
            p = position;
            this.sdf = sdf;
        }
    }
}
