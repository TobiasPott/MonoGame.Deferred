using DeferredEngine.Pipeline;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Recources
{
    public class PostProcssingFxSetup : BaseFxSetup
    {
        //Vignette and CA
        public Effect Effect { get; protected set; }
        public EffectParameter Param_ScreenTexture { get; protected set; }
        public EffectParameter Param_ChromaticAbberationStrength { get; protected set; }
        public EffectParameter Param_SCurveStrength { get; protected set; }
        public EffectParameter Param_WhitePoint { get; protected set; }
        public EffectParameter Param_PowExposure { get; protected set; }

        public EffectTechnique Technique_VignetteChroma { get; protected set; }
        public EffectTechnique Technique_Base { get; protected set; }


        public PostProcssingFxSetup(string shaderPath = "shaders/postprocessing/postprocessing") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Effect = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
            Param_ScreenTexture = Effect.Parameters["ScreenTexture"];
            Param_ChromaticAbberationStrength = Effect.Parameters["ChromaticAbberationStrength"];
            Param_SCurveStrength = Effect.Parameters["SCurveStrength"];
            Param_WhitePoint = Effect.Parameters["WhitePoint"];
            Param_PowExposure = Effect.Parameters["PowExposure"];

            Technique_VignetteChroma = Effect.Techniques["VignetteChroma"];
            Technique_Base = Effect.Techniques["Base"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}