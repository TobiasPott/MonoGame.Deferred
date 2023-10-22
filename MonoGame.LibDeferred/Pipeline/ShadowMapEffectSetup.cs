using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{
    // Shadow Map
    public class ShadowMapEffectSetup : BaseFxSetup
    {
        public Effect Effect;

        public EffectPass Pass_LinearPass { get; protected set; } // Linear = VS Depth -> used for directional lights
        public EffectPass Pass_DistancePass { get; protected set; } // Distance = distance(pixel, light) -> used for omnidirectional lights
        public EffectPass Pass_DistanceAlphaPass { get; protected set; }

        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_WorldView { get; protected set; }
        public EffectParameter Param_World { get; protected set; }
        public EffectParameter Param_LightPositionWS { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }
        public EffectParameter Param_SizeBias { get; protected set; }
        public EffectParameter Param_MaskTexture { get; protected set; }

        public ShadowMapEffectSetup(string shaderPath = "Shaders/Shadow/ShadowMap") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>("Shaders/Shadow/ShadowMap");

            Pass_LinearPass = Effect.Techniques["DrawLinearDepth"].Passes[0]; // Linear = VS Depth -> used for directional lights
            Pass_DistancePass = Effect.Techniques["DrawDistanceDepth"].Passes[0]; // Distance = distance(pixel, light) -> used for omnidirectional lights
            Pass_DistanceAlphaPass = Effect.Techniques["DrawDistanceDepthAlpha"].Passes[0];

            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_WorldView = Effect.Parameters["WorldView"];
            Param_World = Effect.Parameters["World"];
            Param_LightPositionWS = Effect.Parameters["LightPositionWS"];
            Param_FarClip = Effect.Parameters["FarClip"];
            Param_SizeBias = Effect.Parameters["SizeBias"];
            Param_MaskTexture = Effect.Parameters["MaskTexture"];


        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }
}
