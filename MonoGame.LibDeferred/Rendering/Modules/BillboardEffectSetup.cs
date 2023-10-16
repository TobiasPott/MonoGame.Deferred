using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class BillboardEffectSetup : EffectSetupBase
    {
        public Effect Effect { get; protected set; }

        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_WorldView { get; protected set; }
        public EffectParameter Param_AspectRatio { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }
        public EffectParameter Param_Texture { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }
        public EffectParameter Param_IdColor { get; protected set; }

        public EffectTechnique Technique_Billboard { get; protected set; }
        public EffectTechnique Technique_Id { get; protected set; }


        public BillboardEffectSetup(string shaderPath = "Shaders/Editor/BillboardEffect") : base(shaderPath)
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Technique_Billboard = Effect.Techniques["Billboard"];
            Technique_Id = Effect.Techniques["Id"];

            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_WorldView = Effect.Parameters["WorldView"];
            Param_AspectRatio = Effect.Parameters["AspectRatio"];
            Param_FarClip = Effect.Parameters["FarClip"];
            Param_Texture = Effect.Parameters["Texture"];
            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_IdColor = Effect.Parameters["IdColor"];

        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
