using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Utilities
{

    public class DecalEffectSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectPass Pass_Decal { get; protected set; }
        public EffectPass Pass_Outline { get; protected set; }

        public EffectParameter Param_DecalMap { get; protected set; }
        public EffectParameter Param_WorldView { get; protected set; }
        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_InverseWorldView { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }


        public DecalEffectSetup(string shaderPath = "Shaders/Deferred/DeferredDecal")
              : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Pass_Decal = Effect.Techniques["Decal"].Passes[0];
            Pass_Outline = Effect.Techniques["Outline"].Passes[0];

            Param_WorldView = Effect.Parameters["WorldView"];
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_InverseWorldView = Effect.Parameters["InverseWorldView"];
            Param_FarClip = Effect.Parameters["FarClip"];

            Param_DecalMap = Effect.Parameters[Names.Sampler("DecalMap")];
            Param_DepthMap = Effect.Parameters[Names.Sampler("DepthMap")];
        }



        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
