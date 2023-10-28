﻿
namespace DeferredEngine.Pipeline
{
    //Render modes
    public enum PipelinePasses
    {
        Deferred,
        Albedo,
        Normal,
        Depth,
        Diffuse,
        Specular,
        Volumetric,
        SSAO,
        SSBlur,
        SSR,
        HDR,
    }
}
