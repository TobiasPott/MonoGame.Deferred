#ifndef __HLSL_MAPS__
#define __HLSL_MAPS__

#define Tex2D(Name) Texture2D Name ## Map

#define SamplerTex(SamplerName, TexName, WrapMode, Filter) SamplerState SamplerName ## Sampler \
{ \
    Texture = (TexName); \
    AddressU = WrapMode; \
    AddressV = WrapMode; \
    MagFilter = Filter; \
    MinFilter = Filter; \
    Mipfilter = Filter; \
}

#define Sampler(SamplerName, WrapMode, Filter) SamplerState SamplerName ## Sampler \
{ \
    AddressU = WrapMode; \
    AddressV = WrapMode; \
    MagFilter = Filter; \
    MinFilter = Filter; \
    Mipfilter = Filter; \
}

#ifdef _DEPTH_MAP
Tex2D(Depth);
#endif

#ifdef _ALBEDO_MAP
Tex2D(Albedo);
#endif

#ifdef _NORMAL_MAP
Tex2D(Normal);
#endif

  
#endif // __HLSL_MAPS__