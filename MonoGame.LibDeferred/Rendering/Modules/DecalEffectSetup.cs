using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    public class DecalEffectSetup : EffectSetupBase
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
              : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Pass_Decal = Effect.Techniques["Decal"].Passes[0];
            Pass_Outline = Effect.Techniques["Outline"].Passes[0];

            Param_DecalMap = Effect.Parameters["DecalMap"];
            Param_WorldView = Effect.Parameters["WorldView"];
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_InverseWorldView = Effect.Parameters["InverseWorldView"];
            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_FarClip = Effect.Parameters["FarClip"];
        }



        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
