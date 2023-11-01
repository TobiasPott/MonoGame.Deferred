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
        private readonly GBufferFxSetup _fxSetup = new GBufferFxSetup();
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
                _fxSetup.Pass_ClearGBuffer.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Draw the Gbuffer!
            if (meshBatcher.CheckRequiresRedraw(RenderType.Opaque, true, false))
                meshBatcher.Draw(renderType: RenderType.Opaque, this.Matrices, RenderContext.Default, this);

            // sample profiler if set
            this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_GBuffer);
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Matrix worldView = localWorldMatrix * (Matrix)view;
            _fxSetup.Param_WorldView.SetValue(worldView);
            _fxSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            worldView = Matrix.Invert(Matrix.Transpose(worldView));
            _fxSetup.Param_WorldViewIT.SetValue(worldView);
            _fxSetup.Effect_GBuffer.CurrentTechnique.Passes[0].Apply();

            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
        }

        public void SetMaterialSettings(MaterialBase material)
        {
            if (RenderingSettings.d_DefaultMaterial)
            {
                _fxSetup.Param_Material_DiffuseColor.SetValue(Color.Gray.ToVector3());
                _fxSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_DefaultRoughness > 0
                                                                                        ? RenderingSettings.m_DefaultRoughness
                                                                                        : 0.3f);
                _fxSetup.Param_Material_Metallic.SetValue(0.0f);
                _fxSetup.Param_Material_MaterialType.SetValue(0);
                _fxSetup.Effect_GBuffer.CurrentTechnique = _fxSetup.Technique_DrawBasic;
            }
            else
            {
                _fxSetup.Param_Material_AlbedoMap.SetValue(material.HasAlbedoMap ? material.AlbedoMap : null);
                _fxSetup.Param_Material_NormalMap.SetValue(material.HasNormalMap ? material.NormalMap : null);
                _fxSetup.Param_Material_RoughnessMap.SetValue(material.HasRoughnessMap ? material.RoughnessMap : null);
                _fxSetup.Param_Material_MetallicMap.SetValue(material.HasMetallicMap ? material.MetallicMap : null);
                _fxSetup.Param_Material_MaskMap.SetValue(material.HasMask ? material.Mask : null);

                _fxSetup.Effect_GBuffer.CurrentTechnique = material.GetGBufferTechnique(_fxSetup);

                // -------------------------
                // Set value base material parameters
                if (!material.HasAlbedoMap)
                {
                    if (material.Type == MaterialBase.MaterialTypes.Emissive && material.EmissiveStrength > 0)
                    {
                        _fxSetup.Param_Material_DiffuseColor.SetValue(material.BaseColor);
                        _fxSetup.Param_Material_Metallic.SetValue(material.EmissiveStrength / 8);
                    }
                    else
                    {
                        _fxSetup.Param_Material_DiffuseColor.SetValue(material.BaseColor);
                    }
                }

                if (!material.HasRoughnessMap)
                    _fxSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_DefaultRoughness > 0
                                                                                            ? RenderingSettings.m_DefaultRoughness
                                                                                            : material.Roughness);
                _fxSetup.Param_Material_Metallic.SetValue(material.Metallic);
                _fxSetup.Param_Material_MaterialType.SetValue(material.MaterialTypeNumber);
            }
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }
    }

    public static class GBufferExtensions
    {
        public static EffectTechnique GetGBufferTechnique(this MaterialBase material, GBufferFxSetup effectSetup)
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
