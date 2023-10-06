using DeferredEngine.Entities;
using DeferredEngine.Logic;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.RenderModules
{
    public partial class IdAndOutlineRenderer
    {
        public struct GizmoDrawContext
        {
            public TransformableObject SelectedObject;
            public int SelectedObjectId;
            public Vector3 SelectedObjectPosition;
            public bool GizmoTransformationMode;
            public GizmoModes GizmoMode;
        }


    }
}
