using DeferredEngine.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class SSReflectionFxSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectParameter Param_DepthMap { get; protected set; }
        public EffectParameter Param_NormalMap { get; protected set; }
        public EffectParameter Param_SourceMap { get; protected set; }
        public EffectParameter Param_Resolution { get; protected set; }
        public EffectParameter Param_Projection { get; protected set; }
        public EffectParameter Param_Time { get; protected set; }
        public EffectParameter Param_FrustumCorners { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }
        public EffectParameter Param_NoiseMap { get; protected set; }
        public EffectParameter Param_Samples { get; protected set; }
        public EffectParameter Param_SecondarySamples { get; protected set; }

        public EffectTechnique Technique_Default { get; protected set; }
        public EffectTechnique Technique_Taa { get; protected set; }

        public SSReflectionFxSetup(string shaderPath = "Shaders/ScreenSpace/ScreenSpaceReflections") : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_NormalMap = Effect.Parameters["NormalMap"];
            Param_SourceMap = Effect.Parameters["SourceMap"];
            Param_Resolution = Effect.Parameters["Resolution"];
            Param_Projection = Effect.Parameters["Projection"];
            Param_Time = Effect.Parameters["Time"];
            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_FarClip = Effect.Parameters["FarClip"];
            Param_NoiseMap = Effect.Parameters["NoiseMap"];

            Param_Samples = Effect.Parameters["Samples"];
            Param_SecondarySamples = Effect.Parameters["SecondarySamples"];

            Technique_Default = Effect.Techniques["Default"];
            Technique_Taa = Effect.Techniques["TAA"];

            // set default noise map
            Param_NoiseMap.SetValue(StaticAssets.Instance.NoiseMap);
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }

    }

}
