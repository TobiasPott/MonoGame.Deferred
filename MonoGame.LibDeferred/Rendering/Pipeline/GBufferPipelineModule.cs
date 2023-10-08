using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class GBufferPipelineModule : RenderingPipelineModule, IRenderModule
    {
        private Effect Effect_Clear;
        private Effect Effect_GBuffer;
        private EffectPass Pass_ClearGBuffer;

        private EffectParameter Param_WorldView;
        private EffectParameter Param_WorldViewProj;
        private EffectParameter Param_WorldViewIT;
        private EffectParameter Param_Camera;
        private EffectParameter Param_FarClip;

        private EffectParameter Param_Material_Metallic;
        private EffectParameter Param_Material_MetallicMap;
        private EffectParameter Param_Material_DiffuseColor;
        private EffectParameter Param_Material_Roughness;
        private EffectParameter Param_Material_MaskMap;
        private EffectParameter Param_Material_Texture;
        private EffectParameter Param_Material_NormalMap;
        private EffectParameter Param_Material_DisplacementMap;
        private EffectParameter Param_Material_RoughnessMap;
        private EffectParameter Param_Material_MaterialType;

        private EffectTechnique Technique_DrawTextureDisplacement;
        private EffectTechnique Technique_DrawTextureSpecularNormalMask;
        private EffectTechnique Technique_DrawTextureNormalMask;
        private EffectTechnique Technique_DrawTextureSpecularMask;
        private EffectTechnique Technique_DrawTextureMask;
        private EffectTechnique Technique_DrawTextureSpecularNormalMetallic;
        private EffectTechnique Technique_DrawTextureSpecularNormal;
        private EffectTechnique Technique_DrawTextureNormal;
        private EffectTechnique Technique_DrawTextureSpecular;
        private EffectTechnique Technique_DrawTextureSpecularMetallic;

        private EffectTechnique Technique_DrawTexture;
        private EffectTechnique Technique_DrawNormal;
        private EffectTechnique Technique_DrawBasic;

        private FullscreenTriangleBuffer _fullscreenTarget;

        public GBufferPipelineModule(ContentManager content, string shaderPathGbuffer = "Shaders/GbufferSetup/Gbuffer")
            : base(content, shaderPathGbuffer)
        { }

        private float _farClip;
        public float FarClip
        {
            get { return _farClip; }
            set
            {
                _farClip = value;
                Param_FarClip.SetValue(value);
            }
        }

        private Vector3 _camera;
        public Vector3 Camera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                Param_Camera.SetValue(value);
            }
        }

        protected override void Load(ContentManager content, string shaderPath = "Shaders/GbufferSetup/GBuffer") => Load(content, shaderPath, "Shaders/GbufferSetup/ClearGBuffer");
        public void Load(ContentManager content, string shaderPathGBuffer, string shaderPathGBufferClear)
        {
            Effect_GBuffer = content.Load<Effect>(shaderPathGBuffer);
            Effect_Clear = content.Load<Effect>(shaderPathGBufferClear);
        }
        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

            Pass_ClearGBuffer = Effect_Clear.Techniques["Clear"].Passes[0];


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

            //Techniques

            Technique_DrawTextureDisplacement = Effect_GBuffer.Techniques["DrawTextureDisplacement"];
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
        }


        public void Draw(RenderTargetBinding[] _renderTargetBinding, MeshMaterialLibrary meshMaterialLibrary, Matrix _viewProjection, Matrix _view)
        {
            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            //Clear the GBuffer
            if (RenderingSettings.g_ClearGBuffer)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;
                _graphicsDevice.BlendState = BlendState.Opaque;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;

                Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!

            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.Opaque, viewProjection: _viewProjection, lightViewPointChanged: true, view: _view, renderModule: this);

        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Matrix worldView = localWorldMatrix * (Matrix)view;
            Param_WorldView.SetValue(worldView);
            Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            worldView = Matrix.Invert(Matrix.Transpose(worldView));
            Param_WorldViewIT.SetValue(worldView);

            Effect_GBuffer.CurrentTechnique.Passes[0].Apply();
        }

        public void SetMaterialSettings(MaterialEffect material)
        {
            if (RenderingSettings.d_defaultmaterial)
            {
                Param_Material_DiffuseColor.SetValue(Color.Gray.ToVector3());
                Param_Material_Roughness.SetValue(RenderingSettings.m_defaultroughness > 0
                        ? RenderingSettings.m_defaultroughness
                        : 0.3f);
                Param_Material_Metallic.SetValue(0.0f);
                Param_Material_MaterialType.SetValue(0);
                Effect_GBuffer.CurrentTechnique = Technique_DrawBasic;
            }
            else
            {
                if (material.HasDisplacement)
                {
                    Param_Material_Texture.SetValue(material.AlbedoMap);
                    Param_Material_NormalMap.SetValue(material.NormalMap);
                    Param_Material_DisplacementMap.SetValue(material.DisplacementMap);
                    Effect_GBuffer.CurrentTechnique =
                        Technique_DrawTextureDisplacement;
                }
                else if (material.HasMask) //Has diffuse for sure then
                {
                    if (material.HasNormalMap && material.HasRoughnessMap)
                    {
                        Param_Material_MaskMap.SetValue(material.Mask);
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_NormalMap.SetValue(material.NormalMap);
                        Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecularNormalMask;
                    }

                    else if (material.HasNormalMap)
                    {
                        Param_Material_MaskMap.SetValue(material.Mask);
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_NormalMap.SetValue(material.NormalMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureNormalMask;
                    }

                    else if (material.HasRoughnessMap)
                    {
                        Param_Material_MaskMap.SetValue(material.Mask);
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecularMask;
                    }
                    else
                    {
                        Param_Material_MaskMap.SetValue(material.Mask);
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecularMask;
                    }
                }
                else
                {
                    if (material.HasNormalMap && material.HasRoughnessMap && material.HasDiffuse &&
                        material.HasMetallic)
                    {
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_NormalMap.SetValue(material.NormalMap);
                        Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Param_Material_MetallicMap.SetValue(material.MetallicMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecularNormalMetallic;
                    }

                    else if (material.HasNormalMap && material.HasRoughnessMap && material.HasDiffuse)
                    {
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_NormalMap.SetValue(material.NormalMap);
                        Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecularNormal;
                    }

                    else if (material.HasNormalMap && material.HasDiffuse)
                    {
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_NormalMap.SetValue(material.NormalMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureNormal;
                    }

                    else if (material.HasMetallic && material.HasRoughnessMap && material.HasDiffuse)
                    {
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecularMetallic;
                    }

                    else if (material.HasRoughnessMap && material.HasDiffuse)
                    {
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Param_Material_RoughnessMap.SetValue(material.RoughnessMap);
                        Effect_GBuffer.CurrentTechnique =
                            Technique_DrawTextureSpecular;
                    }

                    else if (material.HasNormalMap && !material.HasDiffuse)
                    {
                        Param_Material_NormalMap.SetValue(material.NormalMap);
                        Effect_GBuffer.CurrentTechnique = Technique_DrawNormal;
                    }

                    else if (material.HasDiffuse)
                    {
                        Param_Material_Texture.SetValue(material.AlbedoMap);
                        Effect_GBuffer.CurrentTechnique = Technique_DrawTexture;
                    }

                    else
                    {
                        Effect_GBuffer.CurrentTechnique = Technique_DrawBasic;
                    }
                }


                if (!material.HasDiffuse)
                {
                    if (material.Type == MaterialEffect.MaterialTypes.Emissive && material.EmissiveStrength > 0)
                    {
                        Param_Material_DiffuseColor.SetValue(material.DiffuseColor);
                        Param_Material_Metallic.SetValue(material.EmissiveStrength / 8);
                    }
                    //* Math.Max(material.EmissiveStrength,1));
                    //}
                    else
                        //{
                        Param_Material_DiffuseColor.SetValue(material.DiffuseColor);
                    //}
                }

                if (!material.HasRoughnessMap)
                    Param_Material_Roughness.SetValue(RenderingSettings.m_defaultroughness >
                                                                               0
                        ? RenderingSettings.m_defaultroughness
                        : material.Roughness);
                Param_Material_Metallic.SetValue(material.Metallic);

                if (material.Type == MaterialEffect.MaterialTypes.SubsurfaceScattering)
                {
                    if (RenderingSettings.sdf_subsurface)
                        Param_Material_MaterialType.SetValue(material.MaterialTypeNumber);
                    else
                        Param_Material_MaterialType.SetValue(0);
                }
                else
                    Param_Material_MaterialType.SetValue(material.MaterialTypeNumber);
            }
        }

        public override void Dispose()
        {
            Effect_Clear?.Dispose();
            Effect_GBuffer?.Dispose();
        }
    }
}
