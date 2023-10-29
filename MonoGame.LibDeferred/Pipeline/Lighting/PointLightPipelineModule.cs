using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class PointLightPipelineModule : PipelineModule
    {
        public static bool g_VolumetricLights = true;


        private PointLightFxSetup _effectSetup = new PointLightFxSetup();
        private GameTime _gameTime;
        private Vector3 _viewOrigin;

        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;


        public GameTime GameTime { set { _gameTime = value; } }
        public Vector3 ViewOrigin { set => _viewOrigin = value; get => _viewOrigin; }

        public Vector2 Resolution { set { _effectSetup.Param_Resolution.SetValue(value); } }


        public PointLightPipelineModule()
            : base()
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
        public void Draw(List<PointLight> pointLights, bool viewProjectionHasChanged)
        {
            if (pointLights.Count < 1) return;

            ModelMeshPart meshpart = StaticAssets.Instance.SphereMeshPart;
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = meshpart.IndexBuffer;

            if (PointLightPipelineModule.g_VolumetricLights && _gameTime != null)
                _effectSetup.Param_Time.SetValue((float)_gameTime.TotalGameTime.TotalSeconds % 1000);
            _effectSetup.Param_InverseView.SetValue(Matrices.InverseView);
           
            _effectSetup.Param_FarClip.SetValue(this.Frustum.FarClip);

            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            for (int index = 0; index < pointLights.Count; index++)
            {
                DrawPointLight(pointLights[index], vertexOffset, startIndex, primitiveCount, viewProjectionHasChanged, this.Matrices.View, this.Matrices.ViewProjection);
            }
        }

        /// <summary>
        /// Draw each individual point lights
        /// </summary>
        private void DrawPointLight(PointLight light, int vertexOffset, int startIndex, int primitiveCount, bool viewProjectionHasChanged, Matrix view, Matrix viewProjection)
        {
            if (!light.IsEnabled) return;

            //first let's check if the light is even in bounds
            if (this.Frustum.Frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint || !this.Frustum.Frustum.Intersects(light.BoundingSphere))
                return;

            //For our stats
            RenderingStats.LightsDrawn++;

            //Send the light parameters to the shader
            if (viewProjectionHasChanged)
            {
                light.Matrices.ViewSpace = light.Matrices.WorldMatrix * view;
                light.Matrices.WorldViewProj = light.Matrices.WorldMatrix * viewProjection;
            }

            _effectSetup.Param_WorldView.SetValue(light.Matrices.ViewSpace);
            _effectSetup.Param_WorldViewProjection.SetValue(light.Matrices.WorldViewProj);
            _effectSetup.Param_LightPosition.SetValue(light.Matrices.ViewSpace.Translation);

            _effectSetup.Param_LightColor.SetValue(light.Color_sRGB);
            _effectSetup.Param_LightRadius.SetValue(light.Radius);
            _effectSetup.Param_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(this.ViewOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            _effectSetup.Param_Inside.SetValue(inside);

            if (LightingPipelineModule.g_UseDepthStencilLightCulling == 2)
            {
                //draw front faces
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);

                _effectSetup.Pass_WriteStencil.Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);

                ////////////
                //draw backfaces
                _graphicsDevice.DepthStencilState = _stencilCullPass2;
                _graphicsDevice.SetState(RasterizerStateOption.CullClockwise);

                ApplyShader(light);

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }
            else
            {
                ApplyShader(light);
                //If we are inside compute the backfaces, otherwise frontfaces of the sphere
                bool isDepthRead = LightingPipelineModule.g_UseDepthStencilLightCulling > 0 && !light.IsVolumetric && inside < 0;
                _graphicsDevice.SetStates(isDepthRead ? DepthStencilStateOption.DepthRead : DepthStencilStateOption.None,
                    inside > 0 ? RasterizerStateOption.CullClockwise : RasterizerStateOption.CullCounterClockwise, BlendStateOption.KeepState);

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }

            //Draw the sphere
        }

        private void ApplyShader(PointLight light)
        {
            // Experimental
            if (light.CastSDFShadows)
            {
                _effectSetup.Pass_ShadowedSDF.Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                _effectSetup.Param_ShadowMap.SetValue(light.ShadowMap);
                _effectSetup.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                _effectSetup.Param_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && PointLightPipelineModule.g_VolumetricLights)
                {
                    _effectSetup.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    _effectSetup.Pass_ShadowedVolumetric.Apply();
                }
                else
                {
                    _effectSetup.Pass_Shadowed.Apply();
                }
            }
            else
            {
                _effectSetup.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && PointLightPipelineModule.g_VolumetricLights)
                {
                    _effectSetup.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    _effectSetup.Pass_UnshadowedVolumetric.Apply();
                }
                else
                {
                    _effectSetup.Pass_Unshadowed.Apply();
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
