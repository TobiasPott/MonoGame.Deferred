
#include "../../Includes/Macros.incl.fx"
#include "../../Includes/VertexStage.incl.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Draw colored lines
//  Either defined by vertex color or by GlobalColor

matrix WorldViewProj;
float4 StaticGlobalColor;

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

V2F_Color VertexShaderFunction(VSIn_PosColor input)
{
    V2F_Color output;
                 
    output.Position = mul(input.Position, WorldViewProj);

    output.Color = input.Color;
    return output;
}

float4 VertexShaderFunctionColor(float4 Position : SV_Position) : SV_Position
{
    float4 outPosition = mul(Position, WorldViewProj);

    return outPosition;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 PSPassthrough_VC(VSIn_PosColor input) : SV_TARGET0
{
    return input.Color; //+ AmbientColor * AmbientIntensity;
}


float4 PSConst_GlobalColor(float4 SV_POSITION : SV_Position) : SV_TARGET0
{
    return StaticGlobalColor;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique VertexColor
{
    pass Pass1
    {
        COMPILE_VS(VertexShaderFunction);
        COMPILE_PS(PSPassthrough_VC);
    }
}

technique GlobalColor
{
    pass Pass1
    {
        COMPILE_VS(VertexShaderFunctionColor);
        COMPILE_PS(PSConst_GlobalColor);
    }
}
