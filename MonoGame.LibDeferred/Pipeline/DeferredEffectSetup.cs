using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{
    public class DeferredEffectSetup : BaseFxSetup
    {
        private static DeferredEffectSetup _instance;
        public static DeferredEffectSetup Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DeferredEffectSetup();
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
        //public EffectParameter Param_HologramMap { get; protected set; } // Unused
        //public EffectParameter Param_LinearMap { get; protected set; } // Unused
        //public EffectParameter Param_SSRMap { get; protected set; } // Unused


        public DeferredEffectSetup(string shaderPath = "Shaders/Deferred/DeferredCompose", string shaderPathClear = "Shaders/Deferred/DeferredClear") : base(shaderPath)
        {
            Effect_Compose = Globals.content.Load<Effect>(shaderPath);
            Effect_Clear = Globals.content.Load<Effect>(shaderPathClear);

            Technique_NonLinear = Effect_Compose.Techniques["TechniqueNonLinear"];
            Technique_Linear = Effect_Compose.Techniques["TechniqueLinear"];

            Pass_Clear = Effect_Clear.CurrentTechnique.Passes[0];

            Param_ColorMap = Effect_Compose.Parameters["colorMap"];
            Param_NormalMap = Effect_Compose.Parameters["normalMap"];
            Param_DiffuseLightMap = Effect_Compose.Parameters["diffuseLightMap"];
            Param_SpecularLightMap = Effect_Compose.Parameters["specularLightMap"];
            Param_VolumeLightMap = Effect_Compose.Parameters["volumeLightMap"];
            Param_SSAOMap = Effect_Compose.Parameters["SSAOMap"];
            Param_UseSSAO = Effect_Compose.Parameters["useSSAO"];
            //Param_HologramMap = Effect_Compose.Parameters["HologramMap"];
            //Param_LinearMap = Effect_Compose.Parameters["LinearMap"];
            //Param_SSRMap = Effect_Compose.Parameters["SSRMap"];

        }

        public override void Dispose()
        {
            Effect_Compose?.Dispose();
            Effect_Clear?.Dispose();
        }
    }

}
