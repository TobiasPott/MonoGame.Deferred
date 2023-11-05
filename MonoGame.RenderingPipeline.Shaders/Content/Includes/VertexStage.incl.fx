#ifndef __HLSL_VERTEXSTAGE__
#define __HLSL_VERTEXSTAGE__

#include "Frustum.incl.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS
//  INPUT
struct VSIn_Pos
{
    float4 Position : SV_POSITION;
};
struct VSIn_PosNml
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
};
struct VSIn_PosTex
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};
struct VSIn_PosColor
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};
struct VSIn_PosTexColor
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};



struct VSIn_PosTex_ClipSpace
{
    float2 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

//  OUTPUT
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



struct V2F_Color
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};
struct V2F_TexCoordColor
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color : COLOR0;
};
struct V2F_ViewPos
{
    float4 Position : SV_POSITION;
    float4 PositionVS : TEXCOORD0;
};
struct V2F_ViewPosColor
{
    float4 Position : SV_POSITION;
    float4 PositionVS : TEXCOORD0;
    float4 Color : COLOR0;
};
struct V2F_ViewPosScreenPos
{
    float4 Position : SV_POSITION;
    float4 PositionVS : TEXCOORD0;
    float4 ScreenPosition : TEXCOORD1;
};
struct V2F_NmlWorldPos
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float3 PositionWS : TEXCOORD0;
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