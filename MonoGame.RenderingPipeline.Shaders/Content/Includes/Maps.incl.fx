#ifndef __HLSL_MAPS__
#define __HLSL_MAPS__

#define TEXTURE(Name) Texture2D Name ## Map

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
TEXTURE(Depth);
#endif

#ifdef _ALBEDO_MAP
TEXTURE(Albedo);
#endif

#ifdef _NORMAL_MAP
TEXTURE(Normal);
#endif

#ifdef _SHADOW_MAP
TEXTURE(Shadow);
#endif

  
#endif // __HLSL_MAPS__