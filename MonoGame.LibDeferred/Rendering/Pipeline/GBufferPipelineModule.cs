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
        private GBufferEffectSetup _effectSetup = new GBufferEffectSetup();

        private GBufferTarget _gBufferTarget;
        private FullscreenTriangleBuffer _fullscreenTarget;


        public GBufferTarget GBufferTarget { set { _gBufferTarget = value; } }
        public float FarClip
        { set { _effectSetup.Param_FarClip.SetValue(value); } }

        public Vector3 Camera
        { set { _effectSetup.Param_Camera.SetValue(value); } }


        public bool ClearGBuffer { get => RenderingSettings.g_ClearGBuffer; }

        public GBufferPipelineModule(ContentManager content, string shaderPathGbuffer)
            : base(content, shaderPathGbuffer)
        { }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }
        protected override void Load(ContentManager content, string shaderPath)
        {
        }
        public void Draw(MeshMaterialLibrary meshMaterialLibrary, Matrix viewProjection, Matrix view)
        {
            _graphicsDevice.SetRenderTargets(_gBufferTarget.Bindings);

            //Clear the GBuffer
            if (this.ClearGBuffer)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;
                _graphicsDevice.BlendState = BlendState.Opaque;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                _effectSetup.Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!
            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.Opaque, viewProjection: viewProjection, view: view, lightViewPointChanged: true, renderModule: this);

        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Matrix worldView = localWorldMatrix * (Matrix)view;
            _effectSetup.Param_WorldView.SetValue(worldView);
            _effectSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            worldView = Matrix.Invert(Matrix.Transpose(worldView));
            _effectSetup.Param_WorldViewIT.SetValue(worldView);
            _effectSetup.Effect_GBuffer.CurrentTechnique.Passes[0].Apply();
        }

        public void SetMaterialSettings(MaterialEffect material)
        {
            if (RenderingSettings.d_defaultmaterial)
            {
                _effectSetup.Param_Material_DiffuseColor.SetValue(Color.Gray.ToVector3());
                _effectSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_defaultroughness > 0
                                                                                        ? RenderingSettings.m_defaultroughness
                                                                                        : 0.3f);
                _effectSetup.Param_Material_Metallic.SetValue(0.0f);
                _effectSetup.Param_Material_MaterialType.SetValue(0);
                _effectSetup.Effect_GBuffer.CurrentTechnique = _effectSetup.Technique_DrawBasic;
            }
            else
            {
                _effectSetup.Param_Material_Texture.SetValue(material.HasAlbedoMap ? material.AlbedoMap : null);
                _effectSetup.Param_Material_NormalMap.SetValue(material.HasNormalMap ? material.NormalMap : null);
                _effectSetup.Param_Material_RoughnessMap.SetValue(material.HasRoughnessMap ? material.RoughnessMap : null);
                _effectSetup.Param_Material_MetallicMap.SetValue(material.HasMetallicMap ? material.MetallicMap : null);
                _effectSetup.Param_Material_MaskMap.SetValue(material.HasMask ? material.Mask : null);

                _effectSetup.Effect_GBuffer.CurrentTechnique = material.GetGBufferTechnique(_effectSetup);
                
                // -------------------------
                // Set value base material parameters
                if (!material.HasAlbedoMap)
                {
                    if (material.Type == MaterialEffect.MaterialTypes.Emissive && material.EmissiveStrength > 0)
                    {
                        _effectSetup.Param_Material_DiffuseColor.SetValue(material.DiffuseColor);
                        _effectSetup.Param_Material_Metallic.SetValue(material.EmissiveStrength / 8);
                    }
                    else
                    {
                        _effectSetup.Param_Material_DiffuseColor.SetValue(material.DiffuseColor);
                    }
                }

                if (!material.HasRoughnessMap)
                    _effectSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_defaultroughness > 0
                                                                                            ? RenderingSettings.m_defaultroughness
                                                                                            : material.Roughness);
                _effectSetup.Param_Material_Metallic.SetValue(material.Metallic);
                _effectSetup.Param_Material_MaterialType.SetValue(material.MaterialTypeNumber);
            }
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }
    }

    public static class GBufferExtensions
    {
        public static EffectTechnique GetGBufferTechnique(this MaterialEffect material, GBufferEffectSetup effectSetup)
        {
            if (material.HasDisplacementMap)
            {
                return effectSetup.Technique_DrawDisplacement;
            }
            else if (material.HasMask) //Has diffuse for sure then
            {
                if (material.HasNormalMap && material.HasRoughnessMap)
                    return effectSetup.Technique_DrawTextureSpecularNormalMask;
                else if (material.HasNormalMap)
                    return effectSetup.Technique_DrawTextureNormalMask;
                else if (material.HasRoughnessMap)
                    return effectSetup.Technique_DrawTextureSpecularMask;
                else
                    return effectSetup.Technique_DrawTextureSpecularMask;
            }
            else
            {
                if (material.HasNormalMap && material.HasRoughnessMap && material.HasAlbedoMap && material.HasMetallicMap)
                    return effectSetup.Technique_DrawTextureSpecularNormalMetallic;
                else if (material.HasNormalMap && material.HasRoughnessMap && material.HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureSpecularNormal;
                else if (material.HasNormalMap && material.HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureNormal;
                else if (material.HasMetallicMap && material.HasRoughnessMap && material.HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureSpecularMetallic;
                else if (material.HasRoughnessMap && material.HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureSpecular;
                else if (material.HasNormalMap && !material.HasAlbedoMap)
                    return effectSetup.Technique_DrawNormal;
                else if (material.HasAlbedoMap)
                    return effectSetup.Technique_DrawTexture;
            }

            return effectSetup.Technique_DrawBasic;
        }
    }

    public class GBufferEffectSetup : EffectSetupBase
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


        public GBufferEffectSetup(string shaderPathBase = "Shaders/GbufferSetup/", string gBufferEffect = "GBuffer", string gBufferClearEffect = "ClearGBuffer")
              : base(shaderPathBase)
        {
            Effect_GBuffer = ShaderGlobals.content.Load<Effect>(shaderPathBase + gBufferEffect);
            Effect_Clear = ShaderGlobals.content.Load<Effect>(shaderPathBase + gBufferClearEffect);

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
