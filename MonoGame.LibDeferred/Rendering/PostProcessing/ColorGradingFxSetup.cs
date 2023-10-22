using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Rendering.PostProcessing
{
    // Color Grading Effect
    public class ColorGradingFxSetup : BaseFxSetup
    {

        public Effect Effect { get; protected set; }

        protected EffectTechnique Technique_ApplyLUT { get; set; }
        protected EffectTechnique Technique_CreateLUT { get; set; }


        public EffectPass Pass_ApplyLUT { get; protected set; }
        public EffectPass Pass_CreateLUT { get; protected set; }

        public EffectParameter Param_Size { get; protected set; }
        public EffectParameter Param_SizeRoot { get; protected set; }
        public EffectParameter Param_InputTexture { get; protected set; }
        public EffectParameter Param_LUT { get; protected set; }



        public ColorGradingFxSetup(string shaderPath = "Shaders/PostProcessing/ColorGrading")
              : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Technique_ApplyLUT = Effect.Techniques["ApplyLUT"];
            Technique_CreateLUT = Effect.Techniques["CreateLUT"];

            Pass_ApplyLUT = Technique_ApplyLUT.Passes[0];
            Pass_CreateLUT = Technique_CreateLUT.Passes[0];

            Param_Size = Effect.Parameters["Size"];
            Param_SizeRoot = Effect.Parameters["SizeRoot"];
            Param_InputTexture = Effect.Parameters["InputTexture"];
            Param_LUT = Effect.Parameters["LUT"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}