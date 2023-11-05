#ifndef __HLSL_VERTEXSTAGE__
#define __HLSL_VERTEXSTAGE__

#include "../../Includes/Frustum.incl.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VSOut_PosTex
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct VSOut_PosTexViewDir
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 ViewDir : TEXCOORD1;
};

struct VSIn_PosTex_ClipSpace
{
    float2 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};



////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS
float4 VSPassthrough_F2ToF4(float2 Position : SV_POSITION) : SV_POSITION
{
    return float4(Position, 1, 1);
}


VSOut_PosTex VSMain_Encoded(VSIn_PosTex_ClipSpace input)
{
    VSOut_PosTex output;
    output.Position = float4(input.Position, 1, 1);
    output.TexCoord = input.TexCoord;
    return output;
}

VSOut_PosTexViewDir VSMain_EncodedViewDir(VSIn_PosTex_ClipSpace input)
{
    VSOut_PosTexViewDir output;
    output.Position = float4(input.Position, 1, 1);
    output.TexCoord = input.TexCoord;
    output.ViewDir = GetFrustumRay(input.TexCoord);
    return output;
}

#endif // __HLSL_VERTEXSTAGE__