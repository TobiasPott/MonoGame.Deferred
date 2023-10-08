using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Renderer.RenderModules.DeferredLighting
{
    public class PointLightRenderModule : IDisposable
    {

        private Effect Effect;

        private EffectTechnique Technique_Unshadowed;
        private EffectTechnique Technique_UnshadowedVolumetric;
        private EffectTechnique Technique_ShadowedSDF;
        private EffectTechnique Technique_Shadowed;
        private EffectTechnique Technique_ShadowedVolumetric;
        private EffectTechnique Technique_WriteStencil;

        private EffectParameter Param_ShadowMap;

        public EffectParameter Param_Resolution;
        private EffectParameter Param_WorldView;
        private EffectParameter Param_WorldViewProjection;
        public EffectParameter Param_InverseView;

        private EffectParameter Param_LightPosition;
        private EffectParameter Param_LightColor;
        private EffectParameter Param_LightRadius;
        private EffectParameter Param_LightIntensity;
        private EffectParameter Param_ShadowMapSize;
        private EffectParameter Param_ShadowMapRadius;
        private EffectParameter Param_Inside;
        private EffectParameter Param_Time;
        public EffectParameter Param_FarClip;
        private EffectParameter Param_LightVolumeDensity;

        public EffectParameter Param_VolumeTexParam;
        public EffectParameter Param_VolumeTexSizeParam;
        public EffectParameter Param_VolumeTexResolution;

        public EffectParameter Param_InstanceInverseMatrix;
        public EffectParameter Param_InstanceScale;
        public EffectParameter Param_InstanceSDFIndex;
        public EffectParameter Param_InstancesCount;

        private EffectParameter Param_AlbedoMap;
        private EffectParameter Param_NormalMap;
        private EffectParameter Param_DepthMap;

        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;

        public PointLightRenderModule(ContentManager content, string shaderPath = "Shaders/Deferred/DeferredPointLight")
        {
            Load(content, shaderPath);

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

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            Param_NormalMap.SetValue(gBufferTarget.Normal);
            Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            this.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            this.Param_InstanceScale.SetValue(scales);
            this.Param_InstanceSDFIndex.SetValue(sdfIndices);
            this.Param_InstancesCount.SetValue((float)count);
        }

        private void Load(ContentManager content, string shaderPath = "Shaders/Deferred/DeferredPointLight")
        {
            Effect = content.Load<Effect>(shaderPath);

            Technique_Unshadowed = Effect.Techniques["Unshadowed"];
            Technique_UnshadowedVolumetric = Effect.Techniques["UnshadowedVolume"];
            Technique_Shadowed = Effect.Techniques["Shadowed"];
            Technique_ShadowedSDF = Effect.Techniques["ShadowedSDF"];
            Technique_ShadowedVolumetric = Effect.Techniques["ShadowedVolume"];
            Technique_WriteStencil = Effect.Techniques["WriteStencilMask"];

            Param_ShadowMap = Effect.Parameters["ShadowMap"];
            
            Param_Resolution = Effect.Parameters["Resolution"];
            Param_WorldView = Effect.Parameters["WorldView"];
            Param_WorldViewProjection = Effect.Parameters["WorldViewProj"];
            Param_InverseView = Effect.Parameters["InverseView"];

            Param_LightPosition = Effect.Parameters["lightPosition"];
            Param_LightColor = Effect.Parameters["lightColor"];
            Param_LightRadius = Effect.Parameters["lightRadius"];
            Param_LightIntensity = Effect.Parameters["lightIntensity"];
            Param_ShadowMapSize = Effect.Parameters["ShadowMapSize"];
            Param_ShadowMapRadius = Effect.Parameters["ShadowMapRadius"];
            Param_Inside = Effect.Parameters["inside"];
            Param_Time = Effect.Parameters["Time"];
            Param_FarClip = Effect.Parameters["FarClip"];
            Param_LightVolumeDensity = Effect.Parameters["lightVolumeDensity"];

            Param_VolumeTexParam = Effect.Parameters["VolumeTex"];
            Param_VolumeTexSizeParam = Effect.Parameters["VolumeTexSize"];
            Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];
            Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            Param_InstanceScale = Effect.Parameters["InstanceScale"];
            Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            Param_InstancesCount = Effect.Parameters["InstancesCount"];

            Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
            Param_NormalMap = Effect.Parameters["NormalMap"];
            Param_DepthMap = Effect.Parameters["DepthMap"];
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
                Param_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

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

            Param_WorldView.SetValue(light.LightViewSpace);
            Param_WorldViewProjection.SetValue(light.LightWorldViewProj);
            Param_LightPosition.SetValue(light.LightViewSpace.Translation);
            Param_LightColor.SetValue(light.ColorV3);
            Param_LightRadius.SetValue(light.Radius);
            Param_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            Param_Inside.SetValue(inside);

            if (RenderingSettings.g_UseDepthStencilLightCulling == 2)
            {
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                //draw front faces
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                Technique_WriteStencil.Passes[0].Apply();

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
                Technique_ShadowedSDF.Passes[0].Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                Param_ShadowMap.SetValue(light.ShadowMap);
                Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                Param_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && RenderingSettings.g_VolumetricLights)
                {
                    Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    Technique_ShadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    Technique_Shadowed.Passes[0].Apply();
                }
            }
            else
            {
                //todo: remove

                Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && RenderingSettings.g_VolumetricLights)
                {
                    Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    Technique_UnshadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    Technique_Unshadowed.Passes[0].Apply();
                }
            }
        }

        public void Dispose()
        {
            Effect?.Dispose();
            _stencilCullPass1?.Dispose();
            _stencilCullPass2?.Dispose();
        }
    }
}
