﻿//Depth Reconstruction from linear depth buffer, TheKosmonaut 2016

#include "../Common/Macros.fx"
#include "../Common/Functions.fx"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4x4 Projection;

float FarClip;

Texture2D DepthMap;

SamplerState texSampler
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
 
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

float TransformDepth(float depth, matrix trafoMatrix)
{
	return (depth*trafoMatrix._33 + trafoMatrix._43) / (depth * trafoMatrix._34 + trafoMatrix._44);
}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  Main function
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

float PixelShaderFunction(VSOutputPosTex input) : DEPTH
{
	float2 texCoord = float2(input.TexCoord);

	float linearDepth = DepthMap.Sample(texSampler, texCoord).r * -FarClip;

	return TransformDepth(linearDepth, Projection);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  TECHNIQUES
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique RestoreDepth
{
	pass Pass1
	{
		VertexShader = compile COMPILETARGET_VS VSMain_Encoded();
		PixelShader = compile COMPILETARGET_PS PixelShaderFunction();
	}
}

