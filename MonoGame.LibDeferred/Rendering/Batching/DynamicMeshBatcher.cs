using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.RenderModules;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace DeferredEngine.Renderer.Helper
{
    // Controls all Materials and Meshes, so they are ordered at render time.
    public class DynamicMeshBatcher
    {
        private const int InitialLibrarySize = 10;
        public MaterialBatch[] MaterialBatch = new MaterialBatch[InitialLibrarySize];

        public int[] MaterialLibPointer = new int[InitialLibrarySize];

        public int Index;

        private bool _previousMode = RenderingSettings.g_cpuculling;
        private readonly BoundingSphere _defaultBoundingSphere;
        private readonly RasterizerState _shadowGenerationRasterizerState = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace, ScissorTestEnable = true };
        private readonly DepthStencilState _depthWrite = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true, DepthBufferFunction = CompareFunction.Always };

        private readonly FullscreenTriangleBuffer _fullscreenTarget;
        private readonly GraphicsDevice _graphicsDevice;

        public DynamicMeshBatcher(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;

            _defaultBoundingSphere = new BoundingSphere(Vector3.Zero, 0);

            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mat">if "null" it will be taken from the model!</param>
        /// <param name="model"></param>
        /// <param name="transform"></param>
        public void Register(MaterialEffect mat, Model model, TransformableObject transform)
        {
            if (model == null) return;

            for (int index = 0; index < model.Meshes.Count; index++)
            {
                var mesh = model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];
                    Register(mat, meshPart, transform, mesh.BoundingSphere);
                }
            }
        }

        public void Register(MaterialEffect mat, ModelMeshPart mesh, TransformableObject transform, BoundingSphere boundingSphere) //These should be ordered by likeness, so I don't get opaque -> transparent -> opaque
        {
            bool found = false;

            if (mat == null)
            {
                if (mesh.Effect is MaterialEffect effect)
                {
                    mat = effect;
                }
                else
                {
                    mat = new MaterialEffect(mesh.Effect);
                }
            }

            //Check if we already have a material like that, if yes put it in there!
            for (var i = 0; i < Index; i++)
            {
                MaterialBatch matLib = MaterialBatch[i];
                if (matLib.HasMaterial(mat))
                {
                    matLib.Register(mesh, transform, boundingSphere);
                    found = true;
                    break;
                }
            }

            //We have no MatLib for that specific Material yet. Make a new one.
            if (!found)
            {
                MaterialBatch[Index] = new MaterialBatch();
                MaterialBatch[Index].SetMaterial(mat);
                MaterialBatch[Index].Register(mesh, transform, boundingSphere);
                Index++;
            }

            //If we exceeded our array length, make the array bigger.
            if (Index >= MaterialBatch.Length)
            {
                MaterialBatch[] tempLib = new MaterialBatch[Index + 1];
                MaterialBatch.CopyTo(tempLib, 0);
                MaterialBatch = tempLib;

                MaterialLibPointer = new int[Index + 1];
                //sort from 0 to Index
                for (int j = 0; j < MaterialLibPointer.Length; j++)
                {
                    MaterialLibPointer[j] = j;
                }
                SortByDistance();
            }
        }

        //Not a real sort, but it does it's job over time
        private void SortByDistance()
        {
            if (!RenderingSettings.g_cpusort) return;

            for (int i = 1; i < Index; i++)
            {
                float distanceI = MaterialBatch[MaterialLibPointer[i]].DistanceSquared;
                float distanceJ = MaterialBatch[MaterialLibPointer[i - 1]].DistanceSquared;

                if (distanceJ < distanceI)
                {
                    //swap
                    int temp = MaterialLibPointer[i];
                    MaterialLibPointer[i] = MaterialLibPointer[i - 1];
                    MaterialLibPointer[i - 1] = temp;
                }
            }
        }

        public void DeleteFromRegistry(ModelEntity entity)
        {
            if (entity.ModelDefinition.Model == null) return; //nothing to delete

            //delete the individual meshes!
            for (int index = 0; index < entity.ModelDefinition.Model.Meshes.Count; index++)
            {
                var mesh = entity.ModelDefinition.Model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];
                    DeleteFromRegistry(entity.Material, meshPart, entity);
                }
            }
        }

        private void DeleteFromRegistry(MaterialEffect mat, ModelMeshPart mesh, TransformableObject transform)
        {
            // ToDo: @tpott: add index lookup of the mat argument to only delete from library of the correct material
            Debug.WriteLine($"DeleteFromRegistry: Unused {mat} argument.");
            for (var i = 0; i < Index; i++)
            {
                MaterialBatch matLib = MaterialBatch[i];
                //if (matLib.HasMaterial(mat))
                //{
                if (matLib.DeleteFromRegistry(mesh, transform))
                {
                    for (var j = i; j < Index - 1; j++)
                    {
                        //slide down one
                        MaterialBatch[j] = MaterialBatch[j + 1];

                    }
                    Index--;

                    break;
                }
                //}
            }
        }

        /// <summary>
        /// Update whether or not Objects are in the viewFrustumEx and need to be rendered or not.
        /// </summary>
        public bool FrustumCulling(BoundingFrustum boundingFrustrum, bool hasCameraChanged, Vector3 cameraPosition)
        {
            //Check if the culling mode has changed
            if (_previousMode != RenderingSettings.g_cpuculling)
            {
                if (_previousMode)
                {
                    //If we previously did cull and now don't we need to set all the submeshes to render
                    for (int index1 = 0; index1 < Index; index1++)
                    {
                        MaterialBatch matLib = MaterialBatch[index1];
                        for (int i = 0; i < matLib.Count; i++)
                        {
                            MeshBatch meshLib = matLib.GetMeshLibrary()[i];
                            for (int j = 0; j < meshLib.Rendered.Count; j++)
                            {
                                meshLib.Rendered[j] = _previousMode;
                            }
                        }
                    }

                }
                _previousMode = RenderingSettings.g_cpuculling;

            }

            if (!RenderingSettings.g_cpuculling) return false;

            bool hasAnythingChanged = false;
            //Ok we applied the transformation to all the entities, now update the submesh boundingboxes!
            // Parallel.For(0, Index, index1 =>
            for (int index1 = 0; index1 < Index; index1++)
            {
                float distance = 0;
                int counter = 0;


                MaterialBatch matLib = RenderingSettings.g_cpusort
                    ? MaterialBatch[MaterialLibPointer[index1]]
                    : MaterialBatch[index1];
                for (int i = 0; i < matLib.Count; i++)
                {
                    MeshBatch meshLib = matLib.GetMeshLibrary()[i];
                    float? distanceSq = meshLib.UpdatePositionAndCheckRender(hasCameraChanged, boundingFrustrum,
                        cameraPosition, _defaultBoundingSphere);

                    //If we get a new distance, apply it to the material
                    if (distanceSq != null)
                    {
                        distance += (float)distanceSq;
                        counter++;
                        hasAnythingChanged = true;
                    }
                }

                if (Math.Abs(distance) > 0.00001f)
                {
                    distance /= counter;
                    matLib.DistanceSquared = distance;
                    matLib.HasChangedThisFrame = true;
                }
            }//);

            //finally sort the materials by distance. Bubble sort should in theory be fast here since little changes.
            if (hasAnythingChanged)
                SortByDistance();

            return hasAnythingChanged;
        }

        /// <summary>
        /// Should be called when the frame is done.
        /// </summary>
        public void FrustumCullingFinalizeFrame()
        {

            for (int index1 = 0; index1 < Index; index1++)
            {
                MaterialBatch matLib = RenderingSettings.g_cpusort ? MaterialBatch[MaterialLibPointer[index1]] : MaterialBatch[index1];

                matLib.HasChangedThisFrame = false;
            }

        }

        public enum RenderType
        {
            Opaque,
            ShadowOmnidirectional,
            ShadowLinear,
            Hologram,
            IdRender,
            IdOutline,
            Forward,
        }

        public void Draw(RenderType renderType, PipelineMatrices matrices, bool lightViewPointChanged = false, bool hasAnyObjectMoved = false, bool outlined = false, int outlineId = 0, IRenderModule renderModule = null)
            => Draw(renderType, matrices.ViewProjection, matrices.View, lightViewPointChanged, hasAnyObjectMoved, outlined, outlineId, renderModule);
        public void Draw(RenderType renderType, Matrix viewProjection, Matrix? view, bool lightViewPointChanged = false, bool hasAnyObjectMoved = false, bool outlined = false, int outlineId = 0, IRenderModule renderModule = null)
        {
            SetBlendAndRasterizerState(renderType);

            if (renderType == RenderType.ShadowLinear || renderType == RenderType.ShadowOmnidirectional)
            {
                //For shadowmaps we need to find out whether any object has moved and if so if it is rendered. If yes, redraw the whole frame, if no don't do anything
                if (!CheckShadowMapUpdateNeeds(lightViewPointChanged, hasAnyObjectMoved))
                    return;
            }

            for (int index1 = 0; index1 < Index; index1++)
            {
                MaterialBatch matLib = MaterialBatch[index1];

                if (matLib.Count < 1) continue;

                //if none of this materialtype is drawn continue too!
                bool isUsed = false;

                for (int i = 0; i < matLib.Count; i++)
                {
                    MeshBatch meshLib = matLib.GetMeshLibrary()[i];

                    //If it's set to "not rendered" skip
                    for (int j = 0; j < meshLib.Rendered.Count; j++)
                    {
                        if (meshLib.Rendered[j])
                        {
                            isUsed = true;
                            //if (meshLib.GetWorldMatrices()[j].HasChanged)
                            //    hasAnyObjectMoved = true;
                        }

                        if (isUsed)// && hasAnyObjectMoved)
                            break;

                    }
                }

                if (!isUsed) continue;

                //Count the draws of different materials!

                MaterialEffect material = matLib.GetMaterial();

                //Check if alpha or opaque!
                if (renderType == RenderType.Opaque && material.IsTransparent || renderType == RenderType.Opaque && material.Type == MaterialEffect.MaterialTypes.ForwardShaded) continue;
                if (renderType == RenderType.Hologram && material.Type != MaterialEffect.MaterialTypes.Hologram)
                    continue;
                if (renderType != RenderType.Hologram && material.Type == MaterialEffect.MaterialTypes.Hologram)
                    continue;

                if (renderType == RenderType.Forward &&
                    material.Type != MaterialEffect.MaterialTypes.ForwardShaded) continue;

                //Set the appropriate Shader for the material
                if (renderType == RenderType.ShadowOmnidirectional || renderType == RenderType.ShadowLinear)
                {
                    if (!material.HasShadow)
                        continue;
                }

                if (renderType != RenderType.IdRender && renderType != RenderType.IdOutline)
                    RenderingStats.MaterialDraws++;

                PerMaterialSettings(renderType, material, renderModule);

                for (int i = 0; i < matLib.Count; i++)
                {
                    MeshBatch meshLib = matLib.GetMeshLibrary()[i];

                    //Initialize the mesh VB and IB
                    _graphicsDevice.SetVertexBuffer(meshLib.GetMesh().VertexBuffer);
                    _graphicsDevice.Indices = (meshLib.GetMesh().IndexBuffer);
                    int primitiveCount = meshLib.GetMesh().PrimitiveCount;
                    int vertexOffset = meshLib.GetMesh().VertexOffset;
                    //int vCount = meshLib.GetMesh().NumVertices;
                    int startIndex = meshLib.GetMesh().StartIndex;

                    //Now draw the local meshes!
                    for (int index = 0; index < meshLib.Count; index++)
                    {

                        //If it's set to "not rendered" skip
                        //if (!meshLib.GetWorldMatrices()[index].Rendered) continue;
                        if (!meshLib.Rendered[index]) continue;

                        Matrix localWorldMatrix = meshLib.GetTransforms()[index].World;

                        if (!ApplyShaders(renderType, renderModule, localWorldMatrix, view, viewProjection, meshLib, index,
                                outlineId, outlined)) continue;
                        RenderingStats.MeshDraws++;

                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex,
                                primitiveCount);
                    }
                }

                //Reset to 
                if (material.RenderCClockwise)
                    _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        /// <summary>
        /// Checks whether or not anything has moved, if no then we ignore the frame
        /// </summary>
        /// <param name="lightViewPointChanged"></param>
        /// <param name="hasAnyObjectMoved"></param>
        /// <returns></returns>
        private bool CheckShadowMapUpdateNeeds(bool lightViewPointChanged, bool hasAnyObjectMoved)
        {
            if (lightViewPointChanged || hasAnyObjectMoved)
            {
                bool discardFrame = true;

                for (int index1 = 0; index1 < Index; index1++)
                {
                    MaterialBatch matLib = MaterialBatch[index1];

                    //We determined beforehand whether something changed this frame
                    if (matLib.HasChangedThisFrame)
                    {
                        for (int i = 0; i < matLib.Count; i++)
                        {
                            //Now we have to check whether we have a rendered thing in here
                            MeshBatch meshLib = matLib.GetMeshLibrary()[i];
                            for (int index = 0; index < meshLib.Count; index++)
                            {
                                //If it's set to "not rendered" skip
                                for (int j = 0; j < meshLib.Rendered.Count; j++)
                                {
                                    if (meshLib.Rendered[j])
                                    {
                                        discardFrame = false;
                                        break;
                                    }
                                }

                                if (!discardFrame) break;

                            }
                        }
                        if (!discardFrame) break;
                    }
                }

                if (discardFrame) return false;

                _graphicsDevice.DepthStencilState = _depthWrite;
                ClearFrame(_graphicsDevice);
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            }

            RenderingStats.activeShadowMaps++;

            return true;
        }

        private void SetBlendAndRasterizerState(RenderType renderType)
        {
            //Default, Opaque!
            if (renderType != RenderType.Forward)
            {
                if (renderType != RenderType.ShadowOmnidirectional)
                {
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                }
                else //Need special rasterization
                {
                    _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    _graphicsDevice.RasterizerState = _shadowGenerationRasterizerState;
                }
            }
            else //if (renderType == RenderType.alpha)
            {
                _graphicsDevice.BlendState = BlendState.NonPremultiplied;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        private bool ApplyShaders(RenderType renderType, IRenderModule renderModule, Matrix localWorldMatrix, Matrix? view, Matrix viewProjection, MeshBatch meshLib, int index, int outlineId, bool outlined)
        {
            if (renderType == RenderType.Opaque
                || renderType == RenderType.ShadowLinear
                || renderType == RenderType.ShadowOmnidirectional
                || renderType == RenderType.Forward)
            {
                renderModule.Apply(localWorldMatrix, view, viewProjection);
            }
            else if (renderType == RenderType.Hologram)
            {
                Shaders.Hologram.Param_World.SetValue(localWorldMatrix);
                Shaders.Hologram.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
                Shaders.Hologram.Effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (renderType == RenderType.IdRender || renderType == RenderType.IdOutline)
            {
                Shaders.IdRender.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

                int id = meshLib.GetTransforms()[index].Id;

                if (renderType == RenderType.IdRender)
                {
                    Shaders.IdRender.Param_ColorId.SetValue(IdGenerator.GetColorFromId(id).ToVector4());

                    Shaders.IdRender.Technique_Id.Apply();
                }
                if (renderType == RenderType.IdOutline)
                {

                    //Is this the Id we want to outline?
                    if (id == outlineId)
                    {
                        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                        Shaders.IdRender.Param_World.SetValue(localWorldMatrix);

                        if (outlined)
                            Shaders.IdRender.Technique_Outline.Apply();
                        else
                        {
                            Shaders.IdRender.Technique_Id.Apply();
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        private void PerMaterialSettings(RenderType renderType, MaterialEffect material, IRenderModule renderModule)
        {
            if (material.RenderCClockwise)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            }
            else if (renderType == RenderType.ShadowOmnidirectional)
            {
                ((ShadowMapPipelineModule)renderModule).SetMaterialSettings(material, renderType);
            }
            //if (renderType == RenderType.IdRender || renderType == RenderType.IdOutline)
            //{
            //    Shaders.IdRenderEffectParameterColorId.SetValue(Color.Transparent.ToVector4());
            //}
            //todo: We only need textures for non shadow mapping, right? Not quite actually, for alpha textures we need materials
            else if (renderType == RenderType.Opaque)
            {
                ((GBufferPipelineModule)renderModule).SetMaterialSettings(material);
            }
        }

        private void ClearFrame(GraphicsDevice graphicsDevice)
        {
            Shaders.DeferredClear.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(graphicsDevice);
        }

        //I don't want to fill up the main Draw as much! Not used right  now
        public void DrawEmissive(GraphicsDevice graphicsDevice, Camera camera, Matrix viewProjection, Matrix transformedViewProjection, Matrix inverseViewProjection, RenderTarget2D renderTargetEmissive, RenderTarget2D renderTargetDiffuse, RenderTarget2D renderTargetSpecular, BlendState lightBlendState, IEnumerable<ModelMesh> sphereModel, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

    }
}
