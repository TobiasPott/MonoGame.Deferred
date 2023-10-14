using DeferredEngine.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    public static partial class Shaders
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


        public static class PostProcssing
        {
            //Vignette and CA
            public static readonly Effect Effect = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
            public static readonly EffectParameter Param_ScreenTexture = Effect.Parameters["ScreenTexture"];
            public static readonly EffectParameter Param_ChromaticAbberationStrength = Effect.Parameters["ChromaticAbberationStrength"];
            public static readonly EffectParameter Param_SCurveStrength = Effect.Parameters["SCurveStrength"];
            public static readonly EffectParameter Param_WhitePoint = Effect.Parameters["WhitePoint"];
            public static readonly EffectParameter Param_PowExposure = Effect.Parameters["PowExposure"];

            public static readonly EffectTechnique Technique_VignetteChroma = Effect.Techniques["VignetteChroma"];
            public static readonly EffectTechnique Technique_Base = Effect.Techniques["Base"];
        }



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

        public static class SSAO
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

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

            public static void SetCameraAndMatrices(Vector3 cameraPosition, PipelineMatrices matrices)
            {
                Shaders.SSAO.Param_InverseViewProjection.SetValue(matrices.InverseViewProjection);
                Shaders.SSAO.Param_Projection.SetValue(matrices.Projection);
                Shaders.SSAO.Param_ViewProjection.SetValue(matrices.ViewProjection);

                Shaders.SSAO.Param_CameraPosition.SetValue(cameraPosition);
            }

        }

        //Deferred Compose & Clear
        public static class Deferred
        {
            public static readonly Effect Effect_Clear = Globals.content.Load<Effect>("Shaders/Deferred/DeferredClear");
            public static readonly Effect Effect_Compose = Globals.content.Load<Effect>("Shaders/Deferred/DeferredCompose");

            public static readonly EffectParameter Param_ColorMap = Effect_Compose.Parameters["colorMap"];
            public static readonly EffectParameter Param_NormalMap = Effect_Compose.Parameters["normalMap"];
            public static readonly EffectParameter Param_diffuseLightMap = Effect_Compose.Parameters["diffuseLightMap"];
            public static readonly EffectParameter Param_specularLightMap = Effect_Compose.Parameters["specularLightMap"];
            public static readonly EffectParameter Param_volumeLightMap = Effect_Compose.Parameters["volumeLightMap"];
            public static readonly EffectParameter Param_HologramMap = Effect_Compose.Parameters["HologramMap"];
            public static readonly EffectParameter Param_SSAOMap = Effect_Compose.Parameters["SSAOMap"];
            public static readonly EffectParameter Param_LinearMap = Effect_Compose.Parameters["LinearMap"];
            public static readonly EffectParameter Param_SSRMap = Effect_Compose.Parameters["SSRMap"];
            public static readonly EffectParameter Param_UseSSAO = Effect_Compose.Parameters["useSSAO"];

            public static readonly EffectTechnique Technique_NonLinear = Effect_Compose.Techniques["TechniqueNonLinear"];
            public static readonly EffectTechnique Technique_Linear = Effect_Compose.Techniques["TechniqueLinear"];
        }

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

            public static void SetGBufferParams(GBufferTarget gBufferTarget)
            {
                Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
                Param_NormalMap.SetValue(gBufferTarget.Normal);
                Param_DepthMap.SetValue(gBufferTarget.Depth);
            }

        }


        //Point light
        public static class DeferredPointLight
        {
            public static Effect Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredPointLight");

            public static EffectTechnique Technique_Unshadowed = Effect.Techniques["Unshadowed"];
            public static EffectTechnique Technique_UnshadowedVolumetric = Effect.Techniques["UnshadowedVolume"];
            public static EffectTechnique Technique_Shadowed = Effect.Techniques["Shadowed"];
            public static EffectTechnique Technique_ShadowedSDF = Effect.Techniques["ShadowedSDF"];
            public static EffectTechnique Technique_ShadowedVolumetric = Effect.Techniques["ShadowedVolume"];
            public static EffectTechnique Technique_WriteStencil = Effect.Techniques["WriteStencilMask"];

            public static EffectParameter Param_ShadowMap = Effect.Parameters["ShadowMap"];

            public static EffectParameter Param_Resolution = Effect.Parameters["Resolution"];
            public static EffectParameter Param_WorldView = Effect.Parameters["WorldView"];
            public static EffectParameter Param_WorldViewProjection = Effect.Parameters["WorldViewProj"];
            public static EffectParameter Param_InverseView = Effect.Parameters["InverseView"];

            public static EffectParameter Param_LightPosition = Effect.Parameters["lightPosition"];
            public static EffectParameter Param_LightColor = Effect.Parameters["lightColor"];
            public static EffectParameter Param_LightRadius = Effect.Parameters["lightRadius"];
            public static EffectParameter Param_LightIntensity = Effect.Parameters["lightIntensity"];
            public static EffectParameter Param_ShadowMapSize = Effect.Parameters["ShadowMapSize"];
            public static EffectParameter Param_ShadowMapRadius = Effect.Parameters["ShadowMapRadius"];
            public static EffectParameter Param_Inside = Effect.Parameters["inside"];
            public static EffectParameter Param_Time = Effect.Parameters["Time"];
            public static EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static EffectParameter Param_LightVolumeDensity = Effect.Parameters["lightVolumeDensity"];

            public static EffectParameter Param_VolumeTex = Effect.Parameters["VolumeTex"];
            public static EffectParameter Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
            public static EffectParameter Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];

            public static EffectParameter Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            public static EffectParameter Param_InstanceScale = Effect.Parameters["InstanceScale"];
            public static EffectParameter Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            public static EffectParameter Param_InstancesCount = Effect.Parameters["InstancesCount"];

            public static EffectParameter Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
            public static EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];

        }

    }

}
