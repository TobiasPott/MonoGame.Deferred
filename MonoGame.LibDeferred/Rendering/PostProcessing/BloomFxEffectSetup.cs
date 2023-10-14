using DeferredEngine.Recources;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.PostProcessing
{
    public partial class BloomFx
    {
        public class BloomFxEffectSetup : EffectSetupBase
        {

            public Effect Effect { get; protected set; }

            protected EffectTechnique Technique_Extract { get; set; }
            protected EffectTechnique Technique_ExtractLuminance { get; set; }
            protected EffectTechnique Technique_Downsample { get; set; }
            protected EffectTechnique Technique_Upsample { get; set; }


            public EffectPass Pass_Extract { get; protected set; }
            public EffectPass Pass_ExtractLuminance { get; protected set; }
            public EffectPass Pass_Downsample { get; protected set; }
            public EffectPass Pass_Upsample { get; protected set; }

            public EffectParameter Param_ScreenTexture { get; protected set; }
            public EffectParameter Param_InverseResolution { get; protected set; }
            public EffectParameter Param_Radius { get; protected set; }
            public EffectParameter Param_Strength { get; protected set; }
            public EffectParameter Param_StreakLength { get; protected set; }
            public EffectParameter Param_Threshold { get; protected set; }



            public BloomFxEffectSetup(string shaderPath = "Shaders/BloomFilter/Bloom")
                  : base(shaderPath)
            {
                Effect = ShaderGlobals.content.Load<Effect>(shaderPath);

                Technique_Extract = Effect.Techniques["Extract"];
                Technique_ExtractLuminance = Effect.Techniques["ExtractLuminance"];
                Technique_Downsample = Effect.Techniques["Downsample"];
                Technique_Upsample = Effect.Techniques["Upsample"];

                Pass_Extract = Technique_Extract.Passes[0];
                Pass_ExtractLuminance = Technique_ExtractLuminance.Passes[0];
                Pass_Downsample = Technique_Downsample.Passes[0];
                Pass_Upsample = Technique_Upsample.Passes[0];

                Param_InverseResolution = Effect.Parameters["InverseResolution"];
                Param_Radius = Effect.Parameters["Radius"];
                Param_Strength = Effect.Parameters["Strength"];
                Param_StreakLength = Effect.Parameters["StreakLength"];
                Param_Threshold = Effect.Parameters["Threshold"];
                Param_ScreenTexture = Effect.Parameters["ScreenTexture"];
            }

            public override void Dispose()
            {
                Effect?.Dispose();
            }
        }
    }
}
