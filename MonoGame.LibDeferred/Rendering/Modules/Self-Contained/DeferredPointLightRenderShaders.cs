using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.DeferredLighting
{
    internal static class DeferredPointLightRenderShaders
    {

        public static EffectParameter Param_Resolution;
        public static EffectParameter Param_InverseView;
        public static EffectParameter Param_FarClip;
        private static bool EffectLoaded = false;
        private static Effect Effect;

        private static EffectTechnique Technique_Unshadowed;
        private static EffectTechnique Technique_UnshadowedVolumetric;
        private static EffectTechnique Technique_ShadowedSDF;
        private static EffectTechnique Technique_Shadowed;
        private static EffectTechnique Technique_ShadowedVolumetric;
        private static EffectTechnique Technique_WriteStencil;

        private static EffectParameter Param_ShadowMap;
        private static EffectParameter Param_WorldView;
        private static EffectParameter Param_WorldViewProjection;

        private static EffectParameter Param_LightPosition;
        private static EffectParameter Param_LightColor;
        private static EffectParameter Param_LightRadius;
        private static EffectParameter Param_LightIntensity;
        private static EffectParameter Param_ShadowMapSize;
        private static EffectParameter Param_ShadowMapRadius;
        private static EffectParameter Param_Inside;
        private static EffectParameter Param_Time;
        private static EffectParameter Param_LightVolumeDensity;

        private static EffectParameter Param_VolumeTex;
        private static EffectParameter Param_VolumeTexSize;
        private static EffectParameter Param_VolumeTexResolution;

        private static EffectParameter Param_InstanceInverseMatrix;
        private static EffectParameter Param_InstanceScale;
        private static EffectParameter Param_InstanceSDFIndex;
        private static EffectParameter Param_InstancesCount;

        private static EffectParameter Param_AlbedoMap;
        private static EffectParameter Param_NormalMap;
        private static EffectParameter Param_DepthMap;

        private static void LoadShader(ContentManager content, string shaderPath = "Shaders/Deferred/DeferredPointLight")
        {
            if (!EffectLoaded)
            {
                Effect = content.Load<Effect>(shaderPath);

                Technique_Unshadowed = Effect.Techniques["Unshadowed"];
                Technique_UnshadowedVolumetric = Effect.Techniques["UnshadowedVolume"];
                Technique_Shadowed = Effect.Techniques["Shadowed"];
                Technique_ShadowedSDF = Effect.Techniques["ShadowedSDF"];
                Technique_ShadowedVolumetric = Effect.Techniques["ShadowedVolume"];
                Technique_WriteStencil = Effect.Techniques["WriteStencilMask"];

                Param_ShadowMap = Effect.Parameters["ShadowMap"];

                Param_Resolution = Effect.Parameters["Resolution"];
                Param_WorldView = Effect.Parameters["WorldView"];
                Param_WorldViewProjection = Effect.Parameters["WorldViewProj"];
                Param_InverseView = Effect.Parameters["InverseView"];

                Param_LightPosition = Effect.Parameters["lightPosition"];
                Param_LightColor = Effect.Parameters["lightColor"];
                Param_LightRadius = Effect.Parameters["lightRadius"];
                Param_LightIntensity = Effect.Parameters["lightIntensity"];
                Param_ShadowMapSize = Effect.Parameters["ShadowMapSize"];
                Param_ShadowMapRadius = Effect.Parameters["ShadowMapRadius"];
                Param_Inside = Effect.Parameters["inside"];
                Param_Time = Effect.Parameters["Time"];
                Param_FarClip = Effect.Parameters["FarClip"];
                Param_LightVolumeDensity = Effect.Parameters["lightVolumeDensity"];

                Param_VolumeTex = Effect.Parameters["VolumeTex"];
                Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
                Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];
                Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
                Param_InstanceScale = Effect.Parameters["InstanceScale"];
                Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
                Param_InstancesCount = Effect.Parameters["InstancesCount"];

                Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
                Param_NormalMap = Effect.Parameters["NormalMap"];
                Param_DepthMap = Effect.Parameters["DepthMap"];
            }
        }
    }
}