using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Pipeline.Utilities
{

    public class HelperGeometryEffectSetup : BaseFxSetup
    {

        //Lines
        public Effect Effect { get; protected set; }

        public EffectPass Pass_VertexColor { get; protected set; }
        public EffectPass Pass_GlobalColor { get; protected set; }

        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_GlobalColor { get; protected set; }


        public HelperGeometryEffectSetup(string shaderPath = "Shaders/Editor/LineEffect")
              : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Pass_VertexColor = Effect.Techniques["VertexColor"].Passes[0];
            Pass_GlobalColor = Effect.Techniques["GlobalColor"].Passes[0];

            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_GlobalColor = Effect.Parameters["GlobalColor"];
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }
}