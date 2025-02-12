﻿
using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.SDF
{

    //Just a template
    public partial class DistanceFieldRenderModule : PipelineModule, IDisposable
    {

        private DistanceFieldEffectSetup _effectSetup = new DistanceFieldEffectSetup();

        public Generator _sdfGenerator;
        public PointLightPipelineModule PointLightRenderModule;
        public EnvironmentPipelineModule EnvironmentProbeRenderModule;

        public RenderTarget2D AtlasTarget { get; protected set; }


        private const int InstanceMaxCount = 40;

        private Matrix[] _instanceInverseMatrixArray = new Matrix[InstanceMaxCount];
        private Vector3[] _instanceScaleArray = new Vector3[InstanceMaxCount];
        private float[] _instanceSDFIndexArray = new float[InstanceMaxCount];
        private int _instancesCount = 0;

        private Vector3[] _volumeTexSizeArray = new Vector3[40];
        private Vector4[] _volumeTexResolutionArray = new Vector4[40];

        private List<SignedDistanceField> _sdfDefinitions;
        private SignedDistanceField[] _signedDistanceFieldDefinitions = new SignedDistanceField[40];
        private int _signedDistanceFieldDefinitionsCount = 0;


        public Vector3 ViewPosition { set { _effectSetup.Param_CameraPositon.SetValue(value); } }
        public Texture2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }
        public Vector3 MeshOffset { set { _effectSetup.Param_MeshOffset.SetValue(value); } }


        public DistanceFieldRenderModule()
        {
            _sdfGenerator = new Generator();
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
        public void SetViewPosition(Vector3 viewPosition)
        {
            _effectSetup.Param_CameraPositon.SetValue(viewPosition);
        }


        public void DrawDistance()
        {
            if (RenderingSettings.SDF.DrawDistance)
            {
                _effectSetup.Param_FrustumCorners.SetValue(this.Frustum.WorldSpaceFrustum);
                _effectSetup.Pass_Distance.Apply();
                FullscreenTriangleBuffer.Instance.Draw(_graphicsDevice);
            }
        }
        public void DrawVolume()
        {
            if (RenderingSettings.SDF.DrawVolume)
            {
                _effectSetup.Param_FrustumCorners.SetValue(this.Frustum.WorldSpaceFrustum);
                _effectSetup.Pass_Volume.Apply();
                FullscreenTriangleBuffer.Instance.Draw(_graphicsDevice);
            }
        }

        public void UpdateDistanceFieldTransformations(List<ModelEntity> entities)
        {
            if (!RenderingSettings.SDF.Draw)
                return;

            //First of all let's build the atlas
            UpdateAtlas(_sdfDefinitions);

            int i = 0;
            for (var index = 0; index < entities.Count; index++)
            {
                ModelEntity entity = entities[index];
                SdfModelDefinition sdfModelDefinition = entity.ModelDefinition as SdfModelDefinition;
                if (sdfModelDefinition != null && sdfModelDefinition.SDF.IsUsed)
                {
                    _instanceInverseMatrixArray[i] = entity.InverseWorld;
                    _instanceScaleArray[i] = entity.Scale;
                    _instanceSDFIndexArray[i] = sdfModelDefinition.SDF.ArrayIndex;

                    i++;

                    if (i >= InstanceMaxCount) break;
                }

            }

            _instancesCount = i;

            //Submit Instances
            this.SetInstanceData(_instanceInverseMatrixArray, _instanceScaleArray, _instanceSDFIndexArray, _instancesCount);
            PointLightRenderModule.SetInstanceData(_instanceInverseMatrixArray, _instanceScaleArray, _instanceSDFIndexArray, _instancesCount);
            EnvironmentProbeRenderModule.SetInstanceData(_instanceInverseMatrixArray, _instanceScaleArray, _instanceSDFIndexArray, _instancesCount);
        }
        private void UpdateAtlas(List<SignedDistanceField> sdfDefinitionsPassed)
        {
            if (sdfDefinitionsPassed.Count < 1) return;

            bool updateAtlas = false;

            if (_signedDistanceFieldDefinitions == null || sdfDefinitionsPassed.Count !=
                _signedDistanceFieldDefinitionsCount)
            {
                _signedDistanceFieldDefinitionsCount = 0;
                updateAtlas = true;
            }


            {
                for (int i = 0; i < sdfDefinitionsPassed.Count; i++)
                {
                    bool found = false;
                    for (int j = 0; j < _signedDistanceFieldDefinitionsCount; j++)
                    {
                        if (sdfDefinitionsPassed[i] == _signedDistanceFieldDefinitions[j])
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        _signedDistanceFieldDefinitions[_signedDistanceFieldDefinitionsCount] = sdfDefinitionsPassed[i];
                        sdfDefinitionsPassed[i].ArrayIndex = _signedDistanceFieldDefinitionsCount;
                        _signedDistanceFieldDefinitionsCount++;

                        updateAtlas = true;
                    }
                }
            }

            //Now build the atlas

            if (!updateAtlas) return;

            AtlasTarget?.Dispose();

            int x = 0, y = 0;
            //Count size
            for (int i = 0; i < _signedDistanceFieldDefinitionsCount; i++)
            {
                x = (int)Math.Max(_signedDistanceFieldDefinitions[i].SdfTexture.Width, x);
                _signedDistanceFieldDefinitions[i].TextureResolution.W = y;
                y += _signedDistanceFieldDefinitions[i].SdfTexture.Height;

                _volumeTexResolutionArray[i] = _signedDistanceFieldDefinitions[i].TextureResolution;
                _volumeTexSizeArray[i] = _signedDistanceFieldDefinitions[i].VolumeSize;
            }

            AtlasTarget = new RenderTarget2D(_graphicsDevice, x, y, false, SurfaceFormat.HalfSingle, DepthFormat.None);

            _graphicsDevice.SetRenderTarget(AtlasTarget);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            for (int i = 0; i < _signedDistanceFieldDefinitionsCount; i++)
            {
                _spriteBatch.Draw(_signedDistanceFieldDefinitions[i].SdfTexture,
                    new Rectangle(0, (int)_signedDistanceFieldDefinitions[i].TextureResolution.W, _signedDistanceFieldDefinitions[i].SdfTexture.Width, _signedDistanceFieldDefinitions[i].SdfTexture.Height), Color.White);
            }
            _spriteBatch.End();


            //Atlas
            this.SetVolumeTexParams(AtlasTarget, _volumeTexSizeArray, _volumeTexResolutionArray);
            PointLightRenderModule.SetVolumeTexParams(AtlasTarget, _volumeTexSizeArray, _volumeTexResolutionArray);
            EnvironmentProbeRenderModule.SetVolumeTexParams(AtlasTarget, _volumeTexSizeArray, _volumeTexResolutionArray);
        }

        public RenderTarget2D CreateSDFTexture(GraphicsDevice graphics, Texture2D triangleData, Vector3 steps, SignedDistanceField sdf, int trianglesLength)
        {
            int xsteps = (int)steps.X;
            int ysteps = (int)steps.Y;
            int zsteps = (int)steps.Z;

            RenderTarget2D output = new RenderTarget2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(output);

            //Offset isntead of position!
            _volumeTexResolutionArray[0] = new Vector4(xsteps, ysteps, zsteps, 0);
            _volumeTexSizeArray[0] = sdf.VolumeSize;

            this.SetVolumeTexParams(triangleData, _volumeTexSizeArray, _volumeTexResolutionArray);

            MeshOffset = sdf.Offset;

            _effectSetup.Param_TriangleTexResolution.SetValue(new Vector2(triangleData.Width, triangleData.Height));
            _effectSetup.Param_TriangleAmount.SetValue((float)trianglesLength);

            _effectSetup.Pass_GenerateSDF.Apply();
            FullscreenTriangleBuffer.Instance.Draw(graphics);

            _signedDistanceFieldDefinitionsCount = -1;

            return output;
        }

        public void UpdateSdfGenerator(List<ModelEntity> entities)
        {
            _sdfGenerator.Update(_graphicsDevice, entities, this, ref _sdfDefinitions);
        }

        public override void Dispose()
        {
            _effectSetup.Dispose();
        }

    }

}
