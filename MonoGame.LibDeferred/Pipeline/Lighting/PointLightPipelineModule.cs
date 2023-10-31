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


        private readonly PointLightFxSetup _fxSetup = new PointLightFxSetup();
        private float _time;
        private Vector3 _viewOrigin;

        private readonly DepthStencilState _stencilCullPass1;
        private readonly DepthStencilState _stencilCullPass2;


        public float Time { set { _time = value; } }
        public Vector3 ViewOrigin { set => _viewOrigin = value; get => _viewOrigin; }

        public Vector2 Resolution { set { _fxSetup.Param_Resolution.SetValue(value); } }


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
            _fxSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _fxSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _fxSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            _fxSetup.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            _fxSetup.Param_InstanceScale.SetValue(scales);
            _fxSetup.Param_InstanceSDFIndex.SetValue(sdfIndices);
            _fxSetup.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            _fxSetup.Param_VolumeTex.SetValue(atlas);
            _fxSetup.Param_VolumeTexSize.SetValue(texSizes);
            _fxSetup.Param_VolumeTexResolution.SetValue(texResolutions);
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

            if (PointLightPipelineModule.g_VolumetricLights)
                _fxSetup.Param_Time.SetValue(_time);
            _fxSetup.Param_InverseView.SetValue(Matrices.InverseView);

            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);

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

            _fxSetup.Param_WorldView.SetValue(light.Matrices.ViewSpace);
            _fxSetup.Param_WorldViewProjection.SetValue(light.Matrices.WorldViewProj);
            _fxSetup.Param_LightPosition.SetValue(light.Matrices.ViewSpace.Translation);

            _fxSetup.Param_LightColor.SetValue(light.Color_sRGB);
            _fxSetup.Param_LightRadius.SetValue(light.Radius);
            _fxSetup.Param_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(this.ViewOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            _fxSetup.Param_Inside.SetValue(inside);

            if (LightingPipelineModule.g_UseDepthStencilLightCulling == 2)
            {
                //draw front faces
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);

                _fxSetup.Pass_WriteStencil.Apply();

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
                _fxSetup.Pass_ShadowedSDF.Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                _fxSetup.Param_ShadowMap.SetValue(light.ShadowMap);
                _fxSetup.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                _fxSetup.Param_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && PointLightPipelineModule.g_VolumetricLights)
                {
                    _fxSetup.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    _fxSetup.Pass_ShadowedVolumetric.Apply();
                }
                else
                {
                    _fxSetup.Pass_Shadowed.Apply();
                }
            }
            else
            {
                _fxSetup.Param_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && PointLightPipelineModule.g_VolumetricLights)
                {
                    _fxSetup.Param_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    _fxSetup.Pass_UnshadowedVolumetric.Apply();
                }
                else
                {
                    _fxSetup.Pass_Unshadowed.Apply();
                }
            }
        }

        public override void Dispose()
        {
            _stencilCullPass1?.Dispose();
            _stencilCullPass2?.Dispose();
            _fxSetup?.Dispose();
        }
    }
}
