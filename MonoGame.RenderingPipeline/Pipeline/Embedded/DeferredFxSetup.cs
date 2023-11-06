using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{
    public class DeferredFxSetup : BaseFxSetup
    {
        private static DeferredFxSetup _instance;
        public static DeferredFxSetup Instance
        {
            get
            {
                _instance ??= new DeferredFxSetup();
                return _instance;
            }
        }


        public Effect Effect_Clear { get; protected set; }
        public Effect Effect_Compose { get; protected set; }

        public EffectTechnique Technique_NonLinear { get; protected set; }
        public EffectTechnique Technique_Linear { get; protected set; }
        public EffectPass Pass_Clear { get; protected set; }

        public EffectParameter Param_ColorMap { get; protected set; }
        public EffectParameter Param_NormalMap { get; protected set; }
        public EffectParameter Param_DiffuseLightMap { get; protected set; }
        public EffectParameter Param_SpecularLightMap { get; protected set; }
        public EffectParameter Param_VolumeLightMap { get; protected set; }
        public EffectParameter Param_SSAOMap { get; protected set; }
        public EffectParameter Param_UseSSAO { get; protected set; }


        public DeferredFxSetup(string shaderPath = "Shaders/Deferred/DeferredCompose", string shaderPathClear = "Shaders/Deferred/DeferredClear") : base()
        {
            Effect_Compose = Globals.content.Load<Effect>(shaderPath);
            Effect_Clear = Globals.content.Load<Effect>(shaderPathClear);

            Technique_NonLinear = Effect_Compose.Techniques["TechniqueNonLinear"];
            Technique_Linear = Effect_Compose.Techniques["TechniqueLinear"];

            Pass_Clear = Effect_Clear.CurrentTechnique.Passes[0];

            Param_ColorMap = Effect_Compose.Parameters["ColorMap"];
            Param_NormalMap = Effect_Compose.Parameters["NormalMap"];
            Param_DiffuseLightMap = Effect_Compose.Parameters["DiffuseLightMap"];
            Param_SpecularLightMap = Effect_Compose.Parameters["SpecularLightMap"];
            Param_VolumeLightMap = Effect_Compose.Parameters["VolumeLightMap"];
            Param_SSAOMap = Effect_Compose.Parameters["SSAOMap"];
            Param_UseSSAO = Effect_Compose.Parameters["useSSAO"];
        }

        public override void Dispose()
        {
            Effect_Compose?.Dispose();
            Effect_Clear?.Dispose();
        }
    }

}
