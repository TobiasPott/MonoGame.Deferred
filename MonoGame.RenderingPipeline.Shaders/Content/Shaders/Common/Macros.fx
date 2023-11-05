#ifndef __HLSL_MACROS__
#define __HLSL_MACROS__


#define PREFIX_VS VertexStage_
#define PREFIX_PS PixelStage_

// Use vs/ps shader model 4 or higher for DX graphics backend
#define COMPILETARGET_VS vs_4_0
#define COMPILETARGET_PS ps_4_0

#endif // __HLSL_MACROS__