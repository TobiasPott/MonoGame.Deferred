
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.DeferredLighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DeferredEngine.Renderer.RenderModules.SDF
{
    //Just a template
    public class DistanceFieldRenderModule : IDisposable
    {
        private RenderTarget2D _atlasRenderTarget2D;

        private Effect Effect;

        private EffectPass Pass_GenerateSDF;
        private EffectPass Pass_Volume;
        private EffectPass Pass_Distance;

        private EffectParameter Param_FrustumCorners;
        private EffectParameter Param_CameraPositon;
        private EffectParameter Param_DepthMap;
        private EffectParameter Param_MeshOffset;

        private EffectParameter Param_VolumeTex;
        private EffectParameter Param_VolumeTexSize;
        private EffectParameter Param_VolumeTexResolution;

        private EffectParameter Param_InstanceInverseMatrix;
        private EffectParameter Param_InstanceScale;
        private EffectParameter Param_InstanceSDFIndex;
        private EffectParameter Param_InstancesCount;

        private EffectParameter Param_TriangleTexResolution;
        private EffectParameter Param_TriangleAmount;


        public PointLightRenderModule PointLightRenderModule;
        public EnvironmentPipelineModule EnvironmentProbeRenderModule;

        private const int InstanceMaxCount = 40;

        private Matrix[] _instanceInverseMatrixArray = new Matrix[InstanceMaxCount];
        private Vector3[] _instanceScaleArray = new Vector3[InstanceMaxCount];
        private float[] _instanceSDFIndexArray = new float[InstanceMaxCount];
        private int _instancesCount = 0;

        private Vector3[] _volumeTexSizeArray = new Vector3[40];
        private Vector4[] _volumeTexResolutionArray = new Vector4[40];

        private SignedDistanceField[] _signedDistanceFieldDefinitions = new SignedDistanceField[40];
        private int _signedDistanceFieldDefinitionsCount = 0;

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        public Vector3[] FrustumCornersWorldSpace { set { Param_FrustumCorners.SetValue(value); } }
        public Vector3 CameraPosition { set { Param_CameraPositon.SetValue(value); } }
        public Texture2D DepthMap { set { Param_DepthMap.SetValue(value); } }


        public Vector3 MeshOffset
        {
            set { Param_MeshOffset.SetValue(value); }
        }

        public DistanceFieldRenderModule(ContentManager content, string shaderPath = "Shaders/SignedDistanceFields/volumeProjection")
        {
            Load(content, shaderPath);
        }

        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            this.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            this.Param_InstanceScale.SetValue(scales);
            this.Param_InstanceSDFIndex.SetValue(sdfIndices);
            this.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            this.Param_VolumeTex.SetValue(atlas);
            this.Param_VolumeTexSize.SetValue(texSizes);
            this.Param_VolumeTexResolution.SetValue(texResolutions);
        }


        public void Load(ContentManager content, string shaderPath = "Shaders/SignedDistanceFields/volumeProjection")
        {
            Effect = content.Load<Effect>(shaderPath);

            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_CameraPositon = Effect.Parameters["CameraPosition"];
            Param_DepthMap = Effect.Parameters["DepthMap"];

            Param_VolumeTex = Effect.Parameters["VolumeTex"];
            Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
            Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];

            Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            Param_InstanceScale = Effect.Parameters["InstanceScale"];
            Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            Param_InstancesCount = Effect.Parameters["InstancesCount"];

            Param_MeshOffset = Effect.Parameters["MeshOffset"];
            Param_TriangleTexResolution = Effect.Parameters["TriangleTexResolution"];
            Param_TriangleAmount = Effect.Parameters["TriangleAmount"];

            Pass_Distance = Effect.Techniques["Distance"].Passes[0];
            Pass_Volume = Effect.Techniques["Volume"].Passes[0];
            Pass_GenerateSDF = Effect.Techniques["GenerateSDF"].Passes[0];
        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        public void Dispose()
        {
            Effect?.Dispose();
        }

        public void Draw(Camera camera)
        {
            CameraPosition = camera.Position;

            if (RenderingSettings.sdf_drawvolume)
                Pass_Volume.Apply();
            else
                Pass_Distance.Apply();
            FullscreenTriangleBuffer.Instance.Draw(_graphicsDevice);
        }

        public void UpdateDistanceFieldTransformations(List<ModelEntity> entities, List<SignedDistanceField> sdfDefinitions)
        {
            if (!RenderingSettings.sdf_draw) return;

            //First of all let's build the atlas
            UpdateAtlas(sdfDefinitions);

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

                            if (sdfDefinitionsPassed[i].NeedsToBeGenerated) throw new Exception("test");
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

            _atlasRenderTarget2D?.Dispose();

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

            //todo: Check if we can use half here
            _atlasRenderTarget2D = new RenderTarget2D(_graphicsDevice, x, y, false, SurfaceFormat.Single, DepthFormat.None);

            _graphicsDevice.SetRenderTarget(_atlasRenderTarget2D);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            for (int i = 0; i < _signedDistanceFieldDefinitionsCount; i++)
            {
                _spriteBatch.Draw(_signedDistanceFieldDefinitions[i].SdfTexture,
                    new Rectangle(0, (int)_signedDistanceFieldDefinitions[i].TextureResolution.W, _signedDistanceFieldDefinitions[i].SdfTexture.Width, _signedDistanceFieldDefinitions[i].SdfTexture.Height), Color.White);
            }
            _spriteBatch.End();


            //Atlas
            this.SetVolumeTexParams(_atlasRenderTarget2D, _volumeTexSizeArray, _volumeTexResolutionArray);
            PointLightRenderModule.SetVolumeTexParams(_atlasRenderTarget2D, _volumeTexSizeArray, _volumeTexResolutionArray);
            EnvironmentProbeRenderModule.SetVolumeTexParams(_atlasRenderTarget2D, _volumeTexSizeArray, _volumeTexResolutionArray);
        }


        public RenderTarget2D CreateSDFTexture(GraphicsDevice graphics, Texture2D triangleData, int xsteps, int ysteps, int zsteps, SignedDistanceField sdf, int trianglesLength)
        {
            RenderTarget2D output = new RenderTarget2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(output);

            //Offset isntead of position!
            _volumeTexResolutionArray[0] = new Vector4(xsteps, ysteps, zsteps, 0);
            _volumeTexSizeArray[0] = sdf.VolumeSize;

            this.SetVolumeTexParams(triangleData, _volumeTexSizeArray, _volumeTexResolutionArray);

            MeshOffset = sdf.Offset;

            Param_TriangleTexResolution.SetValue(new Vector2(triangleData.Width, triangleData.Height));
            Param_TriangleAmount.SetValue((float)trianglesLength);

            Pass_GenerateSDF.Apply();
            FullscreenTriangleBuffer.Instance.Draw(graphics);

            _signedDistanceFieldDefinitionsCount = -1;

            return output;
        }

        public Texture2D GetAtlas()
        {
            return _atlasRenderTarget2D;
        }
    }
}
