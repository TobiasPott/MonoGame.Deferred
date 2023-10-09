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
        private static Effect Effect_GBuffer = ShaderGlobals.content.Load<Effect>("Shaders/GbufferSetup/GBuffer");
        private static Effect Effect_Clear = ShaderGlobals.content.Load<Effect>("Shaders/GbufferSetup/ClearGBuffer");

        private static EffectPass Pass_ClearGBuffer = Effect_Clear.Techniques["Clear"].Passes[0];

        //Techniques
        private static EffectTechnique Technique_DrawDisplacement = Effect_GBuffer.Techniques["DrawTextureDisplacement"];
        private static EffectTechnique Technique_DrawTextureSpecularNormalMask = Effect_GBuffer.Techniques["DrawTextureSpecularNormalMask"];
        private static EffectTechnique Technique_DrawTextureNormalMask = Effect_GBuffer.Techniques["DrawTextureNormalMask"];
        private static EffectTechnique Technique_DrawTextureSpecularMask = Effect_GBuffer.Techniques["DrawTextureSpecularMask"];
        private static EffectTechnique Technique_DrawTextureMask = Effect_GBuffer.Techniques["DrawTextureMask"];
        private static EffectTechnique Technique_DrawTextureSpecularNormalMetallic = Effect_GBuffer.Techniques["DrawTextureSpecularNormalMetallic"];
        private static EffectTechnique Technique_DrawTextureSpecularNormal = Effect_GBuffer.Techniques["DrawTextureSpecularNormal"];
        private static EffectTechnique Technique_DrawTextureNormal = Effect_GBuffer.Techniques["DrawTextureNormal"];
        private static EffectTechnique Technique_DrawTextureSpecular = Effect_GBuffer.Techniques["DrawTextureSpecular"];
        private static EffectTechnique Technique_DrawTextureSpecularMetallic = Effect_GBuffer.Techniques["DrawTextureSpecularMetallic"];
        private static EffectTechnique Technique_DrawTexture = Effect_GBuffer.Techniques["DrawTexture"];
        private static EffectTechnique Technique_DrawNormal = Effect_GBuffer.Techniques["DrawNormal"];
        private static EffectTechnique Technique_DrawBasic = Effect_GBuffer.Techniques["DrawBasic"];

        // Parameters
        private static EffectParameter Param_WorldView = Effect_GBuffer.Parameters["WorldView"];
        private static EffectParameter Param_WorldViewProj = Effect_GBuffer.Parameters["WorldViewProj"];
        private static EffectParameter Param_WorldViewIT = Effect_GBuffer.Parameters["WorldViewIT"];
        private static EffectParameter Param_Camera = Effect_GBuffer.Parameters["Camera"];
        private static EffectParameter Param_FarClip = Effect_GBuffer.Parameters["FarClip"];

        private static EffectParameter Param_Material_Metallic = Effect_GBuffer.Parameters["Metallic"];
        private static EffectParameter Param_Material_MetallicMap = Effect_GBuffer.Parameters["MetallicMap"];
        private static EffectParameter Param_Material_DiffuseColor = Effect_GBuffer.Parameters["DiffuseColor"];
        private static EffectParameter Param_Material_Roughness = Effect_GBuffer.Parameters["Roughness"];

        private static EffectParameter Param_Material_MaskMap = Effect_GBuffer.Parameters["Mask"];
        private static EffectParameter Param_Material_Texture = Effect_GBuffer.Parameters["Texture"];
        private static EffectParameter Param_Material_NormalMap = Effect_GBuffer.Parameters["NormalMap"];
        private static EffectParameter Param_Material_RoughnessMap = Effect_GBuffer.Parameters["RoughnessMap"];
        private static EffectParameter Param_Material_DisplacementMap = Effect_GBuffer.Parameters["DisplacementMap"];

        private static EffectParameter Param_Material_MaterialType = Effect_GBuffer.Parameters["MaterialType"];



        private FullscreenTriangleBuffer _fullscreenTarget;

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

                Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!

            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.Opaque, viewProjection: viewProjection, lightViewPointChanged: true, view: view, renderModule: this);

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
                        Technique_DrawDisplacement;
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
