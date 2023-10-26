using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using DeferredEngine.Rendering.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System.Diagnostics;

namespace DeferredEngine.Rendering
{

    [Flags]
    public enum RenderFlags
    {
        None = 0,
        Outlined = 1,
    }
    public class RenderContext
    {
        public static readonly RenderContext Default = new RenderContext();


        public RenderFlags Flags = RenderFlags.None;
        public int OutlineId = 0;
    }

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


    // Controls all Materials and Meshes, so they are ordered at render time.
    public class DynamicMeshBatcher
    {

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

        public void Draw(RenderType renderType, PipelineMatrices matrices, RenderContext context, IRenderModule renderModule = null)
            => Draw(renderType, matrices.ViewProjection, matrices.View, context, renderModule);
        public void Draw(RenderType renderType, Matrix viewProjection, Matrix? view, RenderContext context, IRenderModule renderModule = null)
        {
            SetGraphicsDeviceStates(renderType);

            for (int matBatchIndex = 0; matBatchIndex < _batches.Count; matBatchIndex++)
            {
                MaterialBatch materialBatch = _batches[matBatchIndex];

                if (materialBatch.Count <= 0)
                    continue;

                //Count the draws of different materials!
                MaterialEffect material = materialBatch.Material;
                //Check if alpha or opaque!
                if (renderType == RenderType.Opaque && material.IsTransparent
                    || renderType == RenderType.Opaque && material.Type == MaterialEffect.MaterialTypes.ForwardShaded)
                    continue;
                if (renderType == RenderType.Hologram && material.Type != MaterialEffect.MaterialTypes.Hologram)
                    continue;
                if (renderType != RenderType.Hologram && material.Type == MaterialEffect.MaterialTypes.Hologram)
                    continue;
                if (renderType == RenderType.Forward && material.Type != MaterialEffect.MaterialTypes.ForwardShaded)
                    continue;
                if ((renderType == RenderType.ShadowOmnidirectional || renderType == RenderType.ShadowLinear) && !material.HasShadow)
                    continue;

                //if none of this materialtype is drawn continue too!
                bool isUsed = false;
                for (int i = 0; i < materialBatch.Count; i++)
                {
                    if (materialBatch[i].IsAnyRendered)
                    {
                        isUsed = true;
                        break;
                    }
                }
                if (!isUsed) continue;

                // Statistics counter
                if (renderType != RenderType.IdRender && renderType != RenderType.IdOutline)
                    RenderingStats.MaterialDraws++;

                //Set the appropriate Shader for the material
                PerMaterialSettings(renderType, material, renderModule);

                for (int i = 0; i < materialBatch.Count; i++)
                {
                    MeshBatch meshBatch = materialBatch[i];
                    ModelMeshPart mesh = meshBatch.GetMesh();

                    //Initialize the mesh VB and IB
                    _graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                    _graphicsDevice.Indices = mesh.IndexBuffer;


                    //Now draw the local meshes!
                    for (int index = 0; index < meshBatch.Count; index++)
                    {
                        //If it's set to "not rendered" skip
                        if (!meshBatch.Rendered[index])
                            continue;

                        if (!RenderModule.ApplyShaders(_graphicsDevice, renderType, renderModule, meshBatch[index].World, view, viewProjection, meshBatch[index].Id, context.OutlineId, context.Flags.HasFlag(RenderFlags.Outlined)))
                            continue;
                        RenderingStats.MeshDraws++;

                        // ToDo: Research Instanced Drawind https://www.braynzarsoft.net/viewtutorial/q16390-33-instancing-with-indexed-primitives
                        //      to improve rendering by providing instance matrices
                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount);
                    }
                }

                //Reset to 
                if (material.RenderCClockwise)
                    _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);
            }
        }


        public bool CheckRequiresRedraw(RenderType renderType, bool lightViewPointChanged, bool hasAnyObjectMoved)
        {
            if (!this.IsAnyRendered)
                return false;

            if (renderType == RenderType.ShadowLinear || renderType == RenderType.ShadowOmnidirectional)
                //For shadowmaps we need to find out whether any object has moved and if so if it is rendered. If yes, redraw the whole frame, if no don't do anything
                return CheckShadowMapRequiresUpdate(lightViewPointChanged, hasAnyObjectMoved);

            return true;
        }
        /// <summary>
        /// Checks whether or not anything has moved, if no then we ignore the frame
        /// </summary>
        private bool CheckShadowMapRequiresUpdate(bool lightViewPointChanged, bool hasAnyObjectMoved)
        {
            if (lightViewPointChanged || hasAnyObjectMoved)
            {
                _graphicsDevice.DepthStencilState = DepthWriteState;

                DeferredFxSetup.Instance.Pass_Clear.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
                _graphicsDevice.SetState(DepthStencilStateOption.Default);
            }

            RenderingStats.activeShadowMaps++;
            return true;
        }

        private void SetGraphicsDeviceStates(RenderType renderType)
        {
            //Default, Opaque!
            if (renderType != RenderType.Forward)
            {
                if (renderType != RenderType.ShadowOmnidirectional)
                    _graphicsDevice.SetStates(DepthStencilStateOption.KeepState, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);
                else //Need special rasterization
                {
                    _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.KeepState, BlendStateOption.Opaque);
                    _graphicsDevice.RasterizerState = ShadowGenerationRasterizerState;
                }
            }
            else //if (renderType == RenderType.alpha)
                _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.NonPremultiplied);
        }


        private void PerMaterialSettings(RenderType renderType, MaterialEffect material, IRenderModule renderModule)
        {
            if (material.RenderCClockwise)
            {
                _graphicsDevice.SetState(RasterizerStateOption.CullClockwise);
            }
            else if (renderType == RenderType.ShadowOmnidirectional)
            {
                ((ShadowMapPipelineModule)renderModule).SetMaterialSettings(material);
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
