
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

        private EffectParameter _frustumCornersParam;
        private EffectParameter _cameraPositonParam;
        private EffectParameter _depthMapParam;
        private EffectParameter _volumeTexParam;
        private EffectParameter _meshOffset;
        private EffectParameter _volumeTexSizeParam;
        private EffectParameter _volumeTexResolutionParam;

        private EffectParameter _instanceInverseMatrixArrayParam;
        private EffectParameter _instanceScaleArrayParam;
        private EffectParameter _instanceSDFIndexArrayParam;
        private EffectParameter _instancesCountParam;


        public PointLightRenderModule PointLightRenderModule;
        public EnvironmentProbeRenderModule EnvironmentProbeRenderModule;

        private const int InstanceMaxCount = 40;

        private Matrix[] _instanceInverseMatrixArray = new Matrix[InstanceMaxCount];
        private Vector3[] _instanceScaleArray = new Vector3[InstanceMaxCount];
        private float[] _instanceSDFIndexArray = new float[InstanceMaxCount];
        private int _instancesCount = 0;

        private Vector3[] _volumeTexSizeArray = new Vector3[40];
        private Vector4[] _volumeTexResolutionArray = new Vector4[40];

        private SignedDistanceField[] _signedDistanceFieldDefinitions = new SignedDistanceField[40];
        private int _signedDistanceFieldDefinitionsCount = 0;

        private EffectParameter _triangleTexResolution;
        private EffectParameter _triangleAmount;

        public Vector3[] FrustumCornersWorldSpace
        {
            set { _frustumCornersParam.SetValue(value); }
        }
        public Vector3 CameraPosition { set { _cameraPositonParam.SetValue(value); } }

        public Texture2D DepthMap { set { _depthMapParam.SetValue(value); } }
        public Texture2D VolumeTex { set { _volumeTexParam.SetValue(value); } }


        public Vector3 MeshOffset
        {
            set { _meshOffset.SetValue(value); }
        }


        public DistanceFieldRenderModule(ContentManager content, string shaderPath = "Shaders/SignedDistanceFields/volumeProjection")
        {
            Load(content, shaderPath);
        }

        public void Load(ContentManager content, string shaderPath = "Shaders/SignedDistanceFields/volumeProjection")
        {
            Effect = content.Load<Effect>(shaderPath);

            _frustumCornersParam = Effect.Parameters["FrustumCorners"];
            _cameraPositonParam = Effect.Parameters["CameraPosition"];
            _depthMapParam = Effect.Parameters["DepthMap"];

            _volumeTexParam = Effect.Parameters["VolumeTex"];
            _volumeTexSizeParam = Effect.Parameters["VolumeTexSize"];
            _volumeTexResolutionParam = Effect.Parameters["VolumeTexResolution"];

            _instanceInverseMatrixArrayParam = Effect.Parameters["InstanceInverseMatrix"];
            _instanceScaleArrayParam = Effect.Parameters["InstanceScale"];
            _instanceSDFIndexArrayParam = Effect.Parameters["InstanceSDFIndex"];
            _instancesCountParam = Effect.Parameters["InstancesCount"];

            _meshOffset = Effect.Parameters["MeshOffset"];
            _triangleTexResolution = Effect.Parameters["TriangleTexResolution"];
            _triangleAmount = Effect.Parameters["TriangleAmount"];

            Pass_Distance = Effect.Techniques["Distance"].Passes[0];
            Pass_Volume = Effect.Techniques["Volume"].Passes[0];
            Pass_GenerateSDF = Effect.Techniques["GenerateSDF"].Passes[0];
        }

        public void Dispose()
        {
            Effect?.Dispose();
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            CameraPosition = camera.Position;

            if (RenderingSettings.sdf_drawvolume)
                Pass_Volume.Apply();
            else
                Pass_Distance.Apply();
            FullscreenTriangleBuffer.Instance.Draw(graphicsDevice);
        }

        public void UpdateDistanceFieldTransformations(List<ModelEntity> entities, List<SignedDistanceField> sdfDefinitions, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            if (!RenderingSettings.sdf_draw) return;

            //First of all let's build the atlas
            UpdateAtlas(sdfDefinitions, graphics, spriteBatch);

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

            //TODO: Check for change

            //Submit
            //Instances

            _instanceInverseMatrixArrayParam.SetValue(_instanceInverseMatrixArray);
            _instanceScaleArrayParam.SetValue(_instanceScaleArray);
            _instanceSDFIndexArrayParam.SetValue(_instanceSDFIndexArray);
            _instancesCountParam.SetValue((float)_instancesCount);

            PointLightRenderModule.SetInstanceData(_instanceInverseMatrixArray, _instanceScaleArray, _instanceSDFIndexArray, _instancesCount);
            EnvironmentProbeRenderModule.SetInstanceData(_instanceInverseMatrixArray, _instanceScaleArray, _instanceSDFIndexArray, _instancesCount);
        }

        private void UpdateAtlas(List<SignedDistanceField> sdfDefinitionsPassed, GraphicsDevice graphics,
            SpriteBatch spriteBatch)
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
            _atlasRenderTarget2D = new RenderTarget2D(graphics, x, y, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(_atlasRenderTarget2D);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            for (int i = 0; i < _signedDistanceFieldDefinitionsCount; i++)
            {
                spriteBatch.Draw(_signedDistanceFieldDefinitions[i].SdfTexture,
                    new Rectangle(0, (int)_signedDistanceFieldDefinitions[i].TextureResolution.W, _signedDistanceFieldDefinitions[i].SdfTexture.Width, _signedDistanceFieldDefinitions[i].SdfTexture.Height), Color.White);
            }
            spriteBatch.End();


            //Atlas
            VolumeTex = _atlasRenderTarget2D;
            _volumeTexSizeParam.SetValue(_volumeTexSizeArray);
            _volumeTexResolutionParam.SetValue(_volumeTexResolutionArray);

            PointLightRenderModule.Param_VolumeTexParam.SetValue(_atlasRenderTarget2D);
            PointLightRenderModule.Param_VolumeTexSizeParam.SetValue(_volumeTexSizeArray);
            PointLightRenderModule.Param_VolumeTexResolution.SetValue(_volumeTexResolutionArray);

            EnvironmentProbeRenderModule.Param_VolumeTex.SetValue(_atlasRenderTarget2D);
            EnvironmentProbeRenderModule.Param_VolumeTexSize.SetValue(_volumeTexSizeArray);
            EnvironmentProbeRenderModule.Param_VolumeTexResolution.SetValue(_volumeTexResolutionArray);
        }


        public RenderTarget2D CreateSDFTexture(GraphicsDevice graphics, Texture2D triangleData, int xsteps, int ysteps, int zsteps, SignedDistanceField sdf, int trianglesLength)
        {
            RenderTarget2D output = new RenderTarget2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(output);

            //Offset isntead of position!
            _volumeTexResolutionArray[0] = new Vector4(xsteps, ysteps, zsteps, 0);
            _volumeTexSizeArray[0] = sdf.VolumeSize;

            _volumeTexSizeParam.SetValue(_volumeTexSizeArray);
            _volumeTexResolutionParam.SetValue(_volumeTexResolutionArray);

            MeshOffset = sdf.Offset;
            VolumeTex = triangleData;

            _triangleTexResolution.SetValue(new Vector2(triangleData.Width, triangleData.Height));
            _triangleAmount.SetValue((float)trianglesLength);

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
