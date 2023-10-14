using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    // Temporal Anti-Aliasing Effect
    public class TemporalAAFxEffectSetup : EffectSetupBase
    {

        public Effect Effect { get; protected set; }


        public EffectPass Pass_TemporalAA { get; protected set; }
        public EffectPass Pass_TonemapInverse { get; protected set; }

        protected EffectTechnique Technique_TemporalAA { get; set; }
        protected EffectTechnique Technique_TonemapInverse { get; set; }

        public EffectParameter Param_AccumulationMap { get; protected set; }
        public EffectParameter Param_UpdateMap { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }
        public EffectParameter Param_CurrentToPrevious { get; protected set; }
        public EffectParameter Param_Resolution { get; protected set; }
        public EffectParameter Param_FrustumCorners { get; protected set; }
        public EffectParameter Param_UseTonemap { get; protected set; }


        public TemporalAAFxEffectSetup(string shaderPath = "Shaders/TemporalAntiAliasing/TemporalAntiAliasing")
              : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Technique_TemporalAA = Effect.Techniques["TemporalAntialiasing"];
            Technique_TonemapInverse = Effect.Techniques["InverseTonemap"];

            Pass_TemporalAA = Technique_TemporalAA.Passes[0];
            Pass_TonemapInverse = Technique_TonemapInverse.Passes[0];

            Param_AccumulationMap = Effect.Parameters["AccumulationMap"];
            Param_UpdateMap = Effect.Parameters["UpdateMap"];
            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_CurrentToPrevious = Effect.Parameters["CurrentToPrevious"];
            Param_Resolution = Effect.Parameters["Resolution"];
            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_UseTonemap = Effect.Parameters["UseTonemap"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
