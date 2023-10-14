using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    //Gaussian Blur
    public class GaussianBlurFxEffectSetup : EffectSetupBase
    {

        public Effect Effect { get; protected set; }

        public EffectTechnique Technique_GaussianBlur { get; protected set; }


        public EffectPass Pass_Horizontal { get; protected set; }
        public EffectPass Pass_Vertical { get; protected set; }

        // Parameters
        public EffectParameter Param_InverseResolution { get; protected set; }
        public EffectParameter Param_TargetMap { get; protected set; }


        public GaussianBlurFxEffectSetup(string shaderPath = "Shaders/ScreenSpace/GaussianBlur")
              : base(shaderPath)
        {
            Effect = ShaderGlobals.content.Load<Effect>(shaderPath);

            Technique_GaussianBlur = Effect.Techniques["GaussianBlur"];

            Pass_Horizontal = Technique_GaussianBlur.Passes["Horizontal"];
            Pass_Vertical = Technique_GaussianBlur.Passes["Vertical"];

            // Parameters
            Param_InverseResolution = Effect.Parameters["InverseResolution"];
            Param_TargetMap = Effect.Parameters["TargetMap"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
