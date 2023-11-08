#ifndef __HLSL_MACROS__
#define __HLSL_MACROS__


#define PREFIX_VS VertexStage_
#define PREFIX_PS PixelStage_

#define DX10_OR_NEWER 0


#define VS_Target vs_3_0
#define PS_Target ps_3_0


#if DX10_OR_NEWER == 1
#ifndef CONST
#define CONST const
#endif
#else
#ifndef CONST
#define CONST static const
#endif
#endif





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