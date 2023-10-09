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

        private float _farClip;
        public float FarClip
        {
            get { return _farClip; }
            set
            {
                _farClip = value;
                Shaders.GBuffer.Param_FarClip.SetValue(value);
            }
        }

        private Vector3 _camera;
        public Vector3 Camera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                Shaders.GBuffer.Param_Camera.SetValue(value);
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
