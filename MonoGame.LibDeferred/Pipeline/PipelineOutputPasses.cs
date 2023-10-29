
namespace DeferredEngine.Pipeline
{
    //Render modes
    public enum PipelineOutputPasses
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
        //SDFDistance,
        //SDFVolume
    }
}
