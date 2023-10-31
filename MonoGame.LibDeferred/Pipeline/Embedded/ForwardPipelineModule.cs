using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Pipeline
{
    public class ForwardPipelineModule : PipelineModule, IRenderModule
    {
        //Forward pass
        public static bool g_EnableForward = true;


        private const int MAXLIGHTS = 40;

        private Vector3[] LightPositionWS;
        private float[] LightRadius;
        private float[] LightIntensity;
        private Vector3[] LightColor;

        private readonly ForwardFxSetup _fxSetup = new ForwardFxSetup();

        public DepthReconstructPipelineModule DepthReconstruct;

        public ForwardPipelineModule()
            : base()
        { }
        public void Draw(DynamicMeshBatcher meshBatcher, RenderTarget2D sourceRT, RenderTarget2D auxRT, RenderTarget2D destRT)
        {
            // ToDo: Wrap into instance property with global override/flag
            if (ForwardPipelineModule.g_EnableForward)
            {
                _graphicsDevice.SetRenderTarget(destRT);
                // reconstruct depth
                DepthReconstruct?.ReconstructDepth();
                if (meshBatcher.CheckRequiresRedraw(RenderType.Forward, false, false))
                    meshBatcher.Draw(RenderType.Forward, this.Matrices, RenderContext.Default, this);
            }
        }

        public void SetupLighting(Vector3 viewOrigin, List<PointLight> pointLights, BoundingFrustum frustum)
        {
            int count = pointLights.Count > MAXLIGHTS ? MAXLIGHTS : pointLights.Count;

            if (LightPositionWS == null || pointLights.Count != LightPositionWS.Length)
            {
                LightPositionWS = new Vector3[count];
                LightColor = new Vector3[count];
                LightIntensity = new float[count];
                LightRadius = new float[count];
            }

            //Fill
            int lightsInBounds = 0;

            foreach (PointLight light in pointLights)
            {
                //Check frustum culling
                if (frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint) continue;

                LightPositionWS[lightsInBounds] = light.Position;
                LightColor[lightsInBounds] = light.Color_sRGB;
                LightIntensity[lightsInBounds] = light.Intensity;
                LightRadius[lightsInBounds] = light.Radius;
                lightsInBounds++;
            }

            //Setup camera
            _fxSetup.Param_CameraPositionWS.SetValue(viewOrigin);

            _fxSetup.Param_LightAmount.SetValue(lightsInBounds);
            _fxSetup.Param_LightPositionWS.SetValue(LightPositionWS);
            _fxSetup.Param_LightColor.SetValue(LightColor);
            _fxSetup.Param_LightIntensity.SetValue(LightIntensity);
            _fxSetup.Param_LightRadius.SetValue(LightRadius);
        }


        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            _fxSetup.Param_World.SetValue(localWorldMatrix);
            _fxSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
            _fxSetup.Param_WorldViewIT.SetValue(Matrix.Transpose(Matrix.Invert(localWorldMatrix)));
            _fxSetup.Pass_Default.Apply();
        }

    }

}