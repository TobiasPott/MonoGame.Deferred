using DeferredEngine.Recources;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Lighting
{

    public class PointLightEffectSetup : EffectSetupBase
    {
        public Effect Effect { get; protected set; }

        public EffectTechnique Technique_Unshadowed { get; protected set; }
        public EffectTechnique Technique_UnshadowedVolumetric { get; protected set; }
        public EffectTechnique Technique_Shadowed { get; protected set; }
        public EffectTechnique Technique_ShadowedSDF { get; protected set; }
        public EffectTechnique Technique_ShadowedVolumetric { get; protected set; }
        public EffectTechnique Technique_WriteStencil { get; protected set; }


        public EffectParameter Param_ShadowMap { get; protected set; }

        public EffectParameter Param_Resolution { get; protected set; }
        public EffectParameter Param_WorldView { get; protected set; }
        public EffectParameter Param_WorldViewProjection { get; protected set; }
        public EffectParameter Param_InverseView { get; protected set; }

        public EffectParameter Param_LightPosition { get; protected set; }
        public EffectParameter Param_LightColor { get; protected set; }
        public EffectParameter Param_LightRadius { get; protected set; }
        public EffectParameter Param_LightIntensity { get; protected set; }
        public EffectParameter Param_ShadowMapSize { get; protected set; }
        public EffectParameter Param_ShadowMapRadius { get; protected set; }
        public EffectParameter Param_Inside { get; protected set; }
        public EffectParameter Param_Time { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }
        public EffectParameter Param_LightVolumeDensity { get; protected set; }

        public EffectParameter Param_VolumeTex { get; protected set; }
        public EffectParameter Param_VolumeTexSize { get; protected set; }
        public EffectParameter Param_VolumeTexResolution { get; protected set; }

        public EffectParameter Param_InstanceInverseMatrix { get; protected set; }
        public EffectParameter Param_InstanceScale { get; protected set; }
        public EffectParameter Param_InstanceSDFIndex { get; protected set; }
        public EffectParameter Param_InstancesCount { get; protected set; }

        public EffectParameter Param_AlbedoMap { get; protected set; }
        public EffectParameter Param_NormalMap { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }

        public PointLightEffectSetup(string shaderPath = "Shaders/Deferred/DeferredPointLight") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredPointLight");

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

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
