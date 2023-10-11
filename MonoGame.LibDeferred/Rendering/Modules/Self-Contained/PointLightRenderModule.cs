using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.DeferredLighting
{
    public class PointLightRenderModule : PipelineModule
    {

        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;

        public PointLightRenderModule(ContentManager content, string shaderPath)
            : base(content, shaderPath)
        {
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

        protected override void Load(ContentManager content, string shaderPath)
        {

        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            Shaders.DeferredPointLight.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            Shaders.DeferredPointLight.Param_NormalMap.SetValue(gBufferTarget.Normal);
            Shaders.DeferredPointLight.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            Shaders.DeferredPointLight.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            Shaders.DeferredPointLight.Param_InstanceScale.SetValue(scales);
            Shaders.DeferredPointLight.Param_InstanceSDFIndex.SetValue(sdfIndices);
            Shaders.DeferredPointLight.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            Shaders.DeferredPointLight.Param_VolumeTex.SetValue(atlas);
            Shaders.DeferredPointLight.Param_VolumeTexSize.SetValue(texSizes);
            Shaders.DeferredPointLight.Param_VolumeTexResolution.SetValue(texResolutions);
        }


        /// <summary>
        /// Draw the point lights, set up some stuff first
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="cameraOrigin"></param>
        /// <param name="gameTime"></param>
        public void Draw(List<DeferredPointLight> pointLights, Vector3 cameraOrigin, GameTime gameTime, BoundingFrustum _boundingFrustum, bool _viewProjectionHasChanged, Matrix _view, Matrix _viewProjection)
        {
            if (pointLights.Count < 1) return;

            ModelMeshPart meshpart = StaticAssets.Instance.SphereMeshPart;
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            if (RenderingSettings.g_VolumetricLights)
                Shaders.DeferredPointLight.Param_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            for (int index = 0; index < pointLights.Count; index++)
            {
                DeferredPointLight light = pointLights[index];
                DrawPointLight(light, cameraOrigin, vertexOffset, startIndex, primitiveCount, _boundingFrustum, _viewProjectionHasChanged, _view, _viewProjection);
            }
        }

        /// <summary>
        /// Draw each individual point lights
        /// </summary>
        private void DrawPointLight(DeferredPointLight light, Vector3 cameraOrigin, int vertexOffset, int startIndex, int primitiveCount, BoundingFrustum _boundingFrustum, bool _viewProjectionHasChanged, Matrix _view, Matrix _viewProjection)
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

            Shaders.DeferredPointLight.Param_WorldView.SetValue(light.LightViewSpace);
            Shaders.DeferredPointLight.Param_WorldViewProjection.SetValue(light.LightWorldViewProj);
            Shaders.DeferredPointLight.Param_LightPosition.SetValue(light.LightViewSpace.Translation);
            Shaders.DeferredPointLight.Param_LightColor.SetValue(light.ColorV3);
            Shaders.DeferredPointLight.Param_LightRadius.SetValue(light.Radius);
            Shaders.DeferredPointLight.Param_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            Shaders.DeferredPointLight.Param_Inside.SetValue(inside);

            if (RenderingSettings.g_UseDepthStencilLightCulling == 2)
            {
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                //draw front faces
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                Shaders.DeferredPointLight.Technique_WriteStencil.Passes[0].Apply();

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
                Shaders.DeferredPointLight.Technique_ShadowedSDF.Passes[0].Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                Shaders.DeferredPointLight.Param_ShadowMap.SetValue(light.ShadowMap);
                Shaders.DeferredPointLight.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                Shaders.DeferredPointLight.Param_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && RenderingSettings.g_VolumetricLights)
                {
                    Shaders.DeferredPointLight.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    Shaders.DeferredPointLight.Technique_ShadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    Shaders.DeferredPointLight.Technique_Shadowed.Passes[0].Apply();
                }
            }
            else
            {
                Shaders.DeferredPointLight.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && RenderingSettings.g_VolumetricLights)
                {
                    Shaders.DeferredPointLight.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    Shaders.DeferredPointLight.Technique_UnshadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    Shaders.DeferredPointLight.Technique_Unshadowed.Passes[0].Apply();
                }
            }
        }

        public override void Dispose()
        {
            _stencilCullPass1?.Dispose();
            _stencilCullPass2?.Dispose();
        }
    }
}
