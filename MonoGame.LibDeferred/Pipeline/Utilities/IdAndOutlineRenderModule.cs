using Deferred.Utilities;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Utilities
{

    public partial class IdAndOutlineRenderModule : PipelineModule
    {
        public static bool e_DrawOutlines = true;

        private readonly Vector4 HoveredColor = new Vector4(1, 1, 1, 0.1f);
        private readonly Vector4 SelectedColor = new Vector4(1, 1, 0, 0.1f);

        public enum Pass
        {
            Color,
            Id
        }


        private Color[] _readbackIdColor = new Color[1];

        public RenderTarget2D Target { get; protected set; }
        private RenderContext _renderContext = new RenderContext() { Flags = RenderFlags.Outlined };

        public BillboardRenderModule BillboardRenderer;
        public int HoveredId;

        public IdAndOutlineRenderModule()
            : base()
        { }

        public void SetUpRenderTarget(Vector2 resolution)
        {
            if (Target != null)
                Target.Dispose();
            Target = RenderTarget2DDefinition.Aux_Id.CreateRenderTarget(_graphicsDevice, resolution);
        }

        public void Draw(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, GizmoDrawContext drawContext, bool mouseMoved)
        {
            if (drawContext.GizmoTransformationMode)
            {
                _graphicsDevice.SetRenderTarget(Target);
                _graphicsDevice.Clear(Color.Black);
                return;
            }

            if (mouseMoved)
                DrawIds(meshBatcher, scene, this.Matrices, drawContext);

            if (IdAndOutlineRenderModule.e_DrawOutlines)
                DrawOutlines(meshBatcher, this.Matrices, drawContext, HoveredId, mouseMoved);
        }

        private void DrawIds(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {

            _graphicsDevice.SetRenderTarget(Target);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            if (meshBatcher.CheckRequiresRedraw(RenderType.IdRender, false, false))
                meshBatcher.Draw(RenderType.IdRender, matrices, RenderContext.Default);

            //Now onto the billboards
            // ToDo: @tpott: Consider moving Billboards into entities like Decals (but with different effect?! O.o)

            BillboardRenderer?.DrawSceneBillboards(scene);

            //Now onto the gizmos
            DrawTransformGizmos(gizmoContext, Pass.Id);

            Rectangle sourceRectangle = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);
            try
            {
                if (sourceRectangle.X >= 0 && sourceRectangle.Y >= 0 && sourceRectangle.X < Target.Width - 2 && sourceRectangle.Y < Target.Height - 2)
                    Target.GetData(0, sourceRectangle, _readbackIdColor, 0, 1);
            }
            catch
            {
                //nothing
            }

            HoveredId = IdGenerator.GetIdFromColor(_readbackIdColor[0]);
        }


        public void DrawTransformGizmos(GizmoDrawContext gizmoContext, Pass pass = Pass.Color)
        {
            if (pass == Pass.Color)
                DrawTransformGizmo(this.Matrices.StaticViewProjection, gizmoContext, IdAndOutlineRenderData.AxisColors);
            else
                DrawTransformGizmo(this.Matrices.StaticViewProjection, gizmoContext, IdAndOutlineRenderData.AxisIdColors);
        }

        private void DrawTransformGizmo(Matrix staticViewProjection, GizmoDrawContext gizmoContext, Color[] axisColors)
        {
            if (gizmoContext.SelectedObjectId == 0) return;

            Vector3 position = gizmoContext.SelectedObjectPosition;
            GizmoModes gizmoMode = gizmoContext.GizmoMode;
            Matrix rotation = (RenderingSettings.e_LocalTransformation || gizmoContext.GizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, IdAndOutlineRenderData.AxisAngles[0], this.HoveredId == IdAndOutlineRenderData.ID_AXIS_X ? 1.5f : 1.0f, axisColors[0], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, IdAndOutlineRenderData.AxisAngles[1], this.HoveredId == IdAndOutlineRenderData.ID_AXIS_Y ? 1.5f : 1.0f, axisColors[1], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, IdAndOutlineRenderData.AxisAngles[2], this.HoveredId == IdAndOutlineRenderData.ID_AXIS_Z ? 1.5f : 1.0f, axisColors[2], staticViewProjection, gizmoMode);

            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, IdAndOutlineRenderData.AxisAngles[3], this.HoveredId == IdAndOutlineRenderData.ID_AXIS_X ? 1.5f : 1.0f, axisColors[0], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, IdAndOutlineRenderData.AxisAngles[4], this.HoveredId == IdAndOutlineRenderData.ID_AXIS_Y ? 1.5f : 1.0f, axisColors[1], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, IdAndOutlineRenderData.AxisAngles[5], this.HoveredId == IdAndOutlineRenderData.ID_AXIS_Z ? 1.5f : 1.0f, axisColors[2], staticViewProjection, gizmoMode);

        }

        private static void DrawTransformGizmoAxis(GraphicsDevice graphicsDevice, Vector3 position, Matrix rotationObject, Vector3 angles, float scale,
            Color color, Matrix staticViewProjection, GizmoModes gizmoMode = GizmoModes.Translation, Vector3? direction = null)
        {
            Matrix rotation = (direction != null) ? Matrix.CreateLookAt(Vector3.Zero, (Vector3)direction, Vector3.UnitX) : angles.ToMatrixRotationXYZ();
            Matrix scaleMatrix = Matrix.CreateScale(scale, scale, 1.0f);
            Matrix worldViewProj = scaleMatrix * rotation * rotationObject * Matrix.CreateTranslation(position) * staticViewProjection;

            IdAndOutlineEffectSetup.Instance.Param_WorldViewProj.SetValue(worldViewProj);
            IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(color.ToVector4());
            IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();

            ModelMeshPart meshpart = gizmoMode == GizmoModes.Translation ? StaticAssets.Instance.EditorArrow3DMeshPart : StaticAssets.Instance.EditorArrow3DRoundMeshPart;
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            graphicsDevice.Indices = (meshpart.IndexBuffer);
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
        }

        private void DrawOutlines(DynamicMeshBatcher meshBatcher, PipelineMatrices matrices, GizmoDrawContext gizmoContext, int hoveredId, bool mouseMoved)
        {
            _graphicsDevice.SetRenderTarget(Target);

            if (!mouseMoved)
                _graphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);
            else
            {
                _graphicsDevice.Clear(Color.Black);
            }
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);


            bool needsRedraw = meshBatcher.CheckRequiresRedraw(RenderType.IdRender, false, false);
            int selectedId = gizmoContext.SelectedObjectId;

            //Selected entity
            if (selectedId != 0)
            {
                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(SelectedColor);
                if (needsRedraw)
                {
                    _renderContext.OutlineId = selectedId;
                    meshBatcher.Draw(RenderType.IdOutline, matrices, _renderContext);
                }
            }
            // Hovered entity
            if (selectedId != hoveredId && hoveredId != 0 && mouseMoved)
            {
                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(HoveredColor);
                if (needsRedraw)
                {
                    _renderContext.OutlineId = hoveredId;
                    meshBatcher.Draw(RenderType.IdOutline, matrices, _renderContext);
                }
            }

        }

        public override void Dispose()
        {
            Target?.Dispose();
        }
    }
  
    public static class IdAndOutlineRenderData
    {
        public const int ID_AXIS_X = 1;
        public const int ID_AXIS_Y = 2;
        public const int ID_AXIS_Z = 3;

        public static readonly Vector3[] AxisAngles = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3((float)-Math.PI / 2.0f, 0, 0),
            new Vector3(0, (float)Math.PI / 2.0f, 0),
            new Vector3((float)Math.PI, 0, 0),
            new Vector3((float)Math.PI / 2.0f, 0, 0),
            new Vector3(0, (float)-Math.PI / 2.0f, 0)
        };
        public static readonly Color[] AxisColors = new Color[] {
            Color.Blue,
            Color.Green,
            Color.Red,
        };
        public static readonly Color[] AxisIdColors = new Color[] {
            new Color(ID_AXIS_X, 0, 0),
            new Color(ID_AXIS_Y, 0, 0),
            new Color(ID_AXIS_Z, 0, 0),
        };

    }

}
