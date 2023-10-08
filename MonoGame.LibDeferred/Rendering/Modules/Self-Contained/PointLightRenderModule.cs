using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.DeferredLighting
{
    public class PointLightRenderModule : IDisposable
    {

        public Effect deferredPointLight_Effect;

        public EffectTechnique deferredPointLight_Unshadowed;
        public EffectTechnique deferredPointLight_UnshadowedVolumetric;
        public EffectTechnique deferredPointLight_ShadowedSDF;
        public EffectTechnique deferredPointLight_Shadowed;
        public EffectTechnique deferredPointLight_ShadowedVolumetric;
        public EffectTechnique deferredPointLight_WriteStencil;

        public EffectParameter deferredPointLightParameter_ShadowMap;

        public EffectParameter deferredPointLightParameter_Resolution;
        public EffectParameter deferredPointLightParameter_WorldView;
        public EffectParameter deferredPointLightParameter_WorldViewProjection;
        public EffectParameter deferredPointLightParameter_InverseView;

        public EffectParameter deferredPointLightParameter_LightPosition;
        public EffectParameter deferredPointLightParameter_LightColor;
        public EffectParameter deferredPointLightParameter_LightRadius;
        public EffectParameter deferredPointLightParameter_LightIntensity;
        public EffectParameter deferredPointLightParameter_ShadowMapSize;
        public EffectParameter deferredPointLightParameter_ShadowMapRadius;
        public EffectParameter deferredPointLightParameter_Inside;
        public EffectParameter deferredPointLightParameter_Time;
        public EffectParameter deferredPointLightParameter_FarClip;
        public EffectParameter deferredPointLightParameter_LightVolumeDensity;

        public EffectParameter deferredPointLightParameter_VolumeTexParam;
        public EffectParameter deferredPointLightParameter_VolumeTexSizeParam;
        public EffectParameter deferredPointLightParameter_VolumeTexResolution;
        public EffectParameter deferredPointLightParameter_InstanceInverseMatrix;
        public EffectParameter deferredPointLightParameter_InstanceScale;
        public EffectParameter deferredPointLightParameter_InstanceSDFIndex;
        public EffectParameter deferredPointLightParameter_InstancesCount;

        public EffectParameter deferredPointLightParameter_NoiseMap;
        public EffectParameter deferredPointLightParameter_AlbedoMap;
        public EffectParameter deferredPointLightParameter_NormalMap;
        public EffectParameter deferredPointLightParameter_DepthMap;

        private int _shaderIndex;

        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;

        public PointLightRenderModule(ShaderManager shaderManager, string shaderPath = "Shaders/Deferred/DeferredPointLight")
        {
            Load(shaderManager, shaderPath);

            InitializeShader();

            _stencilCullPass1 = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.LessEqual,
                StencilFunction = CompareFunction.Always,
                StencilDepthBufferFail = StencilOperation.IncrementSaturation,
                StencilPass = StencilOperation.Keep,
                StencilFail = StencilOperation.Keep,
                CounterClockwiseStencilFunction = CompareFunction.Always,
                CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep,
                CounterClockwiseStencilPass = StencilOperation.Keep,
                CounterClockwiseStencilFail = StencilOperation.Keep,
                StencilMask = 0,
                ReferenceStencil = 0,
                StencilEnable = true,
            };

            _stencilCullPass2 = new DepthStencilState()
            {
                DepthBufferEnable = false,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.GreaterEqual,
                CounterClockwiseStencilFunction = CompareFunction.Equal,
                StencilFunction = CompareFunction.Equal,
                StencilFail = StencilOperation.Zero,
                StencilPass = StencilOperation.Zero,
                CounterClockwiseStencilFail = StencilOperation.Zero,
                CounterClockwiseStencilPass = StencilOperation.Zero,
                ReferenceStencil = 0,
                StencilEnable = true,
                StencilMask = 0,

            };

        }

        private void Load(ShaderManager shaderManager, string shaderPath)
        {
            _shaderIndex = shaderManager.AddShader(shaderPath);

            deferredPointLight_Effect = shaderManager.GetShader(_shaderIndex);
        }


        private void InitializeShader()
        {
            deferredPointLight_Unshadowed = deferredPointLight_Effect.Techniques["Unshadowed"];
            deferredPointLight_UnshadowedVolumetric = deferredPointLight_Effect.Techniques["UnshadowedVolume"];
            deferredPointLight_Shadowed = deferredPointLight_Effect.Techniques["Shadowed"];
            deferredPointLight_ShadowedSDF = deferredPointLight_Effect.Techniques["ShadowedSDF"];
            deferredPointLight_ShadowedVolumetric = deferredPointLight_Effect.Techniques["ShadowedVolume"];
            deferredPointLight_WriteStencil = deferredPointLight_Effect.Techniques["WriteStencilMask"];

            deferredPointLightParameter_ShadowMap = deferredPointLight_Effect.Parameters["ShadowMap"];

            deferredPointLightParameter_Resolution = deferredPointLight_Effect.Parameters["Resolution"];
            deferredPointLightParameter_WorldView = deferredPointLight_Effect.Parameters["WorldView"];
            deferredPointLightParameter_WorldViewProjection = deferredPointLight_Effect.Parameters["WorldViewProj"];
            deferredPointLightParameter_InverseView = deferredPointLight_Effect.Parameters["InverseView"];

            deferredPointLightParameter_LightPosition = deferredPointLight_Effect.Parameters["lightPosition"];
            deferredPointLightParameter_LightColor = deferredPointLight_Effect.Parameters["lightColor"];
            deferredPointLightParameter_LightRadius = deferredPointLight_Effect.Parameters["lightRadius"];
            deferredPointLightParameter_LightIntensity = deferredPointLight_Effect.Parameters["lightIntensity"];
            deferredPointLightParameter_ShadowMapSize = deferredPointLight_Effect.Parameters["ShadowMapSize"];
            deferredPointLightParameter_ShadowMapRadius = deferredPointLight_Effect.Parameters["ShadowMapRadius"];
            deferredPointLightParameter_Inside = deferredPointLight_Effect.Parameters["inside"];
            deferredPointLightParameter_Time = deferredPointLight_Effect.Parameters["Time"];
            deferredPointLightParameter_FarClip = deferredPointLight_Effect.Parameters["FarClip"];
            deferredPointLightParameter_LightVolumeDensity = deferredPointLight_Effect.Parameters["lightVolumeDensity"];

            deferredPointLightParameter_VolumeTexParam = deferredPointLight_Effect.Parameters["VolumeTex"];
            deferredPointLightParameter_VolumeTexSizeParam = deferredPointLight_Effect.Parameters["VolumeTexSize"];
            deferredPointLightParameter_VolumeTexResolution = deferredPointLight_Effect.Parameters["VolumeTexResolution"];
            deferredPointLightParameter_InstanceInverseMatrix = deferredPointLight_Effect.Parameters["InstanceInverseMatrix"];
            deferredPointLightParameter_InstanceScale = deferredPointLight_Effect.Parameters["InstanceScale"];
            deferredPointLightParameter_InstanceSDFIndex = deferredPointLight_Effect.Parameters["InstanceSDFIndex"];
            deferredPointLightParameter_InstancesCount = deferredPointLight_Effect.Parameters["InstancesCount"];

            deferredPointLightParameter_NoiseMap = deferredPointLight_Effect.Parameters["NoiseMap"];
            deferredPointLightParameter_AlbedoMap = deferredPointLight_Effect.Parameters["AlbedoMap"];
            deferredPointLightParameter_NormalMap = deferredPointLight_Effect.Parameters["NormalMap"];
            deferredPointLightParameter_DepthMap = deferredPointLight_Effect.Parameters["DepthMap"];
        }

        /// <summary>
        /// Draw the point lights, set up some stuff first
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="cameraOrigin"></param>
        /// <param name="gameTime"></param>
        public void Draw(List<DeferredPointLight> pointLights, Vector3 cameraOrigin, GameTime gameTime, Assets assets, BoundingFrustum _boundingFrustum, bool _viewProjectionHasChanged, Matrix _view, Matrix _viewProjection, Matrix _inverseView, GraphicsDevice _graphicsDevice)
        {

            if (pointLights.Count < 1) return;

            ModelMeshPart meshpart = assets.SphereMeshPart;
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            if (RenderingSettings.g_VolumetricLights)
                deferredPointLightParameter_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            for (int index = 0; index < pointLights.Count; index++)
            {
                DeferredPointLight light = pointLights[index];
                DrawPointLight(light, cameraOrigin, vertexOffset, startIndex, primitiveCount, _boundingFrustum, _viewProjectionHasChanged, _view, _viewProjection, _inverseView, _graphicsDevice);
            }
        }

        /// <summary>
        /// Draw each individual point lights
        /// </summary>
        /// <param name="light"></param>
        /// <param name="cameraOrigin"></param>
        private void DrawPointLight(DeferredPointLight light, Vector3 cameraOrigin, int vertexOffset, int startIndex, int primitiveCount, BoundingFrustum _boundingFrustum, bool _viewProjectionHasChanged, Matrix _view, Matrix _viewProjection, Matrix _inverseView, GraphicsDevice _graphicsDevice)
        {
            if (!light.IsEnabled) return;

            //first let's check if the light is even in bounds
            if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint ||
                !_boundingFrustum.Intersects(light.BoundingSphere))
                return;

            //For our stats
            RenderingStats.LightsDrawn++;

            //Send the light parameters to the shader
            if (_viewProjectionHasChanged)
            {
                light.LightViewSpace = light.WorldMatrix * _view;
                light.LightWorldViewProj = light.WorldMatrix * _viewProjection;
            }

            deferredPointLightParameter_WorldView.SetValue(light.LightViewSpace);
            deferredPointLightParameter_WorldViewProjection.SetValue(light.LightWorldViewProj);
            deferredPointLightParameter_LightPosition.SetValue(light.LightViewSpace.Translation);
            deferredPointLightParameter_LightColor.SetValue(light.ColorV3);
            deferredPointLightParameter_LightRadius.SetValue(light.Radius);
            deferredPointLightParameter_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            deferredPointLightParameter_Inside.SetValue(inside);

            if (RenderingSettings.g_UseDepthStencilLightCulling == 2)
            {
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                //draw front faces
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                deferredPointLight_WriteStencil.Passes[0].Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);

                ////////////

                _graphicsDevice.DepthStencilState = _stencilCullPass2;
                //draw backfaces
                _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                ApplyShader(light);

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }
            else
            {
                //If we are inside compute the backfaces, otherwise frontfaces of the sphere
                _graphicsDevice.RasterizerState = inside > 0 ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;

                ApplyShader(light);

                _graphicsDevice.DepthStencilState = RenderingSettings.g_UseDepthStencilLightCulling > 0 && !light.IsVolumetric && inside < 0 ? DepthStencilState.DepthRead : DepthStencilState.None;

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }

            //Draw the sphere
        }

        private void ApplyShader(DeferredPointLight light)
        {
            // Experimental
            if (light.CastSDFShadows)
            {
                deferredPointLight_ShadowedSDF.Passes[0].Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                deferredPointLightParameter_ShadowMap.SetValue(light.ShadowMap);
                deferredPointLightParameter_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                deferredPointLightParameter_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && RenderingSettings.g_VolumetricLights)
                {
                    deferredPointLightParameter_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    deferredPointLight_ShadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    deferredPointLight_Shadowed.Passes[0].Apply();
                }
            }
            else
            {
                //todo: remove

                deferredPointLightParameter_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && RenderingSettings.g_VolumetricLights)
                {
                    deferredPointLightParameter_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    deferredPointLight_UnshadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    deferredPointLight_Unshadowed.Passes[0].Apply();
                }
            }
        }

        public void Dispose()
        {
            deferredPointLight_Effect?.Dispose();
            _stencilCullPass1?.Dispose();
            _stencilCullPass2?.Dispose();
        }
    }
}
