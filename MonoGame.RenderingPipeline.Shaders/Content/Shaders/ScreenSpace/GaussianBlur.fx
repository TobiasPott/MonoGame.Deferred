
#include "../../Includes/Macros.incl.fx"
#include "../../Includes/Maps.incl.fx"
#include "../../Includes/VertexStage.incl.fx"

static const int BlurKernelSize = 13;
static const float2 BlurKernel[BlurKernelSize] =
{
    { -6, 0 },
    { -5, 0 },
    { -4, 0 },
    { -3, 0 },
    { -2, 0 },
    { -1, 0 },
    { 0, 0 },
    { 1, 0 },
    { 2, 0 },
    { 3, 0 },
    { 4, 0 },
    { 5, 0 },
    { 6, 0 }
};
static const float BlurWeights[BlurKernelSize] =
{
    0.002216f, 0.008764f, 0.026995f, 0.064759f, 0.120985f, 0.176033f, 0.199471f,
    0.176033f, 0.120985f, 0.064759f, 0.026995f, 0.008764f, 0.002216f
};

DECLARE_MAP(TargetMap, CLAMP, LINEAR, 0);

float2 InverseResolution;

struct VI
{
    float2 Position : POSITION0;
};

float4 PSMain_H(VSOut_PosTex input) : SV_Target0
{
    float4 outputColor = float4(0, 0, 0, 0);
    
    [unroll] 
    for (int i = 0; i < BlurKernelSize; i++)
    {
        float2 offset = BlurKernel[i].xy * InverseResolution.xy;
    
        float4 sample = TargetMap.Sample(TargetMapSampler, input.TexCoord + offset);
        sample *= BlurWeights[i];
		
        outputColor += sample;
    }
    
    return outputColor;
}

float4 PSMain_V(VSOut_PosTex input) : SV_Target0
{
    float4 outputColor = float4(0, 0, 0, 0);
    
    [unroll] 
    for (int i = 0; i < BlurKernelSize; i++)
    {
        float2 offset = BlurKernel[i].yx * InverseResolution.xy;
    
        float4 sample = TargetMap.Sample(TargetMapSampler, input.TexCoord + offset);
        sample *= BlurWeights[i];

        outputColor += sample;
    }
    
    return outputColor;
}

technique GaussianBlur
{
    pass Horizontal
    {
        COMPILE_VS(VSMain_Encoded);
        COMPILE_PS(PSMain_H);
    }

    pass Vertical
    {
        COMPILE_VS(VSMain_Encoded);
        COMPILE_PS(PSMain_V);
    }
}
