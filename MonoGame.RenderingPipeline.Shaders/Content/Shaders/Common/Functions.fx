#ifndef __HLSL_FUNCTIONS__
#define __HLSL_FUNCTIONS__

#include "FrustumCorners.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VSInput_Encoded
{
    float2 Position : SV_POSITION;
};

struct VSOutputPosTex
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutputPosTexViewDir
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 ViewDir : TEXCOORD1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS
float4 VSPassthrough_F2ToF4(float2 Position : POSITION0) : SV_POSITION
{
    return float4(Position, 1, 1);
}


VSOutputPosTex VSMain_Encoded(VSInput_Encoded input, uint id : SV_VERTEXID)
{
    VSOutputPosTex output;
    output.Position = float4(input.Position, 0, 1);
    output.TexCoord.x = (float) (id / 2) * 2.0;
    output.TexCoord.y = 1.0 - (float) (id % 2) * 2.0;

    return output;
}

VSOutputPosTexViewDir VSMain_EncodedViewDir(VSInput_Encoded input, uint id : SV_VERTEXID)
{
    VSOutputPosTexViewDir output;
    output.Position = float4(input.Position, 0, 1);
    output.TexCoord.x = (float) (id / 2) * 2.0;
    output.TexCoord.y = 1.0 - (float) (id % 2) * 2.0;

    output.ViewDir = GetFrustumRay(id);
    return output;
}

#endif // __HLSL_FUNCTIONS__