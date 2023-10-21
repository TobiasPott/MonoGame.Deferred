using DeferredEngine.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    //Hologram Effect
    public class HologramEffectSetup : EffectSetupBase
    {
        private static HologramEffectSetup _instance = null;
        public static HologramEffectSetup Instance
        {
            get
            {
                if (_instance == null) _instance = new HologramEffectSetup();
                return _instance;
            }
        }


        public Effect Effect { get; protected set; }

        // Parameters
        public EffectParameter Param_World { get; protected set; }
        public EffectParameter Param_WorldViewProj { get; protected set; }


        public HologramEffectSetup(string shaderPath = "Shaders/Hologram/HologramEffect")
              : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            // Parameters
            Param_World = Effect.Parameters["World"];
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
