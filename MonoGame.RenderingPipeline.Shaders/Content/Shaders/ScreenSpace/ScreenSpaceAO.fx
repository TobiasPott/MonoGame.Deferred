﻿

//Screen Space Ambient Occlusion shader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../../Includes/Macros.incl.fx"
#define _DEPTH_MAP
#define _NORMAL_MAP
#include "../../Includes/Maps.incl.fx"
#include "../../Includes/VertexStage.incl.fx"
#include "../Common/helper.fx"

float2 AspectRatio;


Texture2D SSAOMap;
SamplerTex(SSAOMap, SSAOMap, CLAMP, LINEAR);

float FalloffMin = 0.000001f;
float FalloffMax = 0.002f;

int Samples = 8;

float Strength = 4;

float SampleRadius = 0.05f;

////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS


float3 GetFrustumRay2(float2 texCoord)
{
    float3 x1 = lerp(FrustumCorners[0], FrustumCorners[1], texCoord.x);
	float3 x2 = lerp(FrustumCorners[2], FrustumCorners[3], texCoord.x);
	float3 outV = lerp(x1, x2, texCoord.y);
	return outV;
}

 //  DEFAULT LIGHT SHADER FOR MODELS

float3 randomNormal(float2 tex)
{
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

float3 getPosition(float2 texCoord)
{
	float linearDepth = DepthMap.SampleLevel(DepthMapSampler, texCoord, 0).r;
	return GetFrustumRay(texCoord) * linearDepth;
}

float weightFunction(float3 vec3, float radius)
{
	// NVIDIA's weighting function
	return 1.0 - /*length(vec3) / radius;*/pow(length(vec3) / radius, 2.0);
}

float4 PixelShaderFunction(VSOut_PosTexViewDir input) : SV_Target
{
	const float3 kernel[] =
	{
	float3(0.2024537f, 0.841204f, -0.9060141f),
	float3(-0.2200423f, 0.6282339f, -0.8275437f),
	float3(-0.7578573f, -0.5583301f, 0.2347527f),
	float3(-0.4540417f, -0.252365f, 0.0694318f),
	float3(0.3677659f, 0.1086345f, -0.4466777f),
	float3(0.8775856f, 0.4617546f, -0.6427765f),
	float3(-0.8433938f, 0.1451271f, 0.2202872f),
	float3(-0.4037157f, -0.8263387f, 0.4698132f),
	float3(0.7867433f, -0.141479f, -0.1567597f),
	float3(0.4839356f, -0.8253108f, -0.1563844f),
	float3(0.4401554f, -0.4228428f, -0.3300118f),
	float3(0.0019193f, -0.8048455f, 0.0726584f),
	float3(-0.0483353f, -0.2527294f, 0.5924745f),
	float3(-0.4192392f, 0.2084218f, -0.3672943f),
	float3(-0.6657394f, 0.6298575f, 0.6342437f),
	float3(-0.0001783f, 0.2834622f, 0.8343929f),
	};

	float2 texCoord = float2(input.TexCoord);

	//get normal data from the NormalMap
	float4 normalData = NormalMap.Sample(NormalMapSampler, texCoord);
	//tranform normal back into [-1,1] range
	float3 currentNormal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

	float linearDepth = DepthMap.Sample(DepthMapSampler, texCoord).r;

	if (linearDepth > 0.99999f)
	{
		return float4(1, 1, 1, 1);
	}
	float3 currentPos = input.ViewDir * linearDepth;

	//alternative 
	//currentPos = getPosition(texCoord);

    float currentDistance = -input.ViewDir.z;

	float amount = 1.0;

	float3 noise = randomNormal(texCoord);

	//HBAO 2 dir
	int sampleshalf = Samples / 2;
	for (int i = 0; i < sampleshalf; i++)
	{
		float3 kernelVec = reflect(kernel[i], noise);
		kernelVec.xy *= AspectRatio;

		float radius = SampleRadius;

		kernelVec.xy = (kernelVec.xy / currentDistance) * radius;

		float biggestAnglePos = 0.0f;

		float biggestAngleNeg = 0.0f;

		float wAO = 0.0;

		for (int b = 1; b <= 4; b++)
		{
			float3 sampleVec = getPosition(texCoord + kernelVec.xy * b / 4.0f) - currentPos;

			float sampleAngle = dot(normalize(sampleVec), currentNormal);

			//sampleAngle *= step(0.3, sampleAngle);

			if (sampleAngle > biggestAnglePos)
			{
				wAO += saturate(weightFunction(sampleVec, radius) * (sampleAngle - biggestAnglePos));

				biggestAnglePos = sampleAngle;
			}

			sampleVec = getPosition(texCoord - kernelVec.xy * b / 4.0f) - currentPos;

			sampleAngle = dot(normalize(sampleVec), currentNormal);

			if (sampleAngle > biggestAngleNeg)
			{
				wAO += saturate(weightFunction(sampleVec, radius) * (sampleAngle - biggestAngleNeg));

				biggestAngleNeg = sampleAngle;
			}
		}

		/*biggestAngle = wAO;
*/
		/*biggestAngle = max(0, biggestAngle);
		*/
/*
		wAO = max(0, wAO);*/

		amount -= wAO / Samples *Strength;
	}

	/*float diff = amount - 0.5f;
	diff *= Strength;

	amount = saturate(0.5f + diff);
*/
	return float4(amount, amount, amount, amount);

}



float4 BilateralBlurVertical(VSOut_PosTex input) : SV_TARGET
{
    const uint numSamples = 9;
    const float texelsize = InverseResolution.x; 
    const float samplerOffsets[numSamples] =
      { -4.0f, -3.0f, -2.0f, -1.0f, 0.0f, 1.0f, 2.0f, 3.0f, 4.0f };
    const float gaussianWeights[numSamples] =
    {
        0.055119, 0.081029, 0.106701, 0.125858, 0.13298, 0.125858, 0.106701, 0.081029, 0.055119
    };
    
    const uint numSamples2 = 4;
    const float samplerOffsets2[numSamples2] =
      { -7.5f, -5.5f, 5.5f, 7.5f };
    const float gaussianWeights2[numSamples2] =
    {
        0.012886, 0.051916, 0.051916, 0.012886,
    };

    float compareDepth = DepthMap.Sample(DepthMapSampler, input.TexCoord).r;

    float4 result = 0;
    float weightSum = 0.0f;
    [unroll]
    for (uint i = 0; i < numSamples; ++i)
    {
        float2 sampleOffset = float2( texelsize * samplerOffsets[i],0);
        float2 samplePos = input.TexCoord + sampleOffset;
         
        float sampleDepth = DepthMap.SampleLevel(DepthMapSampler, samplePos, 0).r;
         
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights[i];
        
        result += SSAOMap.SampleLevel(SSAOMapSampler, samplePos, 0) * weight;
        
        weightSum += weight;
    }

   [unroll]
    for (uint j = 0; j < numSamples2; ++j)
    {
        float2 sampleOffset = float2(texelsize * samplerOffsets2[j],0);
        float2 samplePos = input.TexCoord + sampleOffset;
         
        float sampleDepth = DepthMap.SampleLevel(DepthMapSampler, samplePos, 0).r;
        
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights2[j];
          
        result += SSAOMap.SampleLevel(SSAOMapSampler, samplePos, 0) * weight;
          
        weightSum += weight;
         
    }

    result /= weightSum;
    
    return result;
}

float4 BilateralBlurHorizontal(VSOut_PosTex input) : SV_TARGET
{
    const uint numSamples = 9;
    const float texelsize = InverseResolution.y;
    const float samplerOffsets[numSamples] =
      { -4.0f, -3.0f, -2.0f, -1.0f, 0.0f, 1.0f, 2.0f, 3.0f, 4.0f };
    const float gaussianWeights[numSamples] =
    {
    0.055119, 0.081029,0.106701, 0.125858, 0.13298, 0.125858,0.106701,0.081029, 0.055119
    };
    
    const uint numSamples2 = 4;
    const float samplerOffsets2[numSamples2] =
      { -7.5f, -5.5f, 5.5f, 7.5f };
    const float gaussianWeights2[numSamples2] =
    {
        0.012886, 0.051916, 0.051916, 0.012886,
    };

    float compareDepth = DepthMap.Sample(DepthMapSampler, input.TexCoord).r;
    
    float4 result = 0;
    float weightSum = 0.0f;
    [unroll]
    for (uint i = 0; i < numSamples; ++i)
    {
        float2 sampleOffset = float2(0.0f, texelsize * samplerOffsets[i]);
        float2 samplePos = input.TexCoord + sampleOffset;
        
        float sampleDepth = DepthMap.Sample(DepthMapSampler, samplePos).r;
         
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights[i];
        
        result += SSAOMap.Sample(SSAOMapSampler, samplePos) * weight;
         
        weightSum += weight;
          
    }

    [unroll]
    for (uint j = 0; j < numSamples2; ++j)
    {
        float2 sampleOffset = float2(0.0f, texelsize * samplerOffsets2[j]);
        float2 samplePos = input.TexCoord + sampleOffset;
         
        float sampleDepth = DepthMap.SampleLevel(DepthMapSampler, samplePos, 0).r;
          
        float weight = (1.0f / (0.0001f + abs(compareDepth - sampleDepth))) * gaussianWeights2[j];
        
        result += SSAOMap.Sample(SSAOMapSampler, samplePos) * weight;
          
        weightSum += weight;
         
    }

    result /= weightSum;
    
    return result;
}


technique SSAO
{
    pass Pass1
    {
        COMPILE_VS(VSMain_EncodedViewDir);
        COMPILE_PS(PixelShaderFunction);
    }
}

technique BilateralVertical
{
    pass Pass1
    {
        COMPILE_VS(VSMain_Encoded);
        COMPILE_PS(BilateralBlurVertical);
    }
}

technique BilateralHorizontal
{
    pass Pass1
    {
        COMPILE_VS(VSMain_Encoded);
        COMPILE_PS(BilateralBlurHorizontal);
    }
}
