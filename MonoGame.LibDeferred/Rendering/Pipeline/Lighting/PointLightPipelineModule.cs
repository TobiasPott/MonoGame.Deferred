using DeferredEngine.Recources;
using DeferredEngine.Renderer;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline.Lighting
{
    public class PointLightPipelineModule : PipelineModule
    {
        public static bool g_VolumetricLights = true;


        private PointLightEffectSetup _effectSetup = new PointLightEffectSetup();
        private BoundingFrustum _frustum;
        private GameTime _gameTime;

        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;


        public BoundingFrustum Frustum { set { _frustum = value; } }
        public GameTime GameTime { set { _gameTime = value; } }

        public float FarClip { set { _effectSetup.Param_FarClip.SetValue(value); } }
        public Matrix InverseView { set { _effectSetup.Param_InverseView.SetValue(value); } }
        public Vector2 Resolution { set { _effectSetup.Param_Resolution.SetValue(value); } }


        public PointLightPipelineModule(ContentManager content, string shaderPath)
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
            _effectSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _effectSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _effectSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            _effectSetup.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            _effectSetup.Param_InstanceScale.SetValue(scales);
            _effectSetup.Param_InstanceSDFIndex.SetValue(sdfIndices);
            _effectSetup.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            _effectSetup.Param_VolumeTex.SetValue(atlas);
            _effectSetup.Param_VolumeTexSize.SetValue(texSizes);
            _effectSetup.Param_VolumeTexResolution.SetValue(texResolutions);
        }


        /// <summary>
        /// Draw the point lights, set up some stuff first
        /// </summary>
        public void Draw(List<DeferredPointLight> pointLights, Vector3 cameraOrigin, PipelineMatrices matrices, bool viewProjectionHasChanged)
        {
            if (pointLights.Count < 1) return;

            ModelMeshPart meshpart = StaticAssets.Instance.SphereMeshPart;
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            if (PointLightPipelineModule.g_VolumetricLights && _gameTime != null)
                _effectSetup.Param_Time.SetValue((float)_gameTime.TotalGameTime.TotalSeconds % 1000);

            for (int index = 0; index < pointLights.Count; index++)
            {
                DrawPointLight(pointLights[index], cameraOrigin, vertexOffset, startIndex, primitiveCount, viewProjectionHasChanged, matrices.View, matrices.ViewProjection);
            }
        }

        /// <summary>
        /// Draw each individual point lights
        /// </summary>
        private void DrawPointLight(DeferredPointLight light, Vector3 cameraOrigin, int vertexOffset, int startIndex, int primitiveCount, bool viewProjectionHasChanged, Matrix view, Matrix viewProjection)
        {
            if (!light.IsEnabled) return;

            //first let's check if the light is even in bounds
            if (_frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint || !_frustum.Intersects(light.BoundingSphere))
                return;

            //For our stats
            RenderingStats.LightsDrawn++;

            //Send the light parameters to the shader
            if (viewProjectionHasChanged)
            {
                light.LightViewSpace = light.WorldMatrix * view;
                light.LightWorldViewProj = light.WorldMatrix * viewProjection;
            }

            _effectSetup.Param_WorldView.SetValue(light.LightViewSpace);
            _effectSetup.Param_WorldViewProjection.SetValue(light.LightWorldViewProj);
            _effectSetup.Param_LightPosition.SetValue(light.LightViewSpace.Translation);
            _effectSetup.Param_LightColor.SetValue(light.ColorV3);
            _effectSetup.Param_LightRadius.SetValue(light.Radius);
            _effectSetup.Param_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            _effectSetup.Param_Inside.SetValue(inside);

            if (LightingPipelineModule.g_UseDepthStencilLightCulling == 2)
            {
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                //draw front faces
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                _effectSetup.Technique_WriteStencil.Passes[0].Apply();

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

                _graphicsDevice.DepthStencilState = LightingPipelineModule.g_UseDepthStencilLightCulling > 0 && !light.IsVolumetric && inside < 0 ? DepthStencilState.DepthRead : DepthStencilState.None;

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }

            //Draw the sphere
        }

        private void ApplyShader(DeferredPointLight light)
        {
            // Experimental
            if (light.CastSDFShadows)
            {
                _effectSetup.Technique_ShadowedSDF.Passes[0].Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                _effectSetup.Param_ShadowMap.SetValue(light.ShadowMap);
                _effectSetup.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                _effectSetup.Param_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && PointLightPipelineModule.g_VolumetricLights)
                {
                    _effectSetup.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    _effectSetup.Technique_ShadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    _effectSetup.Technique_Shadowed.Passes[0].Apply();
                }
            }
            else
            {
                _effectSetup.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && PointLightPipelineModule.g_VolumetricLights)
                {
                    _effectSetup.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    _effectSetup.Technique_UnshadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    _effectSetup.Technique_Unshadowed.Passes[0].Apply();
                }
            }
        }

        public override void Dispose()
        {
            _stencilCullPass1?.Dispose();
            _stencilCullPass2?.Dispose();
            _effectSetup?.Dispose();
        }
    }
}
