#ifndef __HLSL_FRUSTUMCORNERS__
#define __HLSL_FRUSTUMCORNERS__



float3 FrustumCorners[4]; //In Viewspace!

float3 GetFrustumRay(float2 texCoord)
{
    float index = texCoord.x + (texCoord.y * 2);
    return FrustumCorners[index];
}
float3 GetFrustumRay(uint id)
{
	//Bottom left
    if (id < 1)
    {
        return FrustumCorners[2];
    }
    else if (id < 2) //Top left
    {
        return FrustumCorners[2] + (FrustumCorners[0] - FrustumCorners[2]) * 2;
    }
    else
    {
        return FrustumCorners[2] + (FrustumCorners[3] - FrustumCorners[2]) * 2;
    }

}

#endif // __HLSL_FRUSTUMCORNERS__