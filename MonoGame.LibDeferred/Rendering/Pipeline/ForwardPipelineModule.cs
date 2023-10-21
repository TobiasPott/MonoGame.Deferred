using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
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

        private ForwardEffectSetup _effectSetup = new ForwardEffectSetup();


        public ForwardPipelineModule(ContentManager content, string shaderPath = "Shaders/forward/forward")
            : base(content, shaderPath)
        { }

        //public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        //{
        //    base.Initialize(graphicsDevice, spriteBatch);
        //}
        protected override void Load(ContentManager content, string shaderPath = "Shaders/forward/forward")
        { }

        /// <summary>
        /// Draw forward shaded, alpha blended materials. Very basic and unoptimized algorithm. Can be improved to use tiling in future.
        /// </summary>
        public RenderTarget2D Draw(DynamicMeshBatcher meshBatcher, RenderTarget2D output, PipelineMatrices matrices)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (meshBatcher.CheckRequiresRedraw(DynamicMeshBatcher.RenderType.Forward, false, false))
                meshBatcher.Draw(DynamicMeshBatcher.RenderType.Forward, matrices, renderModule: this);

            return output;
        }

        public void PrepareDraw(Camera camera, List<PointLight> pointLights, BoundingFrustum frustum)
        {
            SetupLighting(camera, pointLights, frustum);

        }

        private void SetupLighting(Camera camera, List<PointLight> pointLights, BoundingFrustum frustum)
        {
            int count = pointLights.Count > 40 ? MAXLIGHTS : pointLights.Count;

            if (LightPositionWS == null || pointLights.Count != LightPositionWS.Length)
            {
                LightPositionWS = new Vector3[count];
                LightColor = new Vector3[count];
                LightIntensity = new float[count];
                LightRadius = new float[count];
            }

            //Fill
            int lightsInBounds = 0;

            for (var index = 0; index < count; index++)
            {
                PointLight light = pointLights[index];

                //Check frustum culling
                if (frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint) continue;

                LightPositionWS[lightsInBounds] = light.Position;
                LightColor[lightsInBounds] = light.Color_sRGB;
                LightIntensity[lightsInBounds] = light.Intensity;
                LightRadius[lightsInBounds] = light.Radius;
                lightsInBounds++;
            }

            //Setup camera
            _effectSetup.Param_CameraPositionWS.SetValue(camera.Position);

            _effectSetup.Param_LightAmount.SetValue(lightsInBounds);
            _effectSetup.Param_LightPositionWS.SetValue(LightPositionWS);
            _effectSetup.Param_LightColor.SetValue(LightColor);
            _effectSetup.Param_LightIntensity.SetValue(LightIntensity);
            _effectSetup.Param_LightRadius.SetValue(LightRadius);
        }


        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            _effectSetup.Param_World.SetValue(localWorldMatrix);
            _effectSetup.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
            _effectSetup.Param_WorldViewIT.SetValue(Matrix.Transpose(Matrix.Invert(localWorldMatrix)));
            _effectSetup.Pass_Default.Apply();
        }

    }

}