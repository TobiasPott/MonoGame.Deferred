using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    public class IdAndOutlineffectSetup : EffectSetupBase
    {
        public Effect Effect { get; protected set; }

        public EffectPass Pass_Id { get; protected set; }
        public EffectPass Pass_Outline { get; protected set; }

        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_ColorId { get; protected set; }
        public EffectParameter Param_OutlineSize { get; protected set; }
        public EffectParameter Param_World { get; protected set; }


        public IdAndOutlineffectSetup(string shaderPath = "Shaders/Editor/IdRender") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>("Shaders/Editor/IdRender");
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_ColorId = Effect.Parameters["ColorId"];
            Param_OutlineSize = Effect.Parameters["OutlineSize"];
            Param_World = Effect.Parameters["World"];

            Pass_Id = Effect.Techniques["DrawId"].Passes[0];
            Pass_Outline = Effect.Techniques["DrawOutline"].Passes[0];
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}

