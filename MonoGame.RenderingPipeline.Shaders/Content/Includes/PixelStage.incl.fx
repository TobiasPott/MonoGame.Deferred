#ifndef __HLSL_PIXELSTAGE__
#define __HLSL_PIXELSTAGE__

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct PSOut_AlbedoNormalDepth
{
    float4 Color : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Depth : SV_Target2;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

#endif // __HLSL_PIXELSTAGE__