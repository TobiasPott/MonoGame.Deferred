﻿    
//Decalshader TheKosmonaut 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "../../Includes/Macros.incl.fx"
#define _DEPTH_MAP
#include "../../Includes/Maps.incl.fx"
#include "../../Includes/VertexStage.incl.fx"
#include "../Common/helper.fx"


float4x4 WorldView;
float4x4 WorldViewProj;
float4x4 InverseWorldView;
float2 Resolution = float2(1280, 720);


DECLARE_MAP(DecalMap, CLAMP, ANISOTROPIC, 8);

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VERTEX SHADER
////////////////////////////////////////////////////////////////////////////////////////////////////////////

V2F_ViewPos VSMain(VSIn_Pos input)
{
    V2F_ViewPos output;
	//processing geometry coordinates
    output.PositionVS = mul(input.Position, WorldView);
    output.Position = mul(input.Position, WorldViewProj);
    return output;
}

V2F_ViewPosColor VSMain_Colored(VSIn_PosColor input)
{
    V2F_ViewPosColor output;

    output.Position = mul(input.Position, WorldViewProj);
    output.PositionVS = mul(input.Position, WorldView);

    output.Color = input.Color;
    return output;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  PIXEL SHADER
////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Used for both shadow casting and unshadowed, shared function
float4 PSMain_Decal(V2F_ViewPos input) : SV_TARGET
{
    /////////////////////////////////////////
	//Mostly based on this http://martindevans.me/game-development/2015/02/27/Drawing-Stuff-On-Other-Stuff-With-Deferred-Screenspace-Decals/

    //read depth, use point sample or load
    float depth = DepthMap.Sample(DepthMapSampler, input.Position.xy / Resolution).r;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position
    float3 cameraDirVS = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);

    //compute ViewSpace position
    float3 positionVS = depth * cameraDirVS;

	//Transform to box space
    float3 positionOS = mul(float4(positionVS, 1), InverseWorldView).xyz;

    clip(1 - abs(positionOS.xyz));

    float2 texCoord = (positionOS.xy + 1) / 2;

    return DecalMap.Sample(DecalMapSampler, texCoord);
}

float4 PSMain_Line(V2F_ViewPosColor input) : SV_TARGET0
{
	//Depth testing!
    // ToDo: GL Compat: input.Position needs normalized UVs/texCoords and resolution is only hardcoded atm
    float depth = DepthMap.Sample(DepthMapSampler, input.Position.xy / Resolution).r * FarClip;

	//Basically extend the depth of this ray to the end of the far plane, this gives us the position of the sphere only
    float localDepth = -input.PositionVS.z;
	
    if (localDepth > depth)
    {
        if ((input.Position.x % 8 > 4) == (input.Position.y % 8 > 4))
            discard;

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
        COMPILE_VS(VSMain);
        COMPILE_PS(PSMain_Decal);
    }
}

technique Outline
{
    pass Pass1
    {
        COMPILE_VS(VSMain_Colored);
		COMPILE_PS(PSMain_Line);
    }
}