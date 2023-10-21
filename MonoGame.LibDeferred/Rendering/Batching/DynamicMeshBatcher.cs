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
        public enum RenderType
        {
            Opaque,
            ShadowOmnidirectional,
            ShadowLinear,
            Hologram,
            Forward,
            IdRender,
            IdOutline,
        }

        private const int InitialLibrarySize = 16;

        private static readonly DepthStencilState DepthWriteState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true, DepthBufferFunction = CompareFunction.Always };
        private static readonly RasterizerState ShadowGenerationRasterizerState = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace, ScissorTestEnable = true };


        private readonly FullscreenTriangleBuffer _fullscreenTarget;
        private readonly GraphicsDevice _graphicsDevice;

        private List<MaterialBatch> _batches = new List<MaterialBatch>(InitialLibrarySize);

        private bool _cpuCulling = RenderingSettings.g_CpuCulling;

        private readonly BoundingSphere _defaultBoundingSphere = new BoundingSphere(Vector3.Zero, 0);


        public bool BatchByMaterial { get; set; } = false;
        public bool IsAnyRendered => _batches.Any(x => x.IsAnyRendered);


        public DynamicMeshBatcher(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;

            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }

        public void Register(MaterialEffect mat, Model model, TransformableObject transform)
        {
            if (model == null) return;

            for (int index = 0; index < model.Meshes.Count; index++)
            {
                ModelMesh mesh = model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    Register(mat, mesh.MeshParts[i], transform, mesh.BoundingSphere);
                }
            }
        }

        private void Register(MaterialEffect mat, ModelMeshPart mesh, TransformableObject transform, BoundingSphere boundingSphere) //These should be ordered by likeness, so I don't get opaque -> transparent -> opaque
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
            for (var i = 0; i < _batches.Count; i++)
            {
                MaterialBatch matLib = _batches[i];
                if (this.BatchByMaterial && matLib.HasMaterial(mat))
                {
                    matLib.Register(mesh, transform, boundingSphere);
                    found = true;
                    break;
                }
            }

            //We have no MatLib for that specific Material yet. Make a new one.
            if (!found)
            {
                MaterialBatch batch = new MaterialBatch();
                batch.SetMaterial(mat);
                batch.Register(mesh, transform, boundingSphere);
                _batches.Add(batch);
                //Debug.WriteLine($"Created new material batch array (GC!): {MaterialBatch.Count}");
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
            for (var i = 0; i < _batches.Count; i++)
            {
                MaterialBatch matLib = _batches[i];
                //if (matLib.HasMaterial(mat))
                //{
                if (matLib.DeleteFromRegistry(mesh, transform))
                {
                    _batches.RemoveAt(i);
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
            if (_cpuCulling != RenderingSettings.g_CpuCulling)
            {
                if (_cpuCulling)
                {
                    //If we previously did cull and now don't we need to set all the submeshes to render
                    for (int matBatchIndex = 0; matBatchIndex < _batches.Count; matBatchIndex++)
                    {
                        MaterialBatch matBatch = _batches[matBatchIndex];
                        for (int i = 0; i < matBatch.Count; i++)
                        {
                            matBatch[i].AllRendered = true;
                        }
                    }

                }
                _cpuCulling = RenderingSettings.g_CpuCulling;

            }
            if (!RenderingSettings.g_CpuCulling) return false;


            bool hasAnythingChanged = false;
            //Ok we applied the transformation to all the entities, now update the submesh boundingboxes!
            for (int matBatchIndex = 0; matBatchIndex < _batches.Count; matBatchIndex++)
            {
                MaterialBatch matLib = _batches[matBatchIndex];
                for (int i = 0; i < matLib.Count; i++)
                    matLib[i].UpdatePositionAndCheckRender(hasCameraChanged, boundingFrustrum, _defaultBoundingSphere);
            }

            return hasAnythingChanged;
        }

        /// <summary>
        /// Should be called when the frame is done.
        /// </summary>
        public void FrustumCullingFinalizeFrame()
        {
            for (int i = 0; i < _batches.Count; i++)
            {
                MaterialBatch matLib = _batches[i];
                matLib.HasChangedThisFrame = false;
            }
        }

        [Flags]
        public enum DrawFlags
        {
            LightViewChanged = 1,
            ObjectsChanged = 2,
            Outlined = 4,

        }

        public void Draw(RenderType renderType, PipelineMatrices matrices,
            bool outlined = false, int outlineId = 0, IRenderModule renderModule = null)
            => Draw(renderType, matrices.ViewProjection, matrices.View, outlined, outlineId, renderModule);
        public void Draw(RenderType renderType, Matrix viewProjection, Matrix? view, bool outlined = false, int outlineId = 0, IRenderModule renderModule = null)
        {
            SetBlendAndRasterizerState(renderType);

            for (int matBatchIndex = 0; matBatchIndex < _batches.Count; matBatchIndex++)
            {
                MaterialBatch matLib = _batches[matBatchIndex];

                if (matLib.Count < 1) continue;

                //if none of this materialtype is drawn continue too!
                bool isUsed = false;

                for (int i = 0; i < matLib.Count; i++)
                {
                    if (matLib[i].IsAnyRendered)
                    {
                        isUsed = true;
                        break;
                    }

                }
                if (!isUsed) continue;

                //Count the draws of different materials!

                MaterialEffect material = matLib.Material;
                //Check if alpha or opaque!
                if (renderType == RenderType.Opaque && material.IsTransparent
                    || renderType == RenderType.Opaque && material.Type == MaterialEffect.MaterialTypes.ForwardShaded)
                    continue;
                if (renderType == RenderType.Hologram && material.Type != MaterialEffect.MaterialTypes.Hologram)
                    continue;
                if (renderType != RenderType.Hologram && material.Type == MaterialEffect.MaterialTypes.Hologram)
                    continue;

                if (renderType == RenderType.Forward &&
                    material.Type != MaterialEffect.MaterialTypes.ForwardShaded)
                    continue;

                //Set the appropriate Shader for the material
                if ((renderType == RenderType.ShadowOmnidirectional || renderType == RenderType.ShadowLinear)
                    && !material.HasShadow)
                    continue;

                if (renderType != RenderType.IdRender && renderType != RenderType.IdOutline)
                    RenderingStats.MaterialDraws++;

                PerMaterialSettings(renderType, material, renderModule);

                for (int i = 0; i < matLib.Count; i++)
                {
                    MeshBatch meshLib = matLib[i];

                    //Initialize the mesh VB and IB
                    ModelMeshPart mesh = meshLib.GetMesh();
                    _graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                    _graphicsDevice.Indices = mesh.IndexBuffer;
                    int primitiveCount = mesh.PrimitiveCount;
                    int vertexOffset = mesh.VertexOffset;
                    //int vCount = meshLib.GetMesh().NumVertices;
                    int startIndex = mesh.StartIndex;

                    //Now draw the local meshes!
                    for (int index = 0; index < meshLib.Count; index++)
                    {
                        //If it's set to "not rendered" skip
                        if (!meshLib.Rendered[index])
                            continue;

                        Matrix localWorldMatrix = meshLib.GetTransforms()[index].World;

                        if (!ApplyShaders(renderType, renderModule, localWorldMatrix, view, viewProjection, meshLib, index, outlineId, outlined))
                            continue;
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


        public bool CheckRequiresRedraw(DynamicMeshBatcher.RenderType renderType, bool lightViewPointChanged, bool hasAnyObjectMoved)
        {
            if (renderType == RenderType.ShadowLinear || renderType == RenderType.ShadowOmnidirectional)
                //For shadowmaps we need to find out whether any object has moved and if so if it is rendered. If yes, redraw the whole frame, if no don't do anything
                return CheckShadowMapUpdateNeeds(lightViewPointChanged, hasAnyObjectMoved);

            return true;
        }
        /// <summary>
        /// Checks whether or not anything has moved, if no then we ignore the frame
        /// </summary>
        private bool CheckShadowMapUpdateNeeds(bool lightViewPointChanged, bool hasAnyObjectMoved)
        {
            if (lightViewPointChanged || hasAnyObjectMoved)
            {
                if (!IsAnyRendered)
                    return false;

                _graphicsDevice.DepthStencilState = DepthWriteState;

                DeferredEffectSetup.Instance.Pass_Clear.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
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
                    _graphicsDevice.RasterizerState = ShadowGenerationRasterizerState;
                }
            }
            else //if (renderType == RenderType.alpha)
            {
                _graphicsDevice.BlendState = BlendState.NonPremultiplied;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        private bool ApplyShaders(RenderType renderType, IRenderModule renderModule, Matrix localToWorldMatrix, Matrix? view, Matrix viewProjection, MeshBatch meshBatch,
            int index, int outlineId, bool outlined)
        {
            switch (renderType)
            {
                case RenderType.Opaque:
                case RenderType.ShadowLinear:
                case RenderType.ShadowOmnidirectional:
                case RenderType.Forward:
                    ApplyRenderModuleShaders(renderModule, localToWorldMatrix, view, viewProjection);
                    break;
                case RenderType.Hologram:
                    ApplyHologramShaders(localToWorldMatrix, viewProjection);
                    break;
                case RenderType.IdRender:
                case RenderType.IdOutline:
                    if (!ApplyIdAndOutlineShaders(renderType, localToWorldMatrix, viewProjection, meshBatch, index, outlineId, outlined))
                        return false;
                    break;
            }
            return true;
        }

        private void ApplyRenderModuleShaders(IRenderModule renderModule, Matrix localToWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            renderModule.Apply(localToWorldMatrix, view, viewProjection);
        }
        private void ApplyHologramShaders(Matrix localToWorldMatrix, Matrix viewProjection)
        {
            HologramEffectSetup.Instance.Param_World.SetValue(localToWorldMatrix);
            HologramEffectSetup.Instance.Param_WorldViewProj.SetValue(localToWorldMatrix * viewProjection);
            HologramEffectSetup.Instance.Effect.CurrentTechnique.Passes[0].Apply();
        }

        private bool ApplyIdAndOutlineShaders(RenderType renderType, Matrix localToWorldMatrix, Matrix viewProjection, MeshBatch meshBatch, int index, int outlineId, bool outlined)
        {
            // ToDo: @tpott: Extract IdRender and Bilboard Shaders members
            IdAndOutlineEffectSetup.Instance.Param_WorldViewProj.SetValue(localToWorldMatrix * viewProjection);

            int id = meshBatch.GetTransforms()[index].Id;

            if (renderType == RenderType.IdRender)
            {
                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(IdGenerator.GetColorFromId(id).ToVector4());
                IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
            }
            if (renderType == RenderType.IdOutline)
            {

                //Is this the Id we want to outline?
                if (id == outlineId)
                {
                    _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                    IdAndOutlineEffectSetup.Instance.Param_World.SetValue(localToWorldMatrix);

                    if (outlined)
                        IdAndOutlineEffectSetup.Instance.Pass_Outline.Apply();
                    else
                        IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
                }
                else
                    return false;
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

    }
}
