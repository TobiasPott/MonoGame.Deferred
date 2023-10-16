using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    // Forward
    public class ForwardEffectSetup : EffectSetupBase
    {
        public Effect Effect { get; protected set; }

        public EffectPass Pass_Default { get; protected set; }

        public EffectParameter Param_World { get; protected set; }
        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_WorldViewIT { get; protected set; }
        public EffectParameter Param_LightAmount { get; protected set; }
        public EffectParameter Param_LightPositionWS { get; protected set; }
        public EffectParameter Param_LightRadius { get; protected set; }
        public EffectParameter Param_LightIntensity { get; protected set; }
        public EffectParameter Param_LightColor { get; protected set; }
        public EffectParameter Param_CameraPositionWS { get; protected set; }

        public ForwardEffectSetup(string shaderPath = "Shaders/forward/forward") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Pass_Default = Effect.Techniques["Default"].Passes[0];

            Param_World = Effect.Parameters["World"];
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_WorldViewIT = Effect.Parameters["WorldViewIT"];
            Param_LightAmount = Effect.Parameters["LightAmount"];
            Param_LightPositionWS = Effect.Parameters["LightPositionWS"];
            Param_LightRadius = Effect.Parameters["LightRadius"];
            Param_LightIntensity = Effect.Parameters["LightIntensity"];
            Param_LightColor = Effect.Parameters["LightColor"];
            Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }
}