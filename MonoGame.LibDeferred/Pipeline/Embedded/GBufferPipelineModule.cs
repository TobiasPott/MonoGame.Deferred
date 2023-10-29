using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline
{
    //Just a template
    public class GBufferPipelineModule : PipelineModule, IRenderModule
    {
        private GBufferFxSetup _effectSetup = new GBufferFxSetup();
        private GBufferTarget _gBufferTarget;
        private FullscreenTriangleBuffer _fullscreenTarget;


        public GBufferTarget GBufferTarget { set { _gBufferTarget = value; } }

        public bool ClearGBuffer { get; set; } = true;


        public GBufferPipelineModule()
            : base()
        { }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }
        public void Draw(DynamicMeshBatcher meshBatcher)
        {
            _graphicsDevice.SetRenderTargets(_gBufferTarget.Bindings);

            //Clear the GBuffer
            if (this.ClearGBuffer)
            {
                _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullNone, BlendStateOption.Opaque);
                _effectSetup.Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!
            if (meshBatcher.CheckRequiresRedraw(RenderType.Opaque, true, false))
                meshBatcher.Draw(renderType: RenderType.Opaque, this.Matrices, RenderContext.Default, this);

        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Matrix worldView = localWorldMatrix * (Matrix)view;
            _effectSetup.Param_WorldView.SetValue(worldView);
            _effectSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            worldView = Matrix.Invert(Matrix.Transpose(worldView));
            _effectSetup.Param_WorldViewIT.SetValue(worldView);
            _effectSetup.Effect_GBuffer.CurrentTechnique.Passes[0].Apply();

            _effectSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
        }

        public void SetMaterialSettings(MaterialEffect material)
        {
            if (RenderingSettings.d_DefaultMaterial)
            {
                _effectSetup.Param_Material_DiffuseColor.SetValue(Color.Gray.ToVector3());
                _effectSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_DefaultRoughness > 0
                                                                                        ? RenderingSettings.m_DefaultRoughness
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
                    _effectSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_DefaultRoughness > 0
                                                                                            ? RenderingSettings.m_DefaultRoughness
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
        public static EffectTechnique GetGBufferTechnique(this MaterialEffect material, GBufferFxSetup effectSetup)
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

}
