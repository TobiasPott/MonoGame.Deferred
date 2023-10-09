﻿
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.DeferredLighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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


        public class Generator
        {
            private Task generateTask;

            private List<SignedDistanceField> sdfDefinitions = new List<SignedDistanceField>();

            public void GenerateTriangles(Model model, ref SdfTriangle[] triangles)
            {
                GeometryDataExtractor.GetVerticesAndIndicesFromModel(model, out Vector3[] vertexPositions, out int[] indices);
                if (triangles.Length != indices.Length / 3)
                    Array.Resize<SdfTriangle>(ref triangles, indices.Length / 3);
                int baseIndex = 0;
                for (var i = 0; i < triangles.Length; i++, baseIndex += 3)
                {
                    triangles[i].a = vertexPositions[indices[baseIndex]];
                    triangles[i].b = vertexPositions[indices[baseIndex + 1]];
                    triangles[i].c = vertexPositions[indices[baseIndex + 2]];
                    //normal
                    triangles[i].ba = triangles[i].b - triangles[i].a;
                    triangles[i].cb = triangles[i].c - triangles[i].b;
                    triangles[i].ac = triangles[i].a - triangles[i].c;

                    triangles[i].n = Vector3.Cross(triangles[i].ba, triangles[i].ac);
                    triangles[i].n.Normalize();
                    triangles[i].n *= 0.03f;
                }
            }

            // ToDo: @tpott: temporarily extract to static method to move SDFGenerator to LibDeferred (due to DistanceFieldRenderModule use)
            public void GenerateDistanceFields(ModelDefinition modelDefinition, GraphicsDevice graphics, DistanceFieldRenderModule distanceFieldRenderModule)
            {
                if (modelDefinition is SdfModelDefinition)
                {
                    SdfModelDefinition sdfModelDefinition = (SdfModelDefinition)modelDefinition;
                    SignedDistanceField uncomputedSignedDistanceField = sdfModelDefinition.SDF;
                    Model unprocessedModel = sdfModelDefinition.Model;

                    //Set to false so it won't get covered in future
                    uncomputedSignedDistanceField.NeedsToBeGenerated = false;
                    uncomputedSignedDistanceField.IsLoaded = false;
                    uncomputedSignedDistanceField.SdfTexture?.Dispose();

                    //First generate tris
                    GenerateTriangles(unprocessedModel, ref sdfModelDefinition.SdfTriangles);
                    SdfTriangle[] triangles = sdfModelDefinition.SdfTriangles;

                    int xsteps = (int)uncomputedSignedDistanceField.TextureResolution.X;
                    int ysteps = (int)uncomputedSignedDistanceField.TextureResolution.Y;
                    int zsteps = (int)uncomputedSignedDistanceField.TextureResolution.Z;

                    Texture2D output;

                    if (!RenderingSettings.sdf_cpu)
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();

                        int maxwidth = 4096; //16384
                        int requiredData = triangles.Length * 3;

                        int x = maxwidth;//Math.Min(requiredData, maxwidth);
                        int y = requiredData / x + 1;

                        Vector4[] data = new Vector4[x * y];

                        int index = 0;
                        for (int i = 0; i < triangles.Length; i++, index += 3)
                        {
                            data[index] = new Vector4(triangles[i].a, 0);
                            data[index + 1] = new Vector4(triangles[i].b, 0);
                            data[index + 2] = new Vector4(triangles[i].c, 0);
                        }

                        //16k

                        Texture2D triangleData = new Texture2D(graphics, x, y, false, SurfaceFormat.Vector4);

                        triangleData.SetData(data);

                        output = distanceFieldRenderModule.CreateSDFTexture(graphics, triangleData, xsteps, ysteps,
                            zsteps, uncomputedSignedDistanceField, triangles.Length);

                        stopwatch.Stop();

                        Debug.Write("\nSDF generated in " + stopwatch.ElapsedMilliseconds + "ms on GPU");

                        float[] texData = new float[xsteps * ysteps * zsteps];

                        output.GetData(texData);

                        string path = uncomputedSignedDistanceField.TexturePath;
                        DataStream.SaveImageData(texData, xsteps, ysteps, zsteps, path);
                        uncomputedSignedDistanceField.TextureResolution = new Vector4(xsteps, ysteps, zsteps, 0);
                        uncomputedSignedDistanceField.SdfTexture = output;
                        uncomputedSignedDistanceField.IsLoaded = true;

                    }
                    else
                    {
                        generateTask = Task.Factory.StartNew(() =>
                        {
                            output = new Texture2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single);

                            float[] data = new float[xsteps * ysteps * zsteps];

                            Stopwatch stopwatch = Stopwatch.StartNew();

                            int numberOfThreads = RenderingSettings.sdf_threads;

                            if (numberOfThreads > 1)
                            {
                                Task[] threads = new Task[numberOfThreads - 1];

                                //Make local datas

                                float[][] dataArray = new float[numberOfThreads][];

                                for (int index = 0; index < threads.Length; index++)
                                {
                                    int i = index;
                                    dataArray[index + 1] = new float[xsteps * ysteps * zsteps];
                                    threads[i] = Task.Factory.StartNew(() =>
                                    {
                                        GenerateData(xsteps, ysteps, zsteps, uncomputedSignedDistanceField,
                                            ref dataArray[i + 1], i + 1,
                                            numberOfThreads, triangles);
                                    });
                                }

                                dataArray[0] = data;
                                GenerateData(xsteps, ysteps, zsteps, uncomputedSignedDistanceField, ref dataArray[0], 0,
                                    numberOfThreads, triangles);

                                Task.WaitAll(threads);

                                //Something broke?
                                for (int i = 0; i < data.Length; i++)
                                {
                                    //data[i] = dataArray[i % numberOfThreads][i];
                                    for (int j = 0; j < numberOfThreads; j++)
                                    {
                                        if (dataArray[j][i] != 0)
                                        {
                                            data[i] = dataArray[j][i];
                                            break;
                                        }
                                    }
                                }

                                for (var index2 = 0; index2 < threads.Length; index2++)
                                {
                                    threads[index2].Dispose();
                                }
                            }
                            else
                            {
                                GenerateData(xsteps, ysteps, zsteps, uncomputedSignedDistanceField, ref data, 0,
                                    numberOfThreads, triangles);
                            }

                            stopwatch.Stop();

                            Debug.Write("\nSDF generated in " + stopwatch.ElapsedMilliseconds + "ms with " +
                                        RenderingSettings.sdf_threads + " thread(s)");

                            string path = uncomputedSignedDistanceField.TexturePath;
                            DataStream.SaveImageData(data, xsteps, ysteps, zsteps, path);
                            output.SetData(data);
                            uncomputedSignedDistanceField.TextureResolution = new Vector4(xsteps, ysteps, zsteps, 0);
                            uncomputedSignedDistanceField.SdfTexture = output;
                            uncomputedSignedDistanceField.IsLoaded = true;
                        });
                    }
                }
            }


            // ToDo: @tpott: temporarily extract to static method to move SDFGenerator to LibDeferred (due to DistanceFieldRenderModule use)
            public void Update(List<ModelEntity> entities, GraphicsDevice graphics, DistanceFieldRenderModule distanceFieldRenderModule, ref List<SignedDistanceField> sdfDefinitionsOut)
            {
                //First let's check which entities need building, if at all!
                sdfDefinitions.Clear();

                //This should preferably be a list of meshes that are in the scene, instead of a list of entities
                for (var index0 = 0; index0 < entities.Count; index0++)
                {
                    ModelEntity entity = entities[index0];

                    SdfModelDefinition sdfModelDefinition = entity.ModelDefinition as SdfModelDefinition;
                    if (sdfModelDefinition != null)
                    {
                        if (!sdfModelDefinition.SDF.IsUsed) continue;
                        if (sdfModelDefinition.SDF.NeedsToBeGenerated)
                            GenerateDistanceFields(entity.ModelDefinition, graphics, distanceFieldRenderModule);

                        bool found = false;
                        //Compile a list of all mbbs used right now
                        for (var i = 0; i < sdfDefinitions.Count; i++)
                        {
                            if (sdfModelDefinition.SDF == sdfDefinitions[i])
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            sdfDefinitions.Add(sdfModelDefinition.SDF);
                    }

                }

                //Now for the model definitions
                for (var i = 0; i < sdfDefinitions.Count; i++)
                {
                    if (RenderingSettings.sdf_regenerate)
                    {
                        sdfDefinitions[i].NeedsToBeGenerated = true;
                    }
                }

                RenderingSettings.sdf_regenerate = false;

                sdfDefinitionsOut = sdfDefinitions;
            }

            private void GenerateData(int xsteps, int ysteps, int zsteps, SignedDistanceField volumeTex, ref float[] data, int threadindex, int numberOfThreads, SdfTriangle[] triangles)
            {
                int xi, yi, zi;

                float volumeTexSizeX = volumeTex.VolumeSize.X;
                float volumeTexSizeY = volumeTex.VolumeSize.Y;
                float volumeTexSizeZ = volumeTex.VolumeSize.Z;

                Vector3 offset = new Vector3(volumeTex.Offset.X, volumeTex.Offset.Y, volumeTex.Offset.Z);

                int i = 0;

                for (xi = 0; xi < xsteps; xi++)
                {
                    for (yi = 0; yi < ysteps; yi++)
                    {
                        for (zi = 0; zi < zsteps; zi++)
                        {
                            //Only do it for the current thread!
                            if (i++ % numberOfThreads != threadindex) continue;

                            Vector3 position = new Vector3(xi * volumeTexSizeX * 2.0f / (xsteps - 1) - volumeTexSizeX,
                                yi * volumeTexSizeY * 2.0f / (ysteps - 1) - volumeTexSizeY,
                                zi * volumeTexSizeZ * 2.0f / (zsteps - 1) - volumeTexSizeZ) + offset;

                            float color = ComputeSDF(position, triangles);

                            data[ToTexCoords(xi, yi, zi, xsteps, zsteps)] = color;

                            if (threadindex == 0)
                                RenderingStats.sdf_load = (xi + (yi + zi / (float)zsteps) / (float)ysteps) / (float)xsteps;
                        }
                    }

                }
            }

            private static int ToTexCoords(int x, int y, int z, int xsteps, int zsteps)
            {
                x += z * xsteps;
                return x + y * xsteps * zsteps;
            }
            //private static float Dot2(Vector3 v)
            //{
            //    return Vector3.Dot(v, v);
            //}

            private static float Saturate(float x)
            {
                return x < 0 ? 0 : x > 1 ? 1 : x;
            }

            private static float ComputeSDF(Vector3 p, SdfTriangle[] triangles)
            {
                //Find nearest distance.
                //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

                float min = 100000;

                //Shoot a ray in some direction to check if we are inside the mesh or outside

                Vector3 dir = Vector3.Up;

                int intersections = 0;

                for (var index = 0; index < triangles.Length; index++)
                {
                    var tri = triangles[index];
                    Vector3 a = tri.a;
                    Vector3 b = tri.b;
                    Vector3 c = tri.c;
                    Vector3 ba = tri.ba;
                    Vector3 pa = p - a;
                    Vector3 cb = tri.cb;
                    Vector3 pb = p - b;
                    Vector3 ac = tri.ac;
                    Vector3 pc = p - c;
                    Vector3 nor = tri.n;

                    float value = (Math.Sign(Vector3.Dot(Vector3.Cross(ba, nor), pa)) +
                                    Math.Sign(Vector3.Dot(Vector3.Cross(cb, nor), pb)) +
                                    Math.Sign(Vector3.Dot(Vector3.Cross(ac, nor), pc)) < 2.0f)
                                    ?
                                    Math.Min(Math.Min(
                                    Extensions.Dot2(ba * Saturate(Vector3.Dot(ba, pa) / Extensions.Dot2(ba)) - pa),
                                    Extensions.Dot2(cb * Saturate(Vector3.Dot(cb, pb) / Extensions.Dot2(cb)) - pb)),
                                    Extensions.Dot2(ac * Saturate(Vector3.Dot(ac, pc) / Extensions.Dot2(ac)) - pc))
                                    :
                                    Vector3.Dot(nor, pa) * Vector3.Dot(nor, pa) / Extensions.Dot2(nor);

                    //intersection
                    intersections += RayCast(a, b, c, p, dir);


                    //int sign = Math.Sign(Vector3.Dot(pa, nor));

                    //value = /*Math.Abs(value)*/value * sign;

                    if (Math.Abs(value) < Math.Abs(min))
                    {
                        min = value;
                    }
                }

                int signum = intersections % 2 == 0 ? 1 : -1;

                return (float)Math.Sqrt(Math.Abs(min)) * signum; /** Math.Sign(min)*/;
            }
            private static int RayCast(Vector3 a, Vector3 b, Vector3 c, Vector3 origin, Vector3 dir)
            {
                Vector3 edge1 = b - a;
                Vector3 edge2 = c - a;
                Vector3 pvec = Vector3.Cross(dir, edge2);
                float det = Vector3.Dot(edge1, pvec);

                const float EPSILON = 0.0000001f;

                if (det > -EPSILON && det < EPSILON) return 0;

                float inv_det = 1.0f / det;
                Vector3 tvec = origin - a;
                float u = Vector3.Dot(tvec, pvec) * inv_det;

                if (u < 0 || u > 1) return 0;
                Vector3 qvec = Vector3.Cross(tvec, edge1);
                float v = Vector3.Dot(dir, qvec) * inv_det;
                if (v < 0 || u + v > 1) return 0;

                float t = Vector3.Dot(edge2, qvec) * inv_det;

                if (t > EPSILON) return 1;

                return 0;
            }

        }
    
    }

}
