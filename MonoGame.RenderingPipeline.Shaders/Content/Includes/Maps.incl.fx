#ifndef __HLSL_MAPS__
#define __HLSL_MAPS__

#define Tex2D(Name) Texture2D Name ## Map

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