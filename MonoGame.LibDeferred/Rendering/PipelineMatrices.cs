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
        public void UpdateFromView()
        {
            //Create our projection matrices
            InverseView = Matrix.Invert(View);
            ViewProjection = View * Projection;
            InverseViewProjection = Matrix.Invert(ViewProjection);
            ViewIT = Matrix.Transpose(InverseView);

        }
    }

}

