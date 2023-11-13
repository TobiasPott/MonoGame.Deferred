
#include "../../Includes/Macros.incl.fx"
#define _DEPTH_MAP
#include "../../Includes/Maps.incl.fx"
#include "../../Includes/VertexStage.incl.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Billboard Shader
//  Draws camera-oriented quads.

// Camera parameters.
float4x4 WorldViewProj;
float4x4 WorldView;

// For mouse picking we draw the object's id as color to the screen
float3 IdColor;

//Will be overwritten
// ToDo: Effect Parameter is null in C# part O.o

// Particle texture and sampler.
Texture2D Texture;
SamplerTex(Texture, Texture, CLAMP, LINEAR);

float2 ResolutionRcp = float2(1.0f / 1280, 1.0f / 800);

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VERTEX SHADER
////////////////////////////////////////////////////////////////////////////////////////////////////////////

V2F_TexCoordColor VSMain(VSIn_PosTexColor input)
{
    V2F_TexCoordColor output;

    output.Position = mul(input.Position, WorldViewProj);
    output.Position /= output.Position.w;

	float4 PositionVS = mul(input.Position, WorldView);

	//Get the screen coordinates of the billboards position
    float2 texCoord = 0.5f * (float2(output.Position.x, -output.Position.y) + 1);
        
    output.TextureCoordinate = float2(input.TexCoord.x, 1-input.TexCoord.y);

    output.Color = input.Color;

    return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  PIXEL SHADER
////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 PSMain_Sprite(V2F_TexCoordColor input) : SV_TARGET0
{
    float4 color = Texture.SampleLevel(TextureSampler, input.TextureCoordinate, 0);

    if (color.a < 0.95f)
        clip(-1);

    return float4(color.rgb * input.Color.rgb * IdColor, 1);
}

float4 PSMain_Id(V2F_TexCoordColor input) : SV_TARGET0
{
    float4 color = Texture.SampleLevel(TextureSampler, input.TextureCoordinate, 0);

    if (color.a < 0.95f)
        clip(-1);

    return float4(IdColor, 1);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES

technique Billboard
{
    pass Pass1
    {
        COMPILE_VS(VSMain);
        COMPILE_PS(PSMain_Sprite);
    }
}

technique Id
{
    pass Pass1
    {
        COMPILE_VS(VSMain);
        COMPILE_PS(PSMain_Id);
    }
}