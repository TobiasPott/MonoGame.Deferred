using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    //As suggested here http://community.monogame.net/t/deferred-engine-playground-download/8180/283?u=kosmonautgames
    //the whole global shaders is shortened to load early without the need for a seperate load function
    // by bettina4you

    public static class Globals
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
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ReconstructDepth");
            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
        }

        //Id Generator
        public static class IdRender
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/Editor/IdRender");
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
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/Editor/BillboardEffect");

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


        //Vignette and CA
        public static readonly Effect PostProcessing = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
        public static readonly EffectParameter PostProcessingParameter_ScreenTexture = PostProcessing.Parameters["ScreenTexture"];
        public static readonly EffectParameter PostProcessingParameter_ChromaticAbberationStrength = PostProcessing.Parameters["ChromaticAbberationStrength"];
        public static readonly EffectParameter PostProcessingParameter_SCurveStrength = PostProcessing.Parameters["SCurveStrength"];
        public static readonly EffectParameter PostProcessingParameter_WhitePoint = PostProcessing.Parameters["WhitePoint"];
        public static readonly EffectParameter PostProcessingParameter_PowExposure = PostProcessing.Parameters["PowExposure"];
        public static readonly EffectTechnique PostProcessingTechnique_VignetteChroma = PostProcessing.Techniques["VignetteChroma"];
        public static readonly EffectTechnique PostProcessingTechnique_Base = PostProcessing.Techniques["Base"];


        //Hologram Effect
        public static readonly Effect HologramEffect = Globals.content.Load<Effect>("Shaders/Hologram/HologramEffect");
        public static readonly EffectParameter HologramEffectParameter_World = HologramEffect.Parameters["World"];
        public static readonly EffectParameter HologramEffectParameter_WorldViewProj = HologramEffect.Parameters["WorldViewProj"];

        //ScreenSpaceReflection Effect
        public static class SSR
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceReflections");

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

        public static readonly Effect ScreenSpaceEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

        public static readonly EffectParameter ScreenSpaceEffectParameter_SSAOMap = ScreenSpaceEffect.Parameters["SSAOMap"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_NormalMap = ScreenSpaceEffect.Parameters["NormalMap"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_DepthMap = ScreenSpaceEffect.Parameters["DepthMap"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_CameraPosition = ScreenSpaceEffect.Parameters["CameraPosition"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_InverseViewProjection = ScreenSpaceEffect.Parameters["InverseViewProjection"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_Projection = ScreenSpaceEffect.Parameters["Projection"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_ViewProjection = ScreenSpaceEffect.Parameters["ViewProjection"];

        public static readonly EffectParameter ScreenSpaceEffect_FalloffMin = ScreenSpaceEffect.Parameters["FalloffMin"];
        public static readonly EffectParameter ScreenSpaceEffect_FalloffMax = ScreenSpaceEffect.Parameters["FalloffMax"];
        public static readonly EffectParameter ScreenSpaceEffect_Samples = ScreenSpaceEffect.Parameters["Samples"];
        public static readonly EffectParameter ScreenSpaceEffect_Strength = ScreenSpaceEffect.Parameters["Strength"];
        public static readonly EffectParameter ScreenSpaceEffect_SampleRadius = ScreenSpaceEffect.Parameters["SampleRadius"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_InverseResolution = ScreenSpaceEffect.Parameters["InverseResolution"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_AspectRatio = ScreenSpaceEffect.Parameters["AspectRatio"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_FrustumCorners = ScreenSpaceEffect.Parameters["FrustumCorners"];

        public static readonly EffectTechnique ScreenSpaceEffectTechnique_SSAO = ScreenSpaceEffect.Techniques["SSAO"];
        public static readonly EffectTechnique ScreenSpaceEffectTechnique_BlurHorizontal = ScreenSpaceEffect.Techniques["BilateralHorizontal"];
        public static readonly EffectTechnique ScreenSpaceEffectTechnique_BlurVertical = ScreenSpaceEffect.Techniques["BilateralVertical"];


        //Gaussian Blur
        public static class GaussianBlur
        {
            public static readonly Effect GaussianBlur_Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
            public static readonly EffectParameter GaussianBlurEffectParameter_InverseResolution = GaussianBlur_Effect.Parameters["InverseResolution"];
            public static readonly EffectParameter GaussianBlurEffectParameter_TargetMap = GaussianBlur_Effect.Parameters["TargetMap"];
        }

        //DeferredCompose
        public static class DeferredCompose
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredCompose");

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

        //DeferredClear
        public static readonly Effect DeferredClear_Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredClear");

        //Directional light

        public static readonly Effect DeferredDirectionalLight_Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredDirectionalLight");

        public static readonly EffectTechnique DeferredDirectionalLightTechnique_Unshadowed = DeferredDirectionalLight_Effect.Techniques["Unshadowed"];
        public static readonly EffectTechnique DeferredDirectionalLightTechnique_SSShadowed = DeferredDirectionalLight_Effect.Techniques["SSShadowed"];
        public static readonly EffectTechnique DeferredDirectionalLightTechnique_Shadowed = DeferredDirectionalLight_Effect.Techniques["Shadowed"];
        public static readonly EffectTechnique DeferredDirectionalLightTechnique_ShadowOnly = DeferredDirectionalLight_Effect.Techniques["ShadowOnly"];

        public static readonly EffectParameter DeferredDirectionalLightParameter_ViewProjection = DeferredDirectionalLight_Effect.Parameters["ViewProjection"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_FrustumCorners = DeferredDirectionalLight_Effect.Parameters["FrustumCorners"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_CameraPosition = DeferredDirectionalLight_Effect.Parameters["cameraPosition"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_InverseViewProjection = DeferredDirectionalLight_Effect.Parameters["InvertViewProjection"];
        public static readonly EffectParameter DeferredDirectionalLightParameterLightViewProjection = DeferredDirectionalLight_Effect.Parameters["LightViewProjection"];
        public static readonly EffectParameter DeferredDirectionalLightParameterLightView = DeferredDirectionalLight_Effect.Parameters["LightView"];
        public static readonly EffectParameter DeferredDirectionalLightParameterLightFarClip = DeferredDirectionalLight_Effect.Parameters["LightFarClip"];

        public static readonly EffectParameter DeferredDirectionalLightParameter_LightColor = DeferredDirectionalLight_Effect.Parameters["lightColor"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_LightIntensity = DeferredDirectionalLight_Effect.Parameters["lightIntensity"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_LightDirection = DeferredDirectionalLight_Effect.Parameters["LightVector"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_ShadowFiltering = DeferredDirectionalLight_Effect.Parameters["ShadowFiltering"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_ShadowMapSize = DeferredDirectionalLight_Effect.Parameters["ShadowMapSize"];

        public static readonly EffectParameter DeferredDirectionalLightParameter_AlbedoMap = DeferredDirectionalLight_Effect.Parameters["AlbedoMap"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_NormalMap = DeferredDirectionalLight_Effect.Parameters["NormalMap"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_DepthMap = DeferredDirectionalLight_Effect.Parameters["DepthMap"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_ShadowMap = DeferredDirectionalLight_Effect.Parameters["ShadowMap"];
        public static readonly EffectParameter DeferredDirectionalLightParameter_SSShadowMap = DeferredDirectionalLight_Effect.Parameters["SSShadowMap"];


        //Point Light


        public static void Load(ContentManager content)
        {
        }
    }

}
