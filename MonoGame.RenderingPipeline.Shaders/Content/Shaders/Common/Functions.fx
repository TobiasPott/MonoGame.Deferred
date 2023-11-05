#ifndef __HLSL_FUNCTIONS__
#define __HLSL_FUNCTIONS__

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VSInput_Encoded
{
    float2 Position : POSITION0;
};

struct VSOutputPosTex
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

VSOutputPosTex VSMain_Encoded(VSInput_Encoded input, uint id : SV_VERTEXID)
{
    VSOutputPosTex output;
    output.Position = float4(input.Position, 0, 1);
    output.TexCoord.x = (float) (id / 2) * 2.0;
    output.TexCoord.y = 1.0 - (float) (id % 2) * 2.0;

    return output;
}

#endif // __HLSL_FUNCTIONS__