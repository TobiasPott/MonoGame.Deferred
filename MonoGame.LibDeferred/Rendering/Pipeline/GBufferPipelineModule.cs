using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class GBufferPipelineModule : PipelineModule, IRenderModule
    {
        private FullscreenTriangleBuffer _fullscreenTarget;

        public float FarClip
        { set { Shaders.GBuffer.Param_FarClip.SetValue(value); } }

        public Vector3 Camera
        { set { Shaders.GBuffer.Param_Camera.SetValue(value); } }


        public GBufferPipelineModule(ContentManager content, string shaderPathGbuffer = "Shaders/GbufferSetup/Gbuffer")
            : base(content, shaderPathGbuffer)
        { }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }
        protected override void Load(ContentManager content, string shaderPath = "Shaders/GbufferSetup/GBuffer") => Load(content, shaderPath, "Shaders/GbufferSetup/ClearGBuffer");
        protected void Load(ContentManager content, string shaderPathGBuffer, string shaderPathGBufferClear)
        {

        }


        public void Draw(MeshMaterialLibrary meshMaterialLibrary, RenderTargetBinding[] _renderTargetBinding, Matrix viewProjection, Matrix view)
        {
            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            //Clear the GBuffer
            if (RenderingSettings.g_ClearGBuffer)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;
                _graphicsDevice.BlendState = BlendState.Opaque;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                Shaders.GBuffer.Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!

            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.Opaque, viewProjection: viewProjection, lightViewPointChanged: true, view: view, renderModule: this);

        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Matrix worldView = localWorldMatrix * (Matrix)view;
            Shaders.GBuffer.Param_WorldView.SetValue(worldView);
            Shaders.GBuffer.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            worldView = Matrix.Invert(Matrix.Transpose(worldView));
            Shaders.GBuffer.Param_WorldViewIT.SetValue(worldView);
            Shaders.GBuffer.Effect_GBuffer.CurrentTechnique.Passes[0].Apply();
        }

        public void SetMaterialSettings(MaterialEffect material)
        {
            if (RenderingSettings.d_defaultmaterial)
            {
                Shaders.GBuffer.Param_Material_DiffuseColor.SetValue(Color.Gray.ToVector3());
                Shaders.GBuffer.Param_Material_Roughness.SetValue(RenderingSettings.m_defaultroughness > 0
                                                                                        ? RenderingSettings.m_defaultroughness
                                                                                        : 0.3f);
                Shaders.GBuffer.Param_Material_Metallic.SetValue(0.0f);
                Shaders.GBuffer.Param_Material_MaterialType.SetValue(0);
                Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawBasic;
            }
            else
            {
                if (material.HasDisplacement)
                {
                    Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                    Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                    Shaders.GBuffer.Param_Material_DisplacementMap.SetValue(material.DisplacementMap);
                    Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawDisplacement;
                }
                else if (material.HasMask) //Has diffuse for sure then
                {
                    if (material.HasNormalMap && material.HasRoughnessMap)
                    {
                        Shaders.GBuffer.Param_Material_MaskMap.SetValue(material.Mask);
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                        Shaders.GBuffer.Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecularNormalMask;
                    }

                    else if (material.HasNormalMap)
                    {
                        Shaders.GBuffer.Param_Material_MaskMap.SetValue(material.Mask);
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureNormalMask;
                    }

                    else if (material.HasRoughnessMap)
                    {
                        Shaders.GBuffer.Param_Material_MaskMap.SetValue(material.Mask);
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecularMask;
                    }
                    else
                    {
                        Shaders.GBuffer.Param_Material_MaskMap.SetValue(material.Mask);
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecularMask;
                    }
                }
                else
                {
                    if (material.HasNormalMap && material.HasRoughnessMap && material.HasDiffuse &&
                        material.HasMetallic)
                    {
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                        Shaders.GBuffer.Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Shaders.GBuffer.Param_Material_MetallicMap.SetValue(material.MetallicMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecularNormalMetallic;
                    }

                    else if (material.HasNormalMap && material.HasRoughnessMap && material.HasDiffuse)
                    {
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                        Shaders.GBuffer.Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecularNormal;
                    }

                    else if (material.HasNormalMap && material.HasDiffuse)
                    {
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureNormal;
                    }

                    else if (material.HasMetallic && material.HasRoughnessMap && material.HasDiffuse)
                    {
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecularMetallic;
                    }

                    else if (material.HasRoughnessMap && material.HasDiffuse)
                    {
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTextureSpecular;
                    }

                    else if (material.HasNormalMap && !material.HasDiffuse)
                    {
                        Shaders.GBuffer.Param_Material_NormalMap.SetValue(material.NormalMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawNormal;
                    }

                    else if (material.HasDiffuse)
                    {
                        Shaders.GBuffer.Param_Material_Texture.SetValue(material.AlbedoMap);
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawTexture;
                    }

                    else
                    {
                        Shaders.GBuffer.Effect_GBuffer.CurrentTechnique = Shaders.GBuffer.Technique_DrawBasic;
                    }
                }


                if (!material.HasDiffuse)
                {
                    if (material.Type == MaterialEffect.MaterialTypes.Emissive && material.EmissiveStrength > 0)
                    {
                        Shaders.GBuffer.Param_Material_DiffuseColor.SetValue(material.DiffuseColor);
                        Shaders.GBuffer.Param_Material_Metallic.SetValue(material.EmissiveStrength / 8);
                    }
                    //* Math.Max(material.EmissiveStrength,1));
                    //}
                    else
                        //{
                        Shaders.GBuffer.Param_Material_DiffuseColor.SetValue(material.DiffuseColor);
                    //}
                }

                if (!material.HasRoughnessMap)
                    Shaders.GBuffer.Param_Material_Roughness.SetValue(RenderingSettings.m_defaultroughness > 0
                                                                                            ? RenderingSettings.m_defaultroughness
                                                                                            : material.Roughness);
                Shaders.GBuffer.Param_Material_Metallic.SetValue(material.Metallic);

                if (material.Type == MaterialEffect.MaterialTypes.SubsurfaceScattering)
                {
                    if (RenderingSettings.sdf_subsurface)
                        Shaders.GBuffer.Param_Material_MaterialType.SetValue(material.MaterialTypeNumber);
                    else
                        Shaders.GBuffer.Param_Material_MaterialType.SetValue(0);
                }
                else
                    Shaders.GBuffer.Param_Material_MaterialType.SetValue(material.MaterialTypeNumber);
            }
        }

        public override void Dispose()
        {
            Shaders.GBuffer.Effect_Clear?.Dispose();
            Shaders.GBuffer.Effect_GBuffer?.Dispose();
        }
    }


}

namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {
        // GBuffer
        public static class GBuffer
        {
            public static Effect Effect_GBuffer = ShaderGlobals.content.Load<Effect>("Shaders/GbufferSetup/GBuffer");
            public static Effect Effect_Clear = ShaderGlobals.content.Load<Effect>("Shaders/GbufferSetup/ClearGBuffer");

            public static EffectPass Pass_ClearGBuffer = Effect_Clear.Techniques["Clear"].Passes[0];

            //Techniques
            public static EffectTechnique Technique_DrawDisplacement = Effect_GBuffer.Techniques["DrawTextureDisplacement"];
            public static EffectTechnique Technique_DrawTextureSpecularNormalMask = Effect_GBuffer.Techniques["DrawTextureSpecularNormalMask"];
            public static EffectTechnique Technique_DrawTextureNormalMask = Effect_GBuffer.Techniques["DrawTextureNormalMask"];
            public static EffectTechnique Technique_DrawTextureSpecularMask = Effect_GBuffer.Techniques["DrawTextureSpecularMask"];
            public static EffectTechnique Technique_DrawTextureMask = Effect_GBuffer.Techniques["DrawTextureMask"];
            public static EffectTechnique Technique_DrawTextureSpecularNormalMetallic = Effect_GBuffer.Techniques["DrawTextureSpecularNormalMetallic"];
            public static EffectTechnique Technique_DrawTextureSpecularNormal = Effect_GBuffer.Techniques["DrawTextureSpecularNormal"];
            public static EffectTechnique Technique_DrawTextureNormal = Effect_GBuffer.Techniques["DrawTextureNormal"];
            public static EffectTechnique Technique_DrawTextureSpecular = Effect_GBuffer.Techniques["DrawTextureSpecular"];
            public static EffectTechnique Technique_DrawTextureSpecularMetallic = Effect_GBuffer.Techniques["DrawTextureSpecularMetallic"];
            public static EffectTechnique Technique_DrawTexture = Effect_GBuffer.Techniques["DrawTexture"];
            public static EffectTechnique Technique_DrawNormal = Effect_GBuffer.Techniques["DrawNormal"];
            public static EffectTechnique Technique_DrawBasic = Effect_GBuffer.Techniques["DrawBasic"];

            // Parameters
            public static EffectParameter Param_WorldView = Effect_GBuffer.Parameters["WorldView"];
            public static EffectParameter Param_WorldViewProj = Effect_GBuffer.Parameters["WorldViewProj"];
            public static EffectParameter Param_WorldViewIT = Effect_GBuffer.Parameters["WorldViewIT"];
            public static EffectParameter Param_Camera = Effect_GBuffer.Parameters["Camera"];
            public static EffectParameter Param_FarClip = Effect_GBuffer.Parameters["FarClip"];

            public static EffectParameter Param_Material_Metallic = Effect_GBuffer.Parameters["Metallic"];
            public static EffectParameter Param_Material_MetallicMap = Effect_GBuffer.Parameters["MetallicMap"];
            public static EffectParameter Param_Material_DiffuseColor = Effect_GBuffer.Parameters["DiffuseColor"];
            public static EffectParameter Param_Material_Roughness = Effect_GBuffer.Parameters["Roughness"];

            public static EffectParameter Param_Material_MaskMap = Effect_GBuffer.Parameters["Mask"];
            public static EffectParameter Param_Material_Texture = Effect_GBuffer.Parameters["Texture"];
            public static EffectParameter Param_Material_NormalMap = Effect_GBuffer.Parameters["NormalMap"];
            public static EffectParameter Param_Material_RoughnessMap = Effect_GBuffer.Parameters["RoughnessMap"];
            public static EffectParameter Param_Material_DisplacementMap = Effect_GBuffer.Parameters["DisplacementMap"];

            public static EffectParameter Param_Material_MaterialType = Effect_GBuffer.Parameters["MaterialType"];
        }

    }

}