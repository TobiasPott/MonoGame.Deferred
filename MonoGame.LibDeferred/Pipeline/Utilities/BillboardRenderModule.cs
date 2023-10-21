using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.Helper.Editor;
using DeferredEngine.Rendering.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System.Diagnostics;

namespace DeferredEngine.Pipeline.Utilities
{
    public partial class BillboardRenderModule
    {

        private GraphicsDevice _graphicsDevice;

        public IdAndOutlineRenderModule IdAndOutlineRenderer;
        private BillboardEffectSetup _effectSetup = new BillboardEffectSetup();
        private BillboardBuffer _billboardBuffer;

        public float FarClip
        { set { _effectSetup.Param_FarClip.SetValue(value); } }
        public Texture2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }
        public float AspectRatio
        { set { _effectSetup.Param_AspectRatio.SetValue(value); } }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _billboardBuffer = new BillboardBuffer(Color.White, graphicsDevice);
        }


        public void DrawSceneBillboards(EntitySceneGroup scene, PipelineMatrices matrices)
        {
            _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VertexBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IndexBuffer);

            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            _effectSetup.Effect.CurrentTechnique = _effectSetup.Technique_Id;

            List<Decal> decals = scene.Decals;
            List<PointLight> pointLights = scene.PointLights;
            List<Pipeline.Lighting.DirectionalLight> dirLights = scene.DirectionalLights;

            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawSceneBillboard(decal.World, matrices, decal.Id);
            }

            for (int index = 0; index < pointLights.Count; index++)
            {
                var light = pointLights[index];
                DrawSceneBillboard(light.World, matrices, light.Id);
            }

            for (int index = 0; index < dirLights.Count; index++)
            {
                var light = dirLights[index];
                DrawSceneBillboard(light.World, matrices, light.Id);
            }

            Debug.WriteLine("DrawSceneBillboards: " + this + " => " + scene.EnvProbe.World);
            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
            DrawSceneBillboard(scene.EnvProbe.World, matrices, scene.EnvProbe.Id);

        }
        private void DrawSceneBillboard(Matrix world, PipelineMatrices matrices, int id)
        {
            _effectSetup.Param_WorldViewProj.SetValue(world * matrices.StaticViewProjection);
            _effectSetup.Param_WorldView.SetValue(world * matrices.View);
            _effectSetup.Param_IdColor.SetValue(IdGenerator.GetColorFromId(id).ToVector3());
            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }



        public void DrawEditorBillboards(EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VertexBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IndexBuffer);

            _effectSetup.Effect.CurrentTechnique = _effectSetup.Technique_Billboard;
            _effectSetup.Param_IdColor.SetValue(Color.Gray.ToVector3());

            // Decals
            List<Decal> decals = scene.Decals;
            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconDecal);
            for (int i = 0; i < decals.Count; i++)
                DrawEditorBillboard(decals[i], matrices, gizmoContext);

            // Point Lights
            List<PointLight> pointLights = scene.PointLights;
            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            for (int i = 0; i < pointLights.Count; i++)
                DrawEditorBillboard(pointLights[i], matrices, gizmoContext);

            //DirectionalLights
            List<Pipeline.Lighting.DirectionalLight> dirLights = scene.DirectionalLights;
            HelperGeometryManager helperManager = HelperGeometryManager.GetInstance();
            for (int i = 0; i < dirLights.Count; i++)
            {
                Pipeline.Lighting.DirectionalLight light = dirLights[i];
                DrawEditorBillboard(light, matrices, gizmoContext);

                helperManager.AddLineStartDir(light.Position, light.Direction * 10, 1, Color.Black, light.Color);

                helperManager.AddLineStartDir(light.Position + Vector3.UnitX * 10, light.Direction, 1, Color.Black, light.Color);
                helperManager.AddLineStartDir(light.Position - Vector3.UnitX * 10, light.Direction, 1, Color.Black, light.Color);
                helperManager.AddLineStartDir(light.Position + Vector3.UnitY * 10, light.Direction, 1, Color.Black, light.Color);
                helperManager.AddLineStartDir(light.Position - Vector3.UnitY * 10, light.Direction, 1, Color.Black, light.Color);
                helperManager.AddLineStartDir(light.Position + Vector3.UnitZ * 10, light.Direction, 1, Color.Black, light.Color);
                helperManager.AddLineStartDir(light.Position - Vector3.UnitZ * 10, light.Direction, 1, Color.Black, light.Color);

                if (light.CastShadows)
                {
                    BoundingFrustum boundingFrustumShadow = new BoundingFrustum(light.Matrices.ViewProjection);
                    helperManager.CreateBoundingBoxLines(boundingFrustumShadow);
                }
            }

            //EnvMap
            if (scene.EnvProbe != null)
            {
                _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
                DrawEditorBillboard(scene.EnvProbe, matrices, gizmoContext);
            }
        }
        private void DrawEditorBillboard(TransformableObject billboardObject, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            Matrix world = Matrix.CreateTranslation(billboardObject.Position);
            _effectSetup.Param_WorldViewProj.SetValue(world * matrices.StaticViewProjection);
            _effectSetup.Param_WorldView.SetValue(world * matrices.View);

            if (billboardObject.Id == IdAndOutlineRenderer.HoveredId)
                _effectSetup.Param_IdColor.SetValue(Color.White.ToVector3());
            if (billboardObject.Id == gizmoContext.SelectedObjectId)
                _effectSetup.Param_IdColor.SetValue(Color.Gold.ToVector3());

            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            if (billboardObject.Id == IdAndOutlineRenderer.HoveredId || billboardObject.Id == gizmoContext.SelectedObjectId)
                _effectSetup.Param_IdColor.SetValue(Color.Gray.ToVector3());
        }



    }
}
