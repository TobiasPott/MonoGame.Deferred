﻿using Deferred.Utilities;
using DeferredEngine.Entities;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Recources
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
