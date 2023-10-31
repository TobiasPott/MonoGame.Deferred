using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Rendering.Helper.Editor;
using DeferredEngine.Rendering.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System.Diagnostics;

namespace DeferredEngine.Pipeline.Utilities
{
    public partial class BillboardRenderModule : PipelineModule
    {


        public IdAndOutlineRenderModule IdAndOutlineRenderer;
        private readonly BillboardEffectSetup _fxSetup = new BillboardEffectSetup();
        private BillboardBuffer _billboardBuffer;

        public Texture2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }
        public float AspectRatio
        { set { _fxSetup.Param_AspectRatio.SetValue(value); } }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _billboardBuffer = new BillboardBuffer(Color.White, graphicsDevice);
        }


        public void DrawSceneBillboards(EntityScene scene)
        {
            _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VertexBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IndexBuffer);

            _fxSetup.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);
            _fxSetup.Effect.CurrentTechnique = _fxSetup.Technique_Id;

            List<Decal> decals = scene.Decals;
            List<PointLight> pointLights = scene.PointLights;
            List<Lighting.DirectionalLight> dirLights = scene.DirectionalLights;

            for (int index = 0; index < decals.Count; index++)
            {
                var decal = decals[index];
                DrawSceneBillboard(decal.World, this.Matrices, decal.Id);
            }

            for (int index = 0; index < pointLights.Count; index++)
            {
                var light = pointLights[index];
                DrawSceneBillboard(light.World, this.Matrices, light.Id);
            }

            for (int index = 0; index < dirLights.Count; index++)
            {
                var light = dirLights[index];
                DrawSceneBillboard(light.World, this.Matrices, light.Id);
            }

            Debug.WriteLine("DrawSceneBillboards: " + this + " => " + scene.EnvProbe.World);
            _fxSetup.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
            DrawSceneBillboard(scene.EnvProbe.World, this.Matrices, scene.EnvProbe.Id);

        }
        private void DrawSceneBillboard(Matrix world, PipelineMatrices matrices, int id)
        {
            _fxSetup.Param_WorldViewProj.SetValue(world * matrices.StaticViewProjection);
            _fxSetup.Param_WorldView.SetValue(world * matrices.View);
            _fxSetup.Param_IdColor.SetValue(IdGenerator.GetColorFromId(id).ToVector3());
            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }



        public void DrawEditorBillboards(EntityScene scene, GizmoDrawContext gizmoContext)
        {
            _graphicsDevice.SetState(RasterizerStateOption.CullCounterClockwise);
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VertexBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IndexBuffer);

            _fxSetup.Effect.CurrentTechnique = _fxSetup.Technique_Billboard;
            _fxSetup.Param_IdColor.SetValue(Color.Gray.ToVector3());
            _fxSetup.Param_FarClip.SetValue(this.Frustum.FarClip);

            // Decals
            List<Decal> decals = scene.Decals;
            _fxSetup.Param_Texture.SetValue(StaticAssets.Instance.IconDecal);
            for (int i = 0; i < decals.Count; i++)
                DrawEditorBillboard(decals[i], gizmoContext);

            // Point Lights
            List<PointLight> pointLights = scene.PointLights;
            _fxSetup.Param_Texture.SetValue(StaticAssets.Instance.IconLight);
            for (int i = 0; i < pointLights.Count; i++)
                DrawEditorBillboard(pointLights[i], gizmoContext);

            //DirectionalLights
            List<Pipeline.Lighting.DirectionalLight> dirLights = scene.DirectionalLights;
            HelperGeometryManager helperManager = HelperGeometryManager.GetInstance();
            for (int i = 0; i < dirLights.Count; i++)
            {
                Pipeline.Lighting.DirectionalLight light = dirLights[i];
                DrawEditorBillboard(light, gizmoContext);

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
                _fxSetup.Param_Texture.SetValue(StaticAssets.Instance.IconEnvmap);
                DrawEditorBillboard(scene.EnvProbe, gizmoContext);
            }
        }
        private void DrawEditorBillboard(TransformableObject billboardObject, GizmoDrawContext gizmoContext)
        {
            Matrix world = Matrix.CreateTranslation(billboardObject.Position);
            _fxSetup.Param_WorldViewProj.SetValue(world * this.Matrices.StaticViewProjection);
            _fxSetup.Param_WorldView.SetValue(world * this.Matrices.View);

            if (billboardObject.Id == IdAndOutlineRenderer.HoveredId)
                _fxSetup.Param_IdColor.SetValue(Color.White.ToVector3());
            if (billboardObject.Id == gizmoContext.SelectedObjectId)
                _fxSetup.Param_IdColor.SetValue(Color.Gold.ToVector3());

            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            if (billboardObject.Id == IdAndOutlineRenderer.HoveredId || billboardObject.Id == gizmoContext.SelectedObjectId)
                _fxSetup.Param_IdColor.SetValue(Color.Gray.ToVector3());
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }
    }
}
