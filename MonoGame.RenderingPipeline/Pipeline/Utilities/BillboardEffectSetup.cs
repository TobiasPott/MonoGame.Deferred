using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Utilities
{
    public class BillboardEffectSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_Texture { get; protected set; }
        public EffectParameter Param_IdColor { get; protected set; }

        public EffectTechnique Technique_Billboard { get; protected set; }
        public EffectTechnique Technique_Id { get; protected set; }


        public BillboardEffectSetup(string shaderPath = "Shaders/Editor/BillboardEffect") : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Technique_Billboard = Effect.Techniques["Billboard"];
            Technique_Id = Effect.Techniques["Id"];

            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_Texture = Effect.Parameters[Names.Sampler("Texture")];
            Param_IdColor = Effect.Parameters["IdColor"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
