using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    //As suggested here http://community.monogame.net/t/deferred-engine-playground-download/8180/283?u=kosmonautgames
    //the whole global shaders is shortened to load early without the need for a seperate load function
    // by bettina4you

    public static class ShaderGlobals
    {
        public static ContentManager content;
    }

    public static class Shaders
    {
        //A static file which contains all shaders
        //Born out of need for quick thoughtless shader building
        //I am working on making seperate shading modules instead and will slowly shorten this one.


        //Depth Reconstruction
        public static class ReconstructDepth
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/ScreenSpace/ReconstructDepth");

            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
        }

        //Id Generator
        public static class IdRender
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Editor/IdRender");
            public static readonly EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            public static readonly EffectParameter Param_ColorId = Effect.Parameters["ColorId"];
            public static readonly EffectParameter Param_OutlineSize = Effect.Parameters["OutlineSize"];
            public static readonly EffectParameter Param_World = Effect.Parameters["World"];

            public static readonly EffectPass Technique_Id = Effect.Techniques["DrawId"].Passes[0];
            public static readonly EffectPass Technique_Outline = Effect.Techniques["DrawOutline"].Passes[0];
        }

        //Billboard Renderert
        public static class Billboard
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Editor/BillboardEffect");

            public static readonly EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            public static readonly EffectParameter Param_WorldView = Effect.Parameters["WorldView"];
            public static readonly EffectParameter Param_AspectRatio = Effect.Parameters["AspectRatio"];
            public static readonly EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static readonly EffectParameter Param_Texture = Effect.Parameters["Texture"];
            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_IdColor = Effect.Parameters["IdColor"];

            public static readonly EffectTechnique Technique_Billboard = Effect.Techniques["Billboard"];
            public static readonly EffectTechnique Technique_Id = Effect.Techniques["Id"];
        }

        //Temporal AntiAliasing


        public static class PostProcssing
        {
            //Vignette and CA
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("shaders/postprocessing/postprocessing");
            public static readonly EffectParameter Param_ScreenTexture = Effect.Parameters["ScreenTexture"];
            public static readonly EffectParameter Param_ChromaticAbberationStrength = Effect.Parameters["ChromaticAbberationStrength"];
            public static readonly EffectParameter Param_SCurveStrength = Effect.Parameters["SCurveStrength"];
            public static readonly EffectParameter Param_WhitePoint = Effect.Parameters["WhitePoint"];
            public static readonly EffectParameter Param_PowExposure = Effect.Parameters["PowExposure"];

            public static readonly EffectTechnique Technique_VignetteChroma = Effect.Techniques["VignetteChroma"];
            public static readonly EffectTechnique Technique_Base = Effect.Techniques["Base"];
        }

        //Hologram Effect
        public static class Hologram
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Hologram/HologramEffect");
            public static readonly EffectParameter Param_World = Effect.Parameters["World"];
            public static readonly EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
        }
        //ScreenSpaceReflection Effect
        public static class SSR
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceReflections");

            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static readonly EffectParameter Param_TargetMap = Effect.Parameters["TargetMap"];
            public static readonly EffectParameter Param_Resolution = Effect.Parameters["resolution"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_Time = Effect.Parameters["Time"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            public static readonly EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static readonly EffectParameter Param_NoiseMap = Effect.Parameters["NoiseMap"];

            public static readonly EffectTechnique Technique_Default = Effect.Techniques["Default"];
            public static readonly EffectTechnique Technique_Taa = Effect.Techniques["TAA"];
        }

        //Screen Space Ambient Occlusion Effect

        public static class SSAO
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

            public static readonly EffectParameter Param_SSAOMap = Effect.Parameters["SSAOMap"];
            public static readonly EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_CameraPosition = Effect.Parameters["CameraPosition"];
            public static readonly EffectParameter Param_InverseViewProjection = Effect.Parameters["InverseViewProjection"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_ViewProjection = Effect.Parameters["ViewProjection"];

            public static readonly EffectParameter Param_FalloffMin = Effect.Parameters["FalloffMin"];
            public static readonly EffectParameter Param_FalloffMax = Effect.Parameters["FalloffMax"];
            public static readonly EffectParameter Param_Samples = Effect.Parameters["Samples"];
            public static readonly EffectParameter Param_Strength = Effect.Parameters["Strength"];
            public static readonly EffectParameter Param_SampleRadius = Effect.Parameters["SampleRadius"];
            public static readonly EffectParameter Param_InverseResolution = Effect.Parameters["InverseResolution"];
            public static readonly EffectParameter Param_AspectRatio = Effect.Parameters["AspectRatio"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];

            public static readonly EffectTechnique Technique_SSAO = Effect.Techniques["SSAO"];
            public static readonly EffectTechnique Technique_BlurHorizontal = Effect.Techniques["BilateralHorizontal"];
            public static readonly EffectTechnique Technique_BlurVertical = Effect.Techniques["BilateralVertical"];
        }

        //Gaussian Blur
        public static class GaussianBlur
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
            public static readonly EffectParameter Param_InverseResolution = Effect.Parameters["InverseResolution"];
            public static readonly EffectParameter Param_TargetMap = Effect.Parameters["TargetMap"];
        }

        //DeferredCompose
        public static class DeferredCompose
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Deferred/DeferredCompose");

            public static readonly EffectParameter Param_ColorMap = Effect.Parameters["colorMap"];
            public static readonly EffectParameter Param_NormalMap = Effect.Parameters["normalMap"];
            public static readonly EffectParameter Param_diffuseLightMap = Effect.Parameters["diffuseLightMap"];
            public static readonly EffectParameter Param_specularLightMap = Effect.Parameters["specularLightMap"];
            public static readonly EffectParameter Param_volumeLightMap = Effect.Parameters["volumeLightMap"];
            public static readonly EffectParameter Param_HologramMap = Effect.Parameters["HologramMap"];
            public static readonly EffectParameter Param_SSAOMap = Effect.Parameters["SSAOMap"];
            public static readonly EffectParameter Param_LinearMap = Effect.Parameters["LinearMap"];
            public static readonly EffectParameter Param_SSRMap = Effect.Parameters["SSRMap"];
            public static readonly EffectParameter Param_UseSSAO = Effect.Parameters["useSSAO"];

            public static readonly EffectTechnique Technique_NonLinear = Effect.Techniques["TechniqueNonLinear"];
            public static readonly EffectTechnique Technique_Linear = Effect.Techniques["TechniqueLinear"];
        }

        //Deferred Clear
        public static class DeferredClear
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Deferred/DeferredClear");
        }

        //Directional light
        public static class DeferredDirectionalLight
        {
            public static readonly Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Deferred/DeferredDirectionalLight");

            public static readonly EffectTechnique Technique_Unshadowed = Effect.Techniques["Unshadowed"];
            public static readonly EffectTechnique Technique_SSShadowed = Effect.Techniques["SSShadowed"];
            public static readonly EffectTechnique Technique_Shadowed = Effect.Techniques["Shadowed"];
            public static readonly EffectTechnique Technique_ShadowOnly = Effect.Techniques["ShadowOnly"];

            public static readonly EffectParameter Param_ViewProjection = Effect.Parameters["ViewProjection"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            public static readonly EffectParameter Param_CameraPosition = Effect.Parameters["cameraPosition"];
            public static readonly EffectParameter Param_InverseViewProjection = Effect.Parameters["InvertViewProjection"];
            public static readonly EffectParameter Param_LightViewProjection = Effect.Parameters["LightViewProjection"];
            public static readonly EffectParameter Param_LightView = Effect.Parameters["LightView"];
            public static readonly EffectParameter Param_LightFarClip = Effect.Parameters["LightFarClip"];

            public static readonly EffectParameter Param_LightColor = Effect.Parameters["lightColor"];
            public static readonly EffectParameter Param_LightIntensity = Effect.Parameters["lightIntensity"];
            public static readonly EffectParameter Param_LightDirection = Effect.Parameters["LightVector"];
            public static readonly EffectParameter Param_ShadowFiltering = Effect.Parameters["ShadowFiltering"];
            public static readonly EffectParameter Param_ShadowMapSize = Effect.Parameters["ShadowMapSize"];

            public static readonly EffectParameter Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
            public static readonly EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_ShadowMap = Effect.Parameters["ShadowMap"];
            public static readonly EffectParameter Param_SSShadowMap = Effect.Parameters["SSShadowMap"];
        }

        //Directional light
        public static class DeferredPointLight
        {
            public static Effect deferredPointLight_Effect = ShaderGlobals.content.Load<Effect>("Shaders/Deferred/DeferredPointLight");

            public static EffectTechnique deferredPointLightTechnique_Unshadowed = deferredPointLight_Effect.Techniques["Unshadowed"];
            public static EffectTechnique deferredPointLightUnshadowedVolumetric = deferredPointLight_Effect.Techniques["UnshadowedVolume"];
            public static EffectTechnique deferredPointLightShadowedSDF = deferredPointLight_Effect.Techniques["Shadowed"];
            public static EffectTechnique deferredPointLightShadowed = deferredPointLight_Effect.Techniques["ShadowedSDF"];
            public static EffectTechnique deferredPointLightShadowedVolumetric = deferredPointLight_Effect.Techniques["ShadowedVolume"];
            public static EffectTechnique deferredPointLightWriteStencil = deferredPointLight_Effect.Techniques["WriteStencilMask"];

            public static EffectParameter deferredPointLightParameterShadowMap = deferredPointLight_Effect.Parameters["ShadowMap"];

            public static EffectParameter deferredPointLightParameterResolution = deferredPointLight_Effect.Parameters["Resolution"];
            public static EffectParameter deferredPointLightParameter_WorldView = deferredPointLight_Effect.Parameters["WorldView"];
            public static EffectParameter deferredPointLightParameter_WorldViewProjection = deferredPointLight_Effect.Parameters["WorldViewProj"];
            public static EffectParameter deferredPointLightParameter_InverseView = deferredPointLight_Effect.Parameters["InverseView"];

            public static EffectParameter deferredPointLightParameter_LightPosition = deferredPointLight_Effect.Parameters["lightPosition"];
            public static EffectParameter deferredPointLightParameter_LightColor = deferredPointLight_Effect.Parameters["lightColor"];
            public static EffectParameter deferredPointLightParameter_LightRadius = deferredPointLight_Effect.Parameters["lightRadius"];
            public static EffectParameter deferredPointLightParameter_LightIntensity = deferredPointLight_Effect.Parameters["lightIntensity"];
            public static EffectParameter deferredPointLightParameter_ShadowMapSize = deferredPointLight_Effect.Parameters["ShadowMapSize"];
            public static EffectParameter deferredPointLightParameter_ShadowMapRadius = deferredPointLight_Effect.Parameters["ShadowMapRadius"];
            public static EffectParameter deferredPointLightParameter_Inside = deferredPointLight_Effect.Parameters["inside"];
            public static EffectParameter deferredPointLightParameter_Time = deferredPointLight_Effect.Parameters["Time"];
            public static EffectParameter deferredPointLightParameter_FarClip = deferredPointLight_Effect.Parameters["FarClip"];
            public static EffectParameter deferredPointLightParameter_LightVolumeDensity = deferredPointLight_Effect.Parameters["lightVolumeDensity"];

            public static EffectParameter deferredPointLightParameter_VolumeTexParam = deferredPointLight_Effect.Parameters["VolumeTex"];
            public static EffectParameter deferredPointLightParameter_VolumeTexSizeParam = deferredPointLight_Effect.Parameters["VolumeTexSize"];
            public static EffectParameter deferredPointLightParameter_VolumeTexResolution = deferredPointLight_Effect.Parameters["VolumeTexResolution"];
            public static EffectParameter deferredPointLightParameter_InstanceInverseMatrix = deferredPointLight_Effect.Parameters["InstanceInverseMatrix"];
            public static EffectParameter deferredPointLightParameter_InstanceScale = deferredPointLight_Effect.Parameters["InstanceScale"];
            public static EffectParameter deferredPointLightParameter_InstanceSDFIndex = deferredPointLight_Effect.Parameters["InstanceSDFIndex"];
            public static EffectParameter deferredPointLightParameter_InstancesCount = deferredPointLight_Effect.Parameters["InstancesCount"];

            public static EffectParameter deferredPointLightParameter_NoiseMap = deferredPointLight_Effect.Parameters["NoiseMap"];
            public static EffectParameter deferredPointLightParameter_AlbedoMap = deferredPointLight_Effect.Parameters["AlbedoMap"];
            public static EffectParameter deferredPointLightParameter_NormalMap = deferredPointLight_Effect.Parameters["NormalMap"];
            public static EffectParameter deferredPointLightParameter_DepthMap = deferredPointLight_Effect.Parameters["DepthMap"];

        }

    }

}
