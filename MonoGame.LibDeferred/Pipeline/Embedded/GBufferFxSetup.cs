using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{
    public class GBufferFxSetup : BaseFxSetup
    {
        public Effect Effect_GBuffer { get; protected set; }
        public Effect Effect_Clear { get; protected set; }

        public EffectPass Pass_ClearGBuffer { get; protected set; }

        //Techniques
        public EffectTechnique Technique_DrawDisplacement { get; protected set; }
        public EffectTechnique Technique_DrawTextureSpecularNormalMask { get; protected set; }
        public EffectTechnique Technique_DrawTextureNormalMask { get; protected set; } // = Effect
        public EffectTechnique Technique_DrawTextureSpecularMask { get; protected set; } // = Effe
        public EffectTechnique Technique_DrawTextureMask { get; protected set; } // = Effect_GBuff
        public EffectTechnique Technique_DrawTextureSpecularNormalMetallic { get; protected set; }
        public EffectTechnique Technique_DrawTextureSpecularNormal { get; protected set; } // = Ef
        public EffectTechnique Technique_DrawTextureNormal { get; protected set; } // = Effect_GBu
        public EffectTechnique Technique_DrawTextureSpecular { get; protected set; } // = Effect_G
        public EffectTechnique Technique_DrawTextureSpecularMetallic { get; protected set; }
        public EffectTechnique Technique_DrawTexture { get; protected set; }
        public EffectTechnique Technique_DrawNormal { get; protected set; }
        public EffectTechnique Technique_DrawBasic { get; protected set; }

        // Parameters
        public EffectParameter Param_WorldView { get; protected set; } // = Effec
        public EffectParameter Param_WorldViewProj { get; protected set; } // = E
        public EffectParameter Param_WorldViewIT { get; protected set; } //  = Ef
        public EffectParameter Param_Camera { get; protected set; } // = Effect_G
        public EffectParameter Param_FarClip { get; protected set; } // = Effect_

        public EffectParameter Param_Material_Metallic { get; protected set; } //
        public EffectParameter Param_Material_MetallicMap { get; protected set; }
        public EffectParameter Param_Material_DiffuseColor { get; protected set; }
        public EffectParameter Param_Material_Roughness { get; protected set; }

        public EffectParameter Param_Material_MaskMap { get; protected set; }
        public EffectParameter Param_Material_Texture { get; protected set; }
        public EffectParameter Param_Material_NormalMap { get; protected set; }
        public EffectParameter Param_Material_RoughnessMap { get; protected set; }
        public EffectParameter Param_Material_DisplacementMap { get; protected set; }

        public EffectParameter Param_Material_MaterialType { get; protected set; }


        public GBufferFxSetup(string shaderPathBase = "Shaders/GbufferSetup/", string gBufferEffect = "GBuffer", string gBufferClearEffect = "ClearGBuffer")
              : base(shaderPathBase)
        {
            Effect_GBuffer = Globals.content.Load<Effect>(shaderPathBase + gBufferEffect);
            Effect_Clear = Globals.content.Load<Effect>(shaderPathBase + gBufferClearEffect);

            Pass_ClearGBuffer = Effect_Clear.Techniques["Clear"].Passes[0];

            //Techniques
            Technique_DrawDisplacement = Effect_GBuffer.Techniques["DrawTextureDisplacement"];
            Technique_DrawTextureSpecularNormalMask = Effect_GBuffer.Techniques["DrawTextureSpecularNormalMask"];
            Technique_DrawTextureNormalMask = Effect_GBuffer.Techniques["DrawTextureNormalMask"];
            Technique_DrawTextureSpecularMask = Effect_GBuffer.Techniques["DrawTextureSpecularMask"];
            Technique_DrawTextureMask = Effect_GBuffer.Techniques["DrawTextureMask"];
            Technique_DrawTextureSpecularNormalMetallic = Effect_GBuffer.Techniques["DrawTextureSpecularNormalMetallic"];
            Technique_DrawTextureSpecularNormal = Effect_GBuffer.Techniques["DrawTextureSpecularNormal"];
            Technique_DrawTextureNormal = Effect_GBuffer.Techniques["DrawTextureNormal"];
            Technique_DrawTextureSpecular = Effect_GBuffer.Techniques["DrawTextureSpecular"];
            Technique_DrawTextureSpecularMetallic = Effect_GBuffer.Techniques["DrawTextureSpecularMetallic"];
            Technique_DrawTexture = Effect_GBuffer.Techniques["DrawTexture"];
            Technique_DrawNormal = Effect_GBuffer.Techniques["DrawNormal"];
            Technique_DrawBasic = Effect_GBuffer.Techniques["DrawBasic"];

            // Parameters
            Param_WorldView = Effect_GBuffer.Parameters["WorldView"];
            Param_WorldViewProj = Effect_GBuffer.Parameters["WorldViewProj"];
            Param_WorldViewIT = Effect_GBuffer.Parameters["WorldViewIT"];
            Param_Camera = Effect_GBuffer.Parameters["Camera"];
            Param_FarClip = Effect_GBuffer.Parameters["FarClip"];

            Param_Material_Metallic = Effect_GBuffer.Parameters["Metallic"];
            Param_Material_MetallicMap = Effect_GBuffer.Parameters["MetallicMap"];
            Param_Material_DiffuseColor = Effect_GBuffer.Parameters["DiffuseColor"];
            Param_Material_Roughness = Effect_GBuffer.Parameters["Roughness"];

            Param_Material_MaskMap = Effect_GBuffer.Parameters["Mask"];
            Param_Material_Texture = Effect_GBuffer.Parameters["Texture"];
            Param_Material_NormalMap = Effect_GBuffer.Parameters["NormalMap"];
            Param_Material_RoughnessMap = Effect_GBuffer.Parameters["RoughnessMap"];
            Param_Material_DisplacementMap = Effect_GBuffer.Parameters["DisplacementMap"];

            Param_Material_MaterialType = Effect_GBuffer.Parameters["MaterialType"];
        }

        public override void Dispose()
        {
            Effect_GBuffer?.Dispose();
            Effect_Clear?.Dispose();
        }
    }

}
