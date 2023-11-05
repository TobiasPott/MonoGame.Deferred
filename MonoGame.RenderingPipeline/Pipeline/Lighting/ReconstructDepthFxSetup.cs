using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Lighting
{

    public class ReconstructDepthFxSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectParameter Param_DepthMap { get; protected set; }
        public EffectParameter Param_Projection { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }

        public ReconstructDepthFxSetup(string shaderPath = "Shaders/ScreenSpace/ReconstructDepth") : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_Projection = Effect.Parameters["Projection"];
            Param_FarClip = Effect.Parameters["FarClip"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
