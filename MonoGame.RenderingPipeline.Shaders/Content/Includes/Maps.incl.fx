#ifndef __HLSL_MAPS__
#define __HLSL_MAPS__


#define TEXTURE(Name) Texture2D Name ## Map

#define DX9_OR_OLDER
#define DX10_OR_NEWER

#ifdef DX10_OR_NEWER
#define DECLARE_MAP(Name, InWrapMode, InFilter, InMaxAnisotropy) Texture2D Name; \
 \
SamplerState Name ## Sampler \
{ \
    Texture = (TexName); \
    AddressU = InWrapMode; \
    AddressV = InWrapMode; \
    Filter = InFilter; \
    MaxAnisotropy = InMaxAnisotropy; \
};
#else
#define DECLARE_MAP(Name, InWrapMode, InFilter, InMaxAnisotropy) Texture2D Name; \
 \
SamplerState Name##Sampler \
{ \
    Texture = <TexName>; \
    AddressU = InWrapMode; \
    AddressV = InWrapMode; \
    MagFilter = InFilter; \
    MinFilter = InFilter; \
    Mipfilter = InFilter; \
    MaxAnisotropy = InMaxAnisotropy; \
};
#endif


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

  
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  DEFAULT MAPS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef _DEPTH_MAP
DECLARE_MAP(DepthMap, CLAMP, POINT, 0);
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