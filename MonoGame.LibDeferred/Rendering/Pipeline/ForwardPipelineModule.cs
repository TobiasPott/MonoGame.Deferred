using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
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
        public RenderTarget2D Draw(DynamicMeshBatcher meshMat, RenderTarget2D output, PipelineMatrices matrices)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshMat.Draw(DynamicMeshBatcher.RenderType.Forward, matrices, renderModule: this);

            return output;
        }

        public void PrepareDraw(Camera camera, List<DeferredPointLight> pointLights, BoundingFrustum frustum)
        {
            SetupLighting(camera, pointLights, frustum);

        }

        private void SetupLighting(Camera camera, List<DeferredPointLight> pointLights, BoundingFrustum frustum)
        {
            //Setup camera
            Shaders.Forward.Param_CameraPositionWS.SetValue(camera.Position);

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
                DeferredPointLight light = pointLights[index];

                //Check frustum culling
                if (frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint) continue;

                LightPositionWS[lightsInBounds] = light.Position;
                LightColor[lightsInBounds] = light.ColorV3;
                LightIntensity[lightsInBounds] = light.Intensity;
                LightRadius[lightsInBounds] = light.Radius;
                lightsInBounds++;
            }

            Shaders.Forward.Param_LightAmount.SetValue(lightsInBounds);
            Shaders.Forward.Param_LightPositionWS.SetValue(LightPositionWS);
            Shaders.Forward.Param_LightColor.SetValue(LightColor);
            Shaders.Forward.Param_LightIntensity.SetValue(LightIntensity);
            Shaders.Forward.Param_LightRadius.SetValue(LightRadius);
        }


        public override void Dispose()
        {
            Shaders.Forward.Effect?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Shaders.Forward.Param_World.SetValue(localWorldMatrix);
            Shaders.Forward.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
            Shaders.Forward.Param_WorldViewIT.SetValue(Matrix.Transpose(Matrix.Invert(localWorldMatrix)));
            Shaders.Forward.Pass_Default.Apply();
        }
    }
}

namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {

        // Forward
        public static class Forward
        {
            public static Effect Effect = Globals.content.Load<Effect>("Shaders/forward/forward");

            public static EffectPass Pass_Default = Effect.Techniques["Default"].Passes[0];

            public static EffectParameter Param_World = Effect.Parameters["World"];
            public static EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            public static EffectParameter Param_WorldViewIT = Effect.Parameters["WorldViewIT"];
            public static EffectParameter Param_LightAmount = Effect.Parameters["LightAmount"];
            public static EffectParameter Param_LightPositionWS = Effect.Parameters["LightPositionWS"];
            public static EffectParameter Param_LightRadius = Effect.Parameters["LightRadius"];
            public static EffectParameter Param_LightIntensity = Effect.Parameters["LightIntensity"];
            public static EffectParameter Param_LightColor = Effect.Parameters["LightColor"];
            public static EffectParameter Param_TiledListLength = Effect.Parameters["TiledListLength"];
            public static EffectParameter Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];

        }

    }
}