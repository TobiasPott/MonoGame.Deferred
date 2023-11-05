#ifndef __HLSL_MACROS__
#define __HLSL_MACROS__


#define PREFIX_VS VertexStage_
#define PREFIX_PS PixelStage_

// Use vs/ps shader model 4 or higher for DX graphics backend
#define COMPILETARGET_VS vs_4_0
#define COMPILETARGET_PS ps_4_0

#define COMPILE_VS(FunctionName) VertexShader = compile vs_4_0 FunctionName()
#define COMPILE_PS(FunctionName) PixelShader = compile ps_4_0 FunctionName()

#endif // __HLSL_MACROS__