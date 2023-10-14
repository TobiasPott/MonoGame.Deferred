using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public class PipelineMatrices
    {
        public Matrix View;
        public Matrix InverseView;
        public Matrix ViewIT;
        public Matrix Projection;
        public Matrix ViewProjection;

        public Matrix StaticViewProjection;
        public Matrix InverseViewProjection;

        public Matrix PreviousViewProjection;
        public Matrix CurrentViewToPreviousViewProjection;

        public void SetFromCubeMapFace(Vector3 origin, CubeMapFace cubeMapFace)
        {
            switch (cubeMapFace)
            {
                case CubeMapFace.NegativeX:
                    {
                        View = Matrix.CreateLookAt(origin, origin + Vector3.Left, Vector3.Up);
                        break;
                    }
                case CubeMapFace.NegativeY:
                    {
                        View = Matrix.CreateLookAt(origin, origin + Vector3.Down, Vector3.Forward);
                        break;
                    }
                case CubeMapFace.NegativeZ:
                    {
                        View = Matrix.CreateLookAt(origin, origin + Vector3.Backward, Vector3.Up);
                        break;
                    }
                case CubeMapFace.PositiveX:
                    {
                        View = Matrix.CreateLookAt(origin, origin + Vector3.Right, Vector3.Up);
                        break;
                    }
                case CubeMapFace.PositiveY:
                    {
                        View = Matrix.CreateLookAt(origin, origin + Vector3.Up, Vector3.Backward);
                        break;
                    }
                case CubeMapFace.PositiveZ:
                    {
                        View = Matrix.CreateLookAt(origin, origin + Vector3.Forward, Vector3.Up);
                        break;
                    }
            }
        }
        public void SetFromCamera(Camera camera)
        {
            //View matrix
            View = Matrix.CreateLookAt(camera.Position, camera.Lookat, camera.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView, RenderingSettings.g_ScreenAspect, 1, RenderingSettings.g_farplane);

            // update our projection matrices
            this.UpdateFromView();
        }
        public void UpdateFromView()
        {
            // update our projection matrices
            InverseView = Matrix.Invert(View);
            ViewIT = Matrix.Transpose(InverseView);
            ViewProjection = View * Projection;
            InverseViewProjection = Matrix.Invert(ViewProjection);

            // this is the unjittered viewProjection. For some effects we don't want the jittered one
            StaticViewProjection = ViewProjection;

            // Transformation for TAA - from current view back to the old view projection
            CurrentViewToPreviousViewProjection = Matrix.Invert(View) * PreviousViewProjection;

        }
    }

}

