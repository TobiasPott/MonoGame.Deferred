using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper.Editor;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using DeferredEngine.Renderer.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public partial class BillboardRenderer
    {

        private GraphicsDevice _graphicsDevice;

        public IdAndOutlineRenderer IdAndOutlineRenderer;
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
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            _effectSetup.Effect.CurrentTechnique = _effectSetup.Technique_Id;

            Matrix staticViewProjection = matrices.StaticViewProjection;
            Matrix view = matrices.View;

            List<Decal> decals = scene.Decals;
            List<DeferredPointLight> pointLights = scene.PointLights;
            List<DeferredDirectionalLight> dirLights = scene.DirectionalLights;

            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawSceneBillboard(decal.World, view, staticViewProjection, decal.Id);
            }

            for (int index = 0; index < pointLights.Count; index++)
            {
                var light = pointLights[index];
                DrawSceneBillboard(light.World, view, staticViewProjection, light.Id);
            }

            for (int index = 0; index < dirLights.Count; index++)
            {
                var light = dirLights[index];
                DrawSceneBillboard(light.World, view, staticViewProjection, light.Id);
            }

            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
            DrawSceneBillboard(scene.EnvProbe.World, view, staticViewProjection, scene.EnvProbe.Id);

        }
        private void DrawSceneBillboard(Matrix world, Matrix view, Matrix staticViewProjection, int id)
        {
            _effectSetup.Param_WorldViewProj.SetValue(world * staticViewProjection);
            _effectSetup.Param_WorldView.SetValue(world * view);
            _effectSetup.Param_IdColor.SetValue(IdGenerator.GetColorFromId(id).ToVector3());
            _effectSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }



        public void DrawEditorBillboards(EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {
            List<Decal> decals = scene.Decals;
            List<DeferredPointLight> pointLights = scene.PointLights;
            List<DeferredDirectionalLight> dirLights = scene.DirectionalLights;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            _effectSetup.Effect.CurrentTechnique = _effectSetup.Technique_Billboard;

            _effectSetup.Param_IdColor.SetValue(Color.Gray.ToVector3());

            //Decals
            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconDecal);
            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawEditorBillboard(decal, matrices.StaticViewProjection, matrices.View, gizmoContext);
            }

            //Lights
            _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            for (int index = 0; index < pointLights.Count; index++)
            {
                var light = pointLights[index];
                DrawEditorBillboard(light, matrices.StaticViewProjection, matrices.View, gizmoContext);
            }

            HelperGeometryManager helperManager = HelperGeometryManager.GetInstance();
            //DirectionalLights
            for (var index = 0; index < dirLights.Count; index++)
            {
                DeferredDirectionalLight light = dirLights[index];
                DrawEditorBillboard(light, matrices.StaticViewProjection, matrices.View, gizmoContext);

                Vector3 lPosition = light.Position;
                Vector3 lDirection = light.Direction * 10;
                Color lColor = light.Color;
                helperManager.AddLineStartDir(lPosition, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition + Vector3.UnitX * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition - Vector3.UnitX * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition + Vector3.UnitY * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition - Vector3.UnitY * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition + Vector3.UnitZ * 10, lDirection, 1, Color.Black, lColor);
                helperManager.AddLineStartDir(lPosition - Vector3.UnitZ * 10, lDirection, 1, Color.Black, lColor);

                if (light.CastShadows)
                {
                    BoundingFrustum boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);
                    helperManager.CreateBoundingBoxLines(boundingFrustumShadow);
                }
            }

            //EnvMap
            if (scene.EnvProbe != null)
            {
                _effectSetup.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
                DrawEditorBillboard(scene.EnvProbe, matrices.StaticViewProjection, matrices.View, gizmoContext);
            }
        }
        private void DrawEditorBillboard(TransformableObject billboardObject, Matrix staticViewProjection, Matrix view, GizmoDrawContext gizmoContext)
        {
            Matrix world = Matrix.CreateTranslation(billboardObject.Position);
            _effectSetup.Param_WorldViewProj.SetValue(world * staticViewProjection);
            _effectSetup.Param_WorldView.SetValue(world * view);

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
