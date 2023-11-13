using DeferredEngine.Pipeline;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    public class SSAmbientOcclusionFxSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectParameter Param_SSAOMap { get; protected set; }
        public EffectParameter Param_NormalMap { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }

        public EffectParameter Param_FalloffMin { get; protected set; }
        public EffectParameter Param_FalloffMax { get; protected set; }
        public EffectParameter Param_Samples { get; protected set; }
        public EffectParameter Param_Strength { get; protected set; }
        public EffectParameter Param_SampleRadius { get; protected set; }
        public EffectParameter Param_InverseResolution { get; protected set; }
        public EffectParameter Param_AspectRatio { get; protected set; }
        public EffectParameter Param_FrustumCorners { get; protected set; }

        public EffectTechnique Technique_SSAO { get; protected set; }
        public EffectTechnique Technique_BlurHorizontal { get; protected set; }
        public EffectTechnique Technique_BlurVertical { get; protected set; }


        public SSAmbientOcclusionFxSetup(string shaderPath = "Shaders/ScreenSpace/ScreenSpaceAO") : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Param_SSAOMap = Effect.Parameters[Names.Sampler("SSAOMap")];
            Param_NormalMap = Effect.Parameters[Names.Sampler("NormalMap")];
            Param_DepthMap = Effect.Parameters[Names.Sampler("DepthMap")];

            Param_FalloffMin = Effect.Parameters["FalloffMin"];
            Param_FalloffMax = Effect.Parameters["FalloffMax"];
            Param_Samples = Effect.Parameters["Samples"];
            Param_Strength = Effect.Parameters["Strength"];
            Param_SampleRadius = Effect.Parameters["SampleRadius"];
            Param_InverseResolution = Effect.Parameters["InverseResolution"];
            Param_AspectRatio = Effect.Parameters["AspectRatio"];
            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];

            Technique_SSAO = Effect.Techniques["SSAO"];
            Technique_BlurHorizontal = Effect.Techniques["BilateralHorizontal"];
            Technique_BlurVertical = Effect.Techniques["BilateralVertical"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }


}
