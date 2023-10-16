using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public partial class EnvironmentPipelineModule : PipelineModule
    {

        private FullscreenTriangleBuffer _fullscreenTarget;

        public Texture2D SSRMap
        { set { Shaders.Environment.Param_SSRMap.SetValue(value); } }
        public Vector3[] FrustumCornersWS
        { set { Shaders.Environment.Param_FrustumCorners.SetValue(value); } }
        public Vector3 CameraPositionWS
        { set { Shaders.Environment.Param_CameraPositionWS.SetValue(value); } }
        public Vector2 Resolution
        { set { Shaders.Environment.Param_Resolution.SetValue(value); } }
        public float Time
        { set { Shaders.Environment.Param_Time.SetValue(value); } }


        public bool FireflyReduction
        { set { Shaders.Environment.Param_FireflyReduction.SetValue(value); } }
        public float FireflyThreshold
        { set { Shaders.Environment.Param_FireflyThreshold.SetValue(value); } }

        public float SpecularStrength
        {
            set
            {
                Shaders.Environment.Param_SpecularStrength.SetValue(value);
                Shaders.Environment.Param_SpecularStrengthRcp.SetValue(1.0f / value);
            }
        }
        public float DiffuseStrength
        { set { Shaders.Environment.Param_DiffuseStrength.SetValue(value); } }


        public bool UseSDFAO
        { set { Shaders.Environment.Param_UseSDFAO.SetValue(value); } }


        public EnvironmentPipelineModule(ContentManager content, string shaderPath)
            : base(content, shaderPath)
        {
            this.FireflyReduction = RenderingSettings.g_SSReflection_FireflyReduction;
            this.FireflyThreshold = RenderingSettings.g_SSReflection_FireflyThreshold;
        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            Shaders.Environment.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            Shaders.Environment.Param_NormalMap.SetValue(gBufferTarget.Normal);
            Shaders.Environment.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            Shaders.Environment.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            Shaders.Environment.Param_InstanceScale.SetValue(scales);
            Shaders.Environment.Param_InstanceSDFIndex.SetValue(sdfIndices);
            Shaders.Environment.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            Shaders.Environment.Param_VolumeTex.SetValue(atlas);
            Shaders.Environment.Param_VolumeTexSize.SetValue(texSizes);
            Shaders.Environment.Param_VolumeTexResolution.SetValue(texResolutions);
        }
        public void SetEnvironmentProbe(EnvironmentProbe probe)
        {
            if (probe != null)
            {
                SpecularStrength = probe.SpecularStrength;
                DiffuseStrength = probe.DiffuseStrength;
                UseSDFAO = probe.UseSDFAO;
            }
            else
            {
                SpecularStrength = 0.0f;
                DiffuseStrength = 0.0f;
                UseSDFAO = false;
            }
        }
        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }

        protected override void Load(ContentManager content, string shaderPath)
        { }


        public void DrawEnvironmentMap(Camera camera, Matrix view, GameTime gameTime)
        {
            CameraPositionWS = camera.Position;

            Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;

            Shaders.Environment.Param_TransposeView.SetValue(Matrix.Transpose(view));
            Shaders.Environment.Pass_Basic.Apply();

            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _fullscreenTarget.Draw(_graphicsDevice);
        }
        public void DrawSky()
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.Environment.Pass_Sky.Apply();
            _fullscreenTarget.Draw(_graphicsDevice);
        }


        public override void Dispose()
        {
            Shaders.Environment.Effect?.Dispose();
        }
    }
}


namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {
        // ToDo: Transform to EffectSetup type
        // Deferred Environment
        public static class Environment
        {
            public static Effect Effect = Globals.content.Load<Effect>("Shaders/Deferred/DeferredEnvironmentMap");

            public static EffectPass Pass_Sky = Effect.Techniques["Sky"].Passes[0];
            public static EffectPass Pass_Basic = Effect.Techniques["Basic"].Passes[0];

            //Environment
            public static EffectParameter Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
            public static EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];

            public static EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            public static EffectParameter Param_SSRMap = Effect.Parameters["ReflectionMap"];
            public static EffectParameter Param_ReflectionCubeMap = Effect.Parameters["ReflectionCubeMap"];
            public static EffectParameter Param_Resolution = Effect.Parameters["Resolution"];
            public static EffectParameter Param_FireflyReduction = Effect.Parameters["FireflyReduction"];
            public static EffectParameter Param_FireflyThreshold = Effect.Parameters["FireflyThreshold"];
            public static EffectParameter Param_TransposeView = Effect.Parameters["TransposeView"];
            public static EffectParameter Param_SpecularStrength = Effect.Parameters["EnvironmentMapSpecularStrength"];
            public static EffectParameter Param_SpecularStrengthRcp = Effect.Parameters["EnvironmentMapSpecularStrengthRcp"];
            public static EffectParameter Param_DiffuseStrength = Effect.Parameters["EnvironmentMapDiffuseStrength"];
            public static EffectParameter Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];
            public static EffectParameter Param_Time = Effect.Parameters["Time"];

            //SDF
            public static EffectParameter Param_VolumeTex = Effect.Parameters["VolumeTex"];
            public static EffectParameter Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
            public static EffectParameter Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];
            public static EffectParameter Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            public static EffectParameter Param_InstanceScale = Effect.Parameters["InstanceScale"];
            public static EffectParameter Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            public static EffectParameter Param_InstancesCount = Effect.Parameters["InstancesCount"];

            public static EffectParameter Param_UseSDFAO = Effect.Parameters["UseSDFAO"];
        }


    }
}