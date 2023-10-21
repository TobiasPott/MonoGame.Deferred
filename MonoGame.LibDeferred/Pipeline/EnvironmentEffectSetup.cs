using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Pipeline
{
    // Deferred Environment
    public class EnvironmentEffectSetup : EffectSetupBase
    {
        public Effect Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredEnvironmentMap");

        public EffectTechnique Technique_Sky { get; protected set; }
        public EffectTechnique Technique_Basic { get; protected set; }

        public EffectPass Pass_Sky { get; protected set; }
        public EffectPass Pass_Basic { get; protected set; }

        // Environment
        public EffectParameter Param_AlbedoMap { get; protected set; }
        public EffectParameter Param_NormalMap { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }

        public EffectParameter Param_FrustumCorners { get; protected set; }
        public EffectParameter Param_SSRMap { get; protected set; }
        public EffectParameter Param_ReflectionCubeMap { get; protected set; }
        public EffectParameter Param_Resolution { get; protected set; }
        public EffectParameter Param_FireflyReduction { get; protected set; }
        public EffectParameter Param_FireflyThreshold { get; protected set; }
        public EffectParameter Param_TransposeView { get; protected set; }
        public EffectParameter Param_SpecularStrength { get; protected set; }
        public EffectParameter Param_SpecularStrengthRcp { get; protected set; }
        public EffectParameter Param_DiffuseStrength { get; protected set; }
        public EffectParameter Param_CameraPositionWS { get; protected set; }
        public EffectParameter Param_Time { get; protected set; }

        // SDF
        public EffectParameter Param_VolumeTex { get; protected set; }
        public EffectParameter Param_VolumeTexSize { get; protected set; }
        public EffectParameter Param_VolumeTexResolution { get; protected set; }
        public EffectParameter Param_InstanceInverseMatrix { get; protected set; }
        public EffectParameter Param_InstanceScale { get; protected set; }
        public EffectParameter Param_InstanceSDFIndex { get; protected set; }
        public EffectParameter Param_InstancesCount { get; protected set; }

        public EffectParameter Param_UseSDFAO { get; protected set; }


        public EnvironmentEffectSetup(string shaderPath = "Shaders/Deferred/DeferredEnvironmentMap") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Technique_Sky = Effect.Techniques["Sky"];
            Technique_Basic = Effect.Techniques["Basic"];

            Pass_Sky = Effect.Techniques["Sky"].Passes[0];
            Pass_Basic = Effect.Techniques["Basic"].Passes[0];

            Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
            Param_NormalMap = Effect.Parameters["NormalMap"];
            Param_DepthMap = Effect.Parameters["DepthMap"];

            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_SSRMap = Effect.Parameters["ReflectionMap"];
            Param_ReflectionCubeMap = Effect.Parameters["ReflectionCubeMap"];
            Param_Resolution = Effect.Parameters["Resolution"];
            Param_FireflyReduction = Effect.Parameters["FireflyReduction"];
            Param_FireflyThreshold = Effect.Parameters["FireflyThreshold"];
            Param_TransposeView = Effect.Parameters["TransposeView"];
            Param_SpecularStrength = Effect.Parameters["EnvironmentMapSpecularStrength"];
            Param_SpecularStrengthRcp = Effect.Parameters["EnvironmentMapSpecularStrengthRcp"];
            Param_DiffuseStrength = Effect.Parameters["EnvironmentMapDiffuseStrength"];
            Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];
            Param_Time = Effect.Parameters["Time"];


            Param_VolumeTex = Effect.Parameters["VolumeTex"];
            Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
            Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];
            Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            Param_InstanceScale = Effect.Parameters["InstanceScale"];
            Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            Param_InstancesCount = Effect.Parameters["InstancesCount"];

            Param_UseSDFAO = Effect.Parameters["UseSDFAO"];
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }


}