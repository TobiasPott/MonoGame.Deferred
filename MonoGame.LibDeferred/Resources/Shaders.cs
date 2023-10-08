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
        public static readonly Effect PostProcessing_Effect = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
        public static readonly EffectParameter PostProcessingParameter_ScreenTexture = PostProcessing_Effect.Parameters["ScreenTexture"];
        public static readonly EffectParameter PostProcessingParameter_ChromaticAbberationStrength = PostProcessing_Effect.Parameters["ChromaticAbberationStrength"];
        public static readonly EffectParameter PostProcessingParameter_SCurveStrength = PostProcessing_Effect.Parameters["SCurveStrength"];
        public static readonly EffectParameter PostProcessingParameter_WhitePoint = PostProcessing_Effect.Parameters["WhitePoint"];
        public static readonly EffectParameter PostProcessingParameter_PowExposure = PostProcessing_Effect.Parameters["PowExposure"];
        public static readonly EffectTechnique PostProcessingTechnique_VignetteChroma = PostProcessing_Effect.Techniques["VignetteChroma"];
        public static readonly EffectTechnique PostProcessingTechnique_Base = PostProcessing_Effect.Techniques["Base"];


        //Hologram Effect
        public static readonly Effect Hologram_Effect = Globals.content.Load<Effect>("Shaders/Hologram/HologramEffect");
        public static readonly EffectParameter HologramEffectParam_World = Hologram_Effect.Parameters["World"];
        public static readonly EffectParameter HologramEffectParam_WorldViewProj = Hologram_Effect.Parameters["WorldViewProj"];

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

        public static readonly Effect ScreenSpace_Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

        public static readonly EffectParameter ScreenSpaceEffectParam_SSAOMap = ScreenSpace_Effect.Parameters["SSAOMap"];
        public static readonly EffectParameter ScreenSpaceEffectParam_NormalMap = ScreenSpace_Effect.Parameters["NormalMap"];
        public static readonly EffectParameter ScreenSpaceEffectParam_DepthMap = ScreenSpace_Effect.Parameters["DepthMap"];
        public static readonly EffectParameter ScreenSpaceEffectParam_CameraPosition = ScreenSpace_Effect.Parameters["CameraPosition"];
        public static readonly EffectParameter ScreenSpaceEffectParam_InverseViewProjection = ScreenSpace_Effect.Parameters["InverseViewProjection"];
        public static readonly EffectParameter ScreenSpaceEffectParam_Projection = ScreenSpace_Effect.Parameters["Projection"];
        public static readonly EffectParameter ScreenSpaceEffectParam_ViewProjection = ScreenSpace_Effect.Parameters["ViewProjection"];

        public static readonly EffectParameter ScreenSpaceEffectParam_FalloffMin = ScreenSpace_Effect.Parameters["FalloffMin"];
        public static readonly EffectParameter ScreenSpaceEffectParam_FalloffMax = ScreenSpace_Effect.Parameters["FalloffMax"];
        public static readonly EffectParameter ScreenSpaceEffectParam_Samples = ScreenSpace_Effect.Parameters["Samples"];
        public static readonly EffectParameter ScreenSpaceEffectParam_Strength = ScreenSpace_Effect.Parameters["Strength"];
        public static readonly EffectParameter ScreenSpaceEffectParam_SampleRadius = ScreenSpace_Effect.Parameters["SampleRadius"];
        public static readonly EffectParameter ScreenSpaceEffectParam_InverseResolution = ScreenSpace_Effect.Parameters["InverseResolution"];
        public static readonly EffectParameter ScreenSpaceEffectParam_AspectRatio = ScreenSpace_Effect.Parameters["AspectRatio"];
        public static readonly EffectParameter ScreenSpaceEffectParam_FrustumCorners = ScreenSpace_Effect.Parameters["FrustumCorners"];

        public static readonly EffectTechnique ScreenSpaceEffectTechnique_SSAO = ScreenSpace_Effect.Techniques["SSAO"];
        public static readonly EffectTechnique ScreenSpaceEffectTechnique_BlurHorizontal = ScreenSpace_Effect.Techniques["BilateralHorizontal"];
        public static readonly EffectTechnique ScreenSpaceEffectTechnique_BlurVertical = ScreenSpace_Effect.Techniques["BilateralVertical"];


        //Gaussian Blur
        public static class GaussianBlur
        {
            public static readonly Effect GaussianBlur_Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
            public static readonly EffectParameter GaussianBlurEffectParam_InverseResolution = GaussianBlur_Effect.Parameters["InverseResolution"];
            public static readonly EffectParameter GaussianBlurEffectParam_TargetMap = GaussianBlur_Effect.Parameters["TargetMap"];
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

        public static class DeferredDirectionalLight
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredDirectionalLight");

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

        //Point Light


        public static void Load(ContentManager content)
        {
        }
    }

}
