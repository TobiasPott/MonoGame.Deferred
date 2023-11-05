////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Deferred Compose
//  Composes the light buffers and the GBuffer to our final HDR Output.
//  Converts albedo from Gamma 2.2 to 1.0 and outputs an HDR file.

#include "../../Includes/Macros.incl.fx"
#include "../../Includes/VertexStage.incl.fx"
#include "../Common/helper.fx"

Texture2D colorMap;
Texture2D normalMap;
Texture2D diffuseLightMap;
Texture2D specularLightMap;
Texture2D volumeLightMap;

Texture2D SSAOMap;

bool useSSAO = true;

sampler pointSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
////  FUNCTION DEFINITIONS

float4 PSMain(VSOut_PosTex input) : SV_Target
{
	int3 texCoordInt = int3(input.Position.xy, 0);

	float4 diffuseColor = colorMap.Load(texCoordInt);
	float4 normalInfo = normalMap.Load(texCoordInt);
	//Convert gamma for linear pipeline
	diffuseColor.rgb = pow(abs(diffuseColor.rgb), 2.2f);

	// See Resources/MaterialEffect for different mat types!
	// materialType 3 = emissive
	// materialType 2 = hologram
	// materialType 1 = default
	float materialType = decodeMattype(normalInfo.b);

	float metalness = decodeMetalness(normalInfo.b);

	//Our "volumetric" light data. This is a seperate buffer that is renders on top of all other stuff.
	float3 volumetrics = volumeLightMap.Load(texCoordInt).rgb;

	//Emissive Material
	//If the material is emissive (matType == 3) we store the factor inside metalness. We simply output the emissive material and do not compose with lighting
	if (abs(materialType - 3) < 0.1f)
	{
		// Optional: 2 << metalness*8, pow(2, m*8) etc.
		// Note: metalness is used as the power value in this case
		return float4(diffuseColor.rgb * metalness * 8 + volumetrics, 1);
	}

	float3 diffuseContrib = float3(0, 0, 0);

	//SSAO
	float ssaoContribution = 1;

	[branch]
	if (useSSAO)
	{
		ssaoContribution = SSAOMap.SampleLevel(pointSampler, input.TexCoord, 0).r;
	}

	//float f0 = lerp(0.04f, diffuseColor.g * 0.25 + 0.75, metalness);

	float3 diffuseLight = diffuseLightMap.Load(texCoordInt).rgb;
	float3 specularLight = specularLightMap.Load(texCoordInt).rgb;

	float3 plasticFinal = diffuseColor.rgb * (diffuseLight)+specularLight;

	float3 metalFinal = diffuseColor.rgb * specularLight;

	float3 finalValue = lerp(plasticFinal, metalFinal, metalness) + diffuseContrib;

	float3 output = (finalValue * ssaoContribution + volumetrics);

	return float4(output, 1);
}

technique TechniqueLinear
{
    pass Pass1
    {
        COMPILE_VS(VSMain_Encoded);
        COMPILE_PS(PSMain);
    }
}
