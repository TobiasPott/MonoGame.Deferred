
//Decalshader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../Common/Macros.fx"
#include "../Common/helper.fx"

float4x4 WorldView;
float4x4 WorldViewProj;
float4x4 InverseWorldView;
float FarClip;

Texture2D DepthMap;
Texture2D DecalMap;

SamplerState AnisotropicSampler
{
	Texture = (DecalMap);
	Filter = Anisotropic;
	MaxAnisotropy = 8;
	AddressU = Clamp;
	AddressV = Clamp;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float4 Position : POSITION0;
};
struct VertexShaderOutput_VS
{
    float4 Position : SV_POSITION;
	float4 PositionVS : TEXCOORD1;
};

struct LineVertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR;
};
struct LineVertexShaderOutput_VS
{
	float4 Position : SV_POSITION;
	float4 PositionVS : TEXCOORD1;
	float4 Color : COLOR0;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  VERTEX SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

VertexShaderOutput_VS VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput_VS output;
	//processing geometry coordinates
	output.PositionVS = mul(input.Position, WorldView);
	output.Position = mul(input.Position, WorldViewProj);
	return output;
}

LineVertexShaderOutput_VS LineVertexShaderFunction(LineVertexShaderInput input)
{
    LineVertexShaderOutput_VS output;

	output.Position = mul(input.Position, WorldViewProj);
	output.PositionVS = mul(input.Position, WorldView);

	output.Color = input.Color;
	return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//  PIXEL SHADER
	////////////////////////////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  HELPER FUNCTIONS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Used for both shadow casting and unshadowed, shared function
float4 DecalPixelShader(VertexShaderOutput_VS input) : SV_TARGET
{
    /////////////////////////////////////////
	//Mostly based on this http://martindevans.me/game-development/2015/02/27/Drawing-Stuff-On-Other-Stuff-With-Deferred-Screenspace-Decals/

    //read depth, use point sample or load
    float depth = DepthMap.Load(int3(input.Position.xy, 0)).r;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position
	float3 cameraDirVS = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);

    //compute ViewSpace position
	float3 positionVS = depth * cameraDirVS;

	//Transform to box space
	float3 positionOS = mul(float4(positionVS, 1), InverseWorldView).xyz;

	clip(1 - abs(positionOS.xyz));

	float2 textureCoordinate = (positionOS.xy + 1) / 2;

	return DecalMap.Sample(AnisotropicSampler, textureCoordinate);
}

float4 LinePixelShaderFunction(LineVertexShaderOutput_VS input) : SV_TARGET0
{
	//Depth testing!
	float depth = DepthMap.Load(int3(input.Position.xy, 0)).r * FarClip;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position of the sphere only
	float localDepth = -input.PositionVS.z;
	
	if (localDepth > depth)
	{
		if ((input.Position.x % 8 > 4)  == (input.Position.y % 8 > 4)) discard;

		return float4(0, 0, 0, 0);
	}

	return input.Color; //+ AmbientColor * AmbientIntensity;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  TECHNIQUES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

technique Decal
{
    pass Pass1
    {
        VertexShader = compile COMPILETARGET_VS VertexShaderFunction();
        PixelShader = compile COMPILETARGET_PS DecalPixelShader();
    }
}

technique Outline
{
	pass Pass1
	{
        VertexShader = compile COMPILETARGET_VS LineVertexShaderFunction();
		PixelShader = compile COMPILETARGET_PS LinePixelShaderFunction();
	}
}