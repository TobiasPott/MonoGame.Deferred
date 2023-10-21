using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System.Windows.Forms;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public class PipelineMatrices
    {
        public Matrix View { get; protected set; }
        public Matrix InverseView { get; protected set; }
        public Matrix ViewIT { get; protected set; }
        public Matrix Projection { get; protected set; }
        public Matrix ViewProjection { get; protected set; }

        public Matrix StaticViewProjection { get; protected set; }
        public Matrix InverseViewProjection { get; protected set; }

        public Matrix PreviousViewProjection { get; protected set; }
        public Matrix CurrentViewToPreviousViewProjection { get; protected set; }



        public void SetFromCamera(Camera camera)
        {
            // update our previous projection matrices
            PreviousViewProjection = ViewProjection;
            //View matrix
            View = camera.View;
            Projection = camera.Projection;
            ViewProjection = camera.ViewProjection;


            // update our projection matrices
            InverseView = Matrix.Invert(View);
            ViewIT = Matrix.Transpose(InverseView);
            InverseViewProjection = Matrix.Invert(ViewProjection);

            // this is the unjittered viewProjection. For some effects we don't want the jittered one
            StaticViewProjection = ViewProjection;
            // Transformation for TAA - from current view back to the old view projection
            CurrentViewToPreviousViewProjection = Matrix.Invert(View) * PreviousViewProjection;
        }
        public bool ApplyViewProjectionJitter(int jitterMode, bool isOffFrame, HaltonSequence haltonSequence)
        {
            switch (jitterMode)
            {
                case 0: //2 frames, just basic translation. Worst taa implementation. Not good with the continous integration used
                    {
                        Vector2 translation = Vector2.One * (isOffFrame ? 0.5f : -0.5f);
                        ViewProjection *= (translation / RenderingSettings.g_ScreenResolution).ToMatrixTranslationXY();
                        return true;
                    }
                case 1: // Just random translation
                    {
                        float randomAngle = FastRand.NextAngle();
                        Vector2 translation = (new Vector2((float)Math.Sin(randomAngle), (float)Math.Cos(randomAngle)) / RenderingSettings.g_ScreenResolution) * 0.5f;
                        ViewProjection *= translation.ToMatrixTranslationXY();
                        return true;
                    }
                case 2: // Halton sequence, default
                    {
                        Vector3 translation = haltonSequence.GetNext();
                        ViewProjection *= Matrix.CreateTranslation(translation);
                        return true;
                    }
            }
            return false;
        }

        //private void SetViewFromCubeMapFace(Vector3 origin, CubeMapFace cubeMapFace)
        //{
        //    View = cubeMapFace switch
        //    {
        //        CubeMapFace.NegativeX => Matrix.CreateLookAt(origin, origin + Vector3.Left, Vector3.Up),
        //        CubeMapFace.NegativeY => Matrix.CreateLookAt(origin, origin + Vector3.Down, Vector3.Forward),
        //        CubeMapFace.NegativeZ => Matrix.CreateLookAt(origin, origin + Vector3.Backward, Vector3.Up),
        //        CubeMapFace.PositiveX => Matrix.CreateLookAt(origin, origin + Vector3.Right, Vector3.Up),
        //        CubeMapFace.PositiveY => Matrix.CreateLookAt(origin, origin + Vector3.Up, Vector3.Backward),
        //        CubeMapFace.PositiveZ => Matrix.CreateLookAt(origin, origin + Vector3.Forward, Vector3.Up),
        //        _ => Matrix.CreateLookAt(origin, origin + Vector3.Forward, Vector3.Up),
        //    };

        //}

    }

}

