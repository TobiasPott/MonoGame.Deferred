#ifndef __HLSL_FRUSTUM__
#define __HLSL_FRUSTUM__


float FarClip = 512;
float3 FrustumCorners[4]; //In Viewspace!


float3 GetFrustumRay(float2 texCoord)
{
    float3 x1 = lerp(FrustumCorners[0], FrustumCorners[1], texCoord.x);
    float3 x2 = lerp(FrustumCorners[2], FrustumCorners[3], texCoord.x);
    float3 outV = lerp(x1, x2, texCoord.y);
    return outV;
}

#endif // __HLSL_FRUSTUM__