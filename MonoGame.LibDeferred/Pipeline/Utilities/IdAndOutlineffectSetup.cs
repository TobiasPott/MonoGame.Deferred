using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Pipeline.Utilities
{
    public class IdAndOutlineEffectSetup : BaseFxSetup
    {
        private static IdAndOutlineEffectSetup _instance = null;
        public static IdAndOutlineEffectSetup Instance
        {
            get
            {
                if (_instance == null) _instance = new IdAndOutlineEffectSetup();
                return _instance;
            }
        }


        public Effect Effect { get; protected set; }

        public EffectPass Pass_Id { get; protected set; }
        public EffectPass Pass_Outline { get; protected set; }

        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_ColorId { get; protected set; }
        public EffectParameter Param_OutlineSize { get; protected set; }
        public EffectParameter Param_World { get; protected set; }


        public IdAndOutlineEffectSetup(string shaderPath = "Shaders/Editor/IdRender") : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);
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

