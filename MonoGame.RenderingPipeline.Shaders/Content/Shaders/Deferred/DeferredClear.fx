
#include "../Common/Macros.fx"
#include "../Common/Functions.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

//------------------------ PIXEL SHADER ----------------------------------------

float4 BasePixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
	return float4(0,0,0,0);
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES

technique Base
{
	pass Pass1
	{

        VertexShader = compile COMPILETARGET_VS VSPassthrough_F2ToF4();
		PixelShader = compile COMPILETARGET_PS BasePixelShaderFunction();
	}
}