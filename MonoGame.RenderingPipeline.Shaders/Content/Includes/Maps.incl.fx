#ifndef __HLSL_MAPS__
#define __HLSL_MAPS__

#include "Macros.incl.fx"


#define TEXTURE(Name) Texture2D Name ## Map

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

#define DECLARE_MAP_EXPLICIT(Name, AddrU, AddrV, AddrW, Mag, Min, Mip, Aniso) Texture2D Name; \
 \
SamplerState Name##Sampler \
{ \
    Texture = <TexName>; \
    AddressU = AddrU; \
    AddressV = AddrV; \
    AddressW = AddrW; \
    MagFilter = Mag; \
    MinFilter = Min; \
    Mipfilter = Mip; \
    MaxAnisotropy = Aniso; \
};


#define SamplerTex(SamplerName, TexName, WrapMode, Filter) SamplerState SamplerName ## Sampler \
{ \
    Texture = (TexName); \
    AddressU = WrapMode; \
    AddressV = WrapMode; \
    MagFilter = Filter; \
    MinFilter = Filter; \
    Mipfilter = Filter; \
}

#define SamplerTexWrapFilter(SamplerName, TexName, WrapU, WrapV, FilterMag, FiltereMin, FilterMip) SamplerState SamplerName ## Sampler \
{ \
    Texture = (TexName); \
    AddressU = WrapU; \
    AddressV = WrapV; \
    MagFilter = FilterMag; \
    MinFilter = FiltereMin; \
    Mipfilter = FilterMip; \
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
//  DEFAULT SAMPLER
////////////////////////////////////////////////////////////////////////////////////////////////////////////

Sampler(Point, CLAMP, POINT);
Sampler(Linear, CLAMP, LINEAR);
  

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  DEFAULT MAPS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef _DEPTH_MAP
DECLARE_MAP_EXPLICIT(DepthMap, CLAMP, CLAMP, CLAMP, POINT, POINT, NONE, 0);
#endif

#ifdef _ALBEDO_MAP
DECLARE_MAP(AlbedoMap, CLAMP, POINT, 0);
#endif

#ifdef _NORMAL_MAP
DECLARE_MAP(NormalMap, CLAMP, POINT, 0);
#endif

#ifdef _SHADOW_MAP
DECLARE_MAP(ShadowMap, CLAMP, POINT, 0);
#endif


#endif // __HLSL_MAPS__