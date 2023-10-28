using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Recources
{
    public struct ObjectHoverContext
    {
        public int HoveredId;
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;

        public ObjectHoverContext(int id, PipelineMatrices matrices)
        {
            HoveredId = id;
            ViewMatrix = matrices.View;
            ProjectionMatrix = matrices.Projection;
        }
    }

}
