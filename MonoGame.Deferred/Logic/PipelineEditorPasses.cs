using System;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Rendering
{
    [Flags()]
    public enum PipelineEditorPasses
    {
        Billboard = 1,
        IdAndOutline = 2,
        Helper = 4,
        SDFDistance = 8,
        SDFVolume = 16,
    }

}

