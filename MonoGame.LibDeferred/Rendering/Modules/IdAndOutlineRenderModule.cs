using Deferred.Utilities;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Ext;
using System.Diagnostics;

namespace DeferredEngine.Renderer.RenderModules
{
    public partial class IdAndOutlineRenderModule
    {
        public static bool e_DrawOutlines = true;

        // ToDo: check if gizmo alignment matches ids and axis order
        public const int ID_AXIS_X = 1;
        public const int ID_AXIS_Y = 2;
        public const int ID_AXIS_Z = 3;

        private static readonly Vector3[] AxisAngles = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3((float)-Math.PI / 2.0f, 0, 0),
            new Vector3(0, (float)Math.PI / 2.0f, 0),
            new Vector3((float)Math.PI, 0, 0),
            new Vector3((float)Math.PI / 2.0f, 0, 0),
            new Vector3(0, (float)-Math.PI / 2.0f, 0)
        };
        private static readonly Color[] AxisColors = new Color[] {
            Color.Blue,
            Color.Green,
            Color.Red,
        };
        private static readonly Color[] AxisIdColors = new Color[] {
            new Color(ID_AXIS_X, 0, 0),
            new Color(ID_AXIS_Y, 0, 0),
            new Color(ID_AXIS_Z, 0, 0),
        };

        private readonly Vector4 HoveredColor = new Vector4(1, 1, 1, 0.1f);
        private readonly Vector4 SelectedColor = new Vector4(1, 1, 0, 0.1f);

        public enum Pass
        {
            Color,
            Id
        }


        public BillboardRenderModule BillboardRenderer;
        private Color[] _readbackIdColor = new Color[1];
        private GraphicsDevice _graphicsDevice;

        private RenderTarget2D _renderTarget;

        public int HoveredId;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public RenderTarget2D GetRenderTarget2D()
        {
            return _renderTarget;
        }


        public void SetUpRenderTarget(int width, int height)
        {
            if (_renderTarget != null)
                _renderTarget.Dispose();
            _renderTarget = RenderTarget2DDefinition.Aux_Id.CreateRenderTarget(_graphicsDevice, width, height);
        }

        public void Draw(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext drawContext, bool mouseMoved)
        {
            if (drawContext.GizmoTransformationMode)
            {
                _graphicsDevice.SetRenderTarget(_renderTarget);
                _graphicsDevice.Clear(Color.Black);
                return;
            }

            if (mouseMoved)
                DrawIds(meshBatcher, scene, matrices, drawContext);

            if (IdAndOutlineRenderModule.e_DrawOutlines)
                DrawOutlines(meshBatcher, matrices, drawContext, mouseMoved, HoveredId, mouseMoved);
        }

        private void DrawIds(DynamicMeshBatcher meshBatcher, EntitySceneGroup scene, PipelineMatrices matrices, GizmoDrawContext gizmoContext)
        {

            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdRender, matrices);

            //Now onto the billboards
            // ToDo: @tpott: Consider moving Billboards into entities like Decals (but with different effect?! O.o)

            BillboardRenderer?.DrawSceneBillboards(scene, matrices);

            //Now onto the gizmos
            DrawTransformGizmos(matrices, gizmoContext, Pass.Id);

            Rectangle sourceRectangle = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);
            try
            {
                if (sourceRectangle.X >= 0 && sourceRectangle.Y >= 0 && sourceRectangle.X < _renderTarget.Width - 2 && sourceRectangle.Y < _renderTarget.Height - 2)
                    _renderTarget.GetData(0, sourceRectangle, _readbackIdColor, 0, 1);
            }
            catch
            {
                //nothing
            }

            HoveredId = IdGenerator.GetIdFromColor(_readbackIdColor[0]);
        }


        public void DrawTransformGizmos(PipelineMatrices matrices, GizmoDrawContext gizmoContext, Pass pass = Pass.Color)
        {
            if (pass == Pass.Color)
                DrawTransformGizmo(matrices.StaticViewProjection, gizmoContext, AxisColors);
            else
                DrawTransformGizmo(matrices.StaticViewProjection, gizmoContext, AxisIdColors);
        }

        private void DrawTransformGizmo(Matrix staticViewProjection, GizmoDrawContext gizmoContext, Color[] axisColors)
        {
            if (gizmoContext.SelectedObjectId == 0) return;

            Vector3 position = gizmoContext.SelectedObjectPosition;
            GizmoModes gizmoMode = gizmoContext.GizmoMode;
            Matrix rotation = (RenderingStats.e_LocalTransformation || gizmoContext.GizmoMode == GizmoModes.Scale) ? gizmoContext.SelectedObject.RotationMatrix : Matrix.Identity;

            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, AxisAngles[0], this.HoveredId == ID_AXIS_X ? 1 : 0.5f, axisColors[0], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, AxisAngles[1], this.HoveredId == ID_AXIS_Y ? 1 : 0.5f, axisColors[1], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, AxisAngles[2], this.HoveredId == ID_AXIS_Z ? 1 : 0.5f, axisColors[2], staticViewProjection, gizmoMode);

            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, AxisAngles[3], this.HoveredId == ID_AXIS_X ? 1 : 0.5f, axisColors[0], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, AxisAngles[4], this.HoveredId == ID_AXIS_Y ? 1 : 0.5f, axisColors[1], staticViewProjection, gizmoMode);
            DrawTransformGizmoAxis(_graphicsDevice, position, rotation, AxisAngles[5], this.HoveredId == ID_AXIS_Z ? 1 : 0.5f, axisColors[2], staticViewProjection, gizmoMode);

        }

        private static void DrawTransformGizmoAxis(GraphicsDevice graphicsDevice, Vector3 position, Matrix rotationObject, Vector3 angles, float scale,
            Color color, Matrix staticViewProjection, GizmoModes gizmoMode = GizmoModes.Translation, Vector3? direction = null)
        {
            Matrix rotation = (direction != null) ? Matrix.CreateLookAt(Vector3.Zero, (Vector3)direction, Vector3.UnitX) : angles.ToMatrixRotationXYZ();
            Matrix scaleMatrix = Matrix.CreateScale(0.75f, 0.75f, scale * 1.5f);
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

        private void DrawOutlines(DynamicMeshBatcher meshBatcher, PipelineMatrices matrices, GizmoDrawContext gizmoContext, bool drawAll, int hoveredId, bool mouseMoved)
        {
            _graphicsDevice.SetRenderTarget(_renderTarget);

            if (!mouseMoved)
                _graphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);
            else
            {
                _graphicsDevice.Clear(Color.Black);
            }
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


            int selectedId = gizmoContext.SelectedObjectId;
            //Selected entity
            if (selectedId != 0)
            {
                //UPdate the size of our outlines!

                if (!drawAll)
                    meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, false, selectedId);

                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(SelectedColor);
                meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, outlined: true, outlineId: selectedId);

                if (selectedId != hoveredId && hoveredId != 0 && mouseMoved)
                {
                    if (!drawAll)
                        meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, false, hoveredId);

                    IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(HoveredColor);
                    meshBatcher.Draw(DynamicMeshBatcher.RenderType.IdOutline, matrices, false, false, outlined: true, outlineId: hoveredId);
                }
            }

        }

    }
}
