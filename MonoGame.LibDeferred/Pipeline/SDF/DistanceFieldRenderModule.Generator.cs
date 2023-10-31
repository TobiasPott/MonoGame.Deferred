
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System.Diagnostics;

namespace DeferredEngine.Rendering.SDF
{
    public partial class DistanceFieldRenderModule
    {
        public class Generator
        {
            private Task generateTask;

            private readonly List<SignedDistanceField> sdfDefinitions = new List<SignedDistanceField>();

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
                    SignedDistanceField uncomputedSdf = sdfModelDefinition.SDF;
                    Model unprocessedModel = sdfModelDefinition.Model;

                    //Set to false so it won't get covered in future
                    uncomputedSdf.NeedsToBeGenerated = false;
                    uncomputedSdf.IsLoaded = false;
                    uncomputedSdf.SdfTexture?.Dispose();

                    //First generate tris
                    GenerateTriangles(unprocessedModel, ref sdfModelDefinition.SdfTriangles);
                    SdfTriangle[] triangles = sdfModelDefinition.SdfTriangles;


                    Vector3 steps = uncomputedSdf.TextureResolution.Xyz();
                    int xsteps = (int)uncomputedSdf.TextureResolution.X;
                    int ysteps = (int)uncomputedSdf.TextureResolution.Y;
                    int zsteps = (int)uncomputedSdf.TextureResolution.Z;

                    Texture2D output;

                    if (!RenderingSettings.SDF.UseCpu)
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

                        output = distanceFieldRenderModule.CreateSDFTexture(graphics, triangleData, steps, uncomputedSdf, triangles.Length);

                        stopwatch.Stop();

                        Debug.Write("\nSDF generated in " + stopwatch.ElapsedMilliseconds + "ms on GPU");

                        float[] texData = new float[xsteps * ysteps * zsteps];

                        output.GetData(texData);

                        string path = uncomputedSdf.TexturePath;
                        DataStream.SaveImageData(texData, xsteps, ysteps, zsteps, path);
                        uncomputedSdf.TextureResolution = new Vector4(xsteps, ysteps, zsteps, 0);
                        uncomputedSdf.SdfTexture = output;
                        uncomputedSdf.IsLoaded = true;

                    }
                    else
                    {
                        generateTask = Task.Factory.StartNew(() =>
                        {
                            output = new Texture2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single);

                            float[] data = new float[xsteps * ysteps * zsteps];

                            Stopwatch stopwatch = Stopwatch.StartNew();

                            int numberOfThreads = RenderingSettings.SDF.NumOfCpuThreads;
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
                                        GenerateData(xsteps, ysteps, zsteps, uncomputedSdf,
                                            ref dataArray[i + 1], i + 1,
                                            numberOfThreads, triangles);
                                    });
                                }

                                dataArray[0] = data;
                                GenerateData(xsteps, ysteps, zsteps, uncomputedSdf, ref dataArray[0], 0,
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
                                GenerateData(xsteps, ysteps, zsteps, uncomputedSdf, ref data, 0,
                                    numberOfThreads, triangles);
                            }

                            stopwatch.Stop();

                            Debug.Write("\nSDF generated in " + stopwatch.ElapsedMilliseconds + "ms with " + numberOfThreads + " thread(s)");

                            string path = uncomputedSdf.TexturePath;
                            DataStream.SaveImageData(data, xsteps, ysteps, zsteps, path);
                            output.SetData(data);
                            uncomputedSdf.TextureResolution = new Vector4(xsteps, ysteps, zsteps, 0);
                            uncomputedSdf.SdfTexture = output;
                            uncomputedSdf.IsLoaded = true;

                        });
                    }
                }
            }


            internal void Update(GraphicsDevice graphics, List<ModelEntity> entities,
                DistanceFieldRenderModule distanceFieldRenderModule, ref List<SignedDistanceField> sdfDefinitionsOut)
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
                    if (RenderingSettings.SDF.Regenerate)
                        sdfDefinitions[i].NeedsToBeGenerated = true;
                }
                RenderingSettings.SDF.Regenerate = false;
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
