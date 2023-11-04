using Deferred.Utilities;
using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using MonoGame.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DeferredEngine.Demo
{
    public partial class EditorLogic
    {
        private static EditorLogic _instance = null;
        public static EditorLogic Instance => _instance;

        private bool _gizmoTransformationMode;
        private Vector3 _gizmoPosition;
        private int _gizmoId;
        private GizmoModes _gizmoMode = GizmoModes.Translation;

        public TransformableObject SelectedObject;

        private GraphicsDevice _graphicsDevice;

        private float previousMouseX = 0;
        private float previousMouseY = 0;

        private readonly double mouseMoveTimer = 400;
        private double _mouseMovedNextThreshold;
        public bool HasMouseMoved { get; protected set; }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException($"Only one instance of {nameof(EditorLogic)} can be alive at a time");
        }

        /// <summary>
        /// Main Logic for the editor part
        /// </summary>
        public void Update(GameTime gameTime, EntityScene scene, ObjectHoverContext hoverContext, DynamicMeshBatcher meshBatcher)
        {
            if (!RenderingSettings.e_IsEditorEnabled) return;

            if (!DebugScreen.ConsoleOpen)
            {
                if (Input.WasKeyPressed(Keys.R)) _gizmoMode = GizmoModes.Rotation;
                if (Input.WasKeyPressed(Keys.T)) _gizmoMode = GizmoModes.Translation;
                if (Input.WasKeyPressed(Keys.Z)) _gizmoMode = GizmoModes.Scale;
            }

            Update_MouseMoved(gameTime);

            int hoveredId = hoverContext.HoveredId;

            if (_gizmoTransformationMode)
            {
                if (Input.mouseState.LeftButton == ButtonState.Pressed)
                {
                    GizmoControl(_gizmoId, hoverContext);
                }
                else _gizmoTransformationMode = false;
            }
            else if (Input.WasLMBClicked() && !GUIMouseInput.UIWasUsed)
            {
                previousMouseX = Input.mouseState.X;
                previousMouseY = Input.mouseState.Y;

                //Gizmos
                if (hoveredId >= 1 && hoveredId <= 3)
                {
                    _gizmoId = hoveredId;
                    GizmoControl(_gizmoId, hoverContext);
                    return;
                }

                if (hoveredId <= 0)
                {
                    SelectedObject = null;
                    return;
                }

                bool foundnew = false;

                //Get the selected entity!
                for (int index = 0; index < scene.Entities.Count; index++)
                {
                    var VARIABLE = scene.Entities[index];
                    if (VARIABLE.Id == hoveredId)
                    {
                        SelectedObject = VARIABLE;
                        foundnew = true;
                        break;
                    }
                }
                if (foundnew == false)
                {
                    for (int index = 0; index < scene.Decals.Count; index++)
                    {
                        Decal decal = scene.Decals[index];
                        if (decal.Id == hoveredId)
                        {
                            SelectedObject = decal;
                            break;
                        }
                    }

                    for (int index = 0; index < scene.PointLights.Count; index++)
                    {
                        PointLight pointLight = scene.PointLights[index];
                        if (pointLight.Id == hoveredId)
                        {
                            SelectedObject = pointLight;
                            break;
                        }
                    }

                    for (int index = 0; index < scene.DirectionalLights.Count; index++)
                    {
                        Pipeline.Lighting.DirectionalLight directionalLight = scene.DirectionalLights[index];
                        if (directionalLight.Id == hoveredId)
                        {
                            SelectedObject = directionalLight;
                            break;
                        }
                    }

                    if (scene.EnvProbe.Id == hoveredId)
                    {
                        SelectedObject = scene.EnvProbe;
                    }

                }

            }

            //Controls

            if (Input.WasKeyPressed(Keys.Delete))
            {
                //Find object
                if (SelectedObject is ModelEntity)
                {
                    scene.Entities.Remove((ModelEntity)SelectedObject);
                    meshBatcher.DeleteFromRegistry((ModelEntity)SelectedObject);

                    SelectedObject = null;
                }
                else if (SelectedObject is Decal)
                {
                    scene.Decals.Remove((Decal)SelectedObject);

                    SelectedObject = null;
                }
                else if (SelectedObject is PointLight)
                {
                    scene.PointLights.Remove((PointLight)SelectedObject);

                    SelectedObject = null;

                }
                else if (SelectedObject is Pipeline.Lighting.DirectionalLight)
                {
                    scene.DirectionalLights.Remove((Pipeline.Lighting.DirectionalLight)SelectedObject);

                    SelectedObject = null;
                }
            }

        }

        private void Update_MouseMoved(GameTime gameTime)
        {
            if (RenderingStats.UIIsHovered || Input.mouseState.RightButton == ButtonState.Pressed)
            {
                HasMouseMoved = false;
                return;
            }
            if (Input.mouseState != Input.mouseLastState)
            {
                //reset the timer!
                _mouseMovedNextThreshold = gameTime.TotalGameTime.TotalMilliseconds + mouseMoveTimer;
                HasMouseMoved = true;
            }
            if (_mouseMovedNextThreshold < gameTime.TotalGameTime.TotalMilliseconds)
            {
                HasMouseMoved = false;
            }
        }
        private void GizmoControl(int gizmoId, ObjectHoverContext data)
        {
            if (SelectedObject == null) return;
            //there must be a selected object for a gizmo

            float x = Input.mouseState.X;
            float y = Input.mouseState.Y;



            if (_gizmoMode == GizmoModes.Translation)
            {

                Vector3 pos1 =
                    _graphicsDevice.Viewport.Unproject(new Vector3(x, y, 0),
                        data.ProjectionMatrix, data.ViewMatrix, Matrix.Identity);
                Vector3 pos2 = _graphicsDevice.Viewport.Unproject(new Vector3(x, y, 1),
                    data.ProjectionMatrix, data.ViewMatrix, Matrix.Identity);

                Ray ray = new Ray(pos1, pos2 - pos1);

                Vector3 normal;
                Vector3 binormal;
                Vector3 tangent;

                if (gizmoId == 1)
                {
                    tangent = Vector3.UnitZ;
                    normal = Vector3.UnitZ;
                    binormal = Vector3.UnitY;
                }
                else if (gizmoId == 2)
                {
                    tangent = Vector3.UnitY;
                    normal = Vector3.UnitY;
                    binormal = Vector3.UnitZ;
                }
                else
                {
                    tangent = Vector3.UnitX;
                    normal = Vector3.UnitZ;
                    binormal = Vector3.UnitX;
                }

                if (RenderingSettings.e_LocalTransformation)
                {
                    tangent = Vector3.Transform(tangent, SelectedObject.RotationMatrix);
                    normal = Vector3.Transform(normal, SelectedObject.RotationMatrix);
                    binormal = Vector3.Transform(binormal, SelectedObject.RotationMatrix);
                }

                Plane plane = new Plane(SelectedObject.Position, SelectedObject.Position + normal,
                       SelectedObject.Position + binormal);


                float? d = ray.Intersects(plane);

                if (d == null) return;

                float f = (float)d;

                Vector3 hitPoint = pos1 + (pos2 - pos1) * f;

                if (_gizmoTransformationMode == false)
                {
                    _gizmoTransformationMode = true;
                    _gizmoPosition = hitPoint;
                    return;
                }


                //Get the difference
                Vector3 diff = hitPoint - _gizmoPosition;

                diff = Vector3.Dot(tangent, diff) * tangent;

                //diff.Z *= gizmoId == 1 ? 1 : 0;
                //diff.Y *= gizmoId == 2 ? 1 : 0;
                //diff.X *= gizmoId == 3 ? 1 : 0;

                SelectedObject.Position += diff;

                _gizmoPosition = hitPoint;
            }
            else
            {
                if (_gizmoTransformationMode == false)
                {
                    _gizmoTransformationMode = true;
                    return;
                }

                float diffL = x - previousMouseX + y - previousMouseY;
                diffL /= 50;

                if (Input.keyboardState.IsKeyDown(Keys.LeftControl))
                    gizmoId = 4;

                if (_gizmoMode == GizmoModes.Rotation)
                {

                    if (!RenderingSettings.e_LocalTransformation)
                    {
                        if (gizmoId == 1)
                        {
                            SelectedObject.RotationMatrix *= Matrix.CreateRotationZ((float)diffL);
                        }
                        if (gizmoId == 2)
                        {
                            SelectedObject.RotationMatrix *= Matrix.CreateRotationY((float)diffL);
                        }
                        if (gizmoId == 3)
                        {
                            SelectedObject.RotationMatrix *= Matrix.CreateRotationX((float)diffL);
                        }
                    }
                    else
                    {
                        if (gizmoId == 1)
                        {
                            SelectedObject.RotationMatrix = Matrix.CreateRotationZ((float)diffL) *
                                                            SelectedObject.RotationMatrix;
                        }
                        if (gizmoId == 2)
                        {
                            SelectedObject.RotationMatrix = Matrix.CreateRotationY((float)diffL) *
                                                            SelectedObject.RotationMatrix;
                        }
                        if (gizmoId == 3)
                        {
                            SelectedObject.RotationMatrix = Matrix.CreateRotationX((float)diffL) *
                                                            SelectedObject.RotationMatrix;
                        }
                    }
                }
                else
                {
                    if (gizmoId == 1 || gizmoId == 4)
                    {
                        SelectedObject.Scale = new Vector3(SelectedObject.Scale.X, SelectedObject.Scale.Y, MathHelper.Max(SelectedObject.Scale.Z + (float)diffL, 0.01f));
                    }
                    if (gizmoId == 2 || gizmoId == 4)
                    {
                        SelectedObject.Scale = new Vector3(SelectedObject.Scale.X, MathHelper.Max(SelectedObject.Scale.Y + (float)diffL, 0.01f), SelectedObject.Scale.Z);
                    }
                    if (gizmoId == 3 || gizmoId == 4)
                    {
                        SelectedObject.Scale = new Vector3(MathHelper.Max(SelectedObject.Scale.X + (float)diffL, 0.01f), SelectedObject.Scale.Y, SelectedObject.Scale.Z);
                    }
                }


                previousMouseX = x;
                previousMouseY = y;
            }

        }

        public GizmoDrawContext GetEditorData()
        {
            if (SelectedObject == null)
                return new GizmoDrawContext { SelectedObjectId = 0, SelectedObjectPosition = Vector3.Zero };
            return new GizmoDrawContext
            {
                SelectedObject = SelectedObject,
                SelectedObjectId = SelectedObject.Id,
                SelectedObjectPosition = SelectedObject.Position,
                GizmoTransformationMode = _gizmoTransformationMode,
                GizmoMode = _gizmoMode,

            };
        }

    }
}
