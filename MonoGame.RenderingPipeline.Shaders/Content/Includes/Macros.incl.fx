#ifndef __HLSL_MACROS__
#define __HLSL_MACROS__


#define PREFIX_VS VertexStage_
#define PREFIX_PS PixelStage_

#define VS_Target vs_4_0
#define PS_Target ps_4_0

// Use vs/ps shader model 4 or higher for DX graphics backend
#define COMPILE_VS(FunctionName) VertexShader = compile VS_Target FunctionName()
#define COMPILE_PS(FunctionName) PixelShader = compile PS_Target FunctionName()

#define PASS(Name, VertexFunc, PixelFunc) \
pass Name \
{\
    COMPILE_VS(VertexFunc); \
    COMPILE_PS(PixelFunc); \
}

#define TECHNIQUE(Name, Pass, VertexFunc, PixelFunc) \
technique Name \
{\
    PASS(Pass, VertexFunc, PixelFunc) \
}

#endif // __HLSL_MACROS__