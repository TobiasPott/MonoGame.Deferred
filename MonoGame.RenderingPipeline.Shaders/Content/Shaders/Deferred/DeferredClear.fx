
#include "../../Includes/Macros.incl.fx"
#include "../../Includes/VertexStage.incl.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS
//------------------------ PIXEL SHADER ----------------------------------------

float4 PSClear() : SV_TARGET0
{
	return float4(0,0,0,0);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
technique Base
{
	pass Pass1
	{

        COMPILE_VS(VSPassthrough_F2ToF4);
        COMPILE_PS(PSClear);
    }
}