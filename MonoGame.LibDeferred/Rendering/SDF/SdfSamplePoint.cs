﻿using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.RenderModules.SDF
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