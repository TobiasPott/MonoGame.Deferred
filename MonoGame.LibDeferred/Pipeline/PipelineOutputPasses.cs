
namespace DeferredEngine.Pipeline
{
    //Render modes
    public enum PipelineOutputPasses
    {
        Deferred = 0,
        Albedo,
        Normal,
        Depth,
        Diffuse,
        Specular,
        Volumetric,
        SSAO,
        SSBlur,
        SSR,
        // [Obsolete("HDR buffer is no longer directly available.")]
        // HDR,
        Final,
    }
}
