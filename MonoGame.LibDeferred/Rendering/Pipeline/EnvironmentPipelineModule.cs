using DeferredEngine.Entities;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class EnvironmentPipelineModule : RenderingPipelineModule
    {
        private Effect Effect;

        private EffectPass Pass_Basic;
        private EffectPass Pass_Sky;


        private EffectParameter Param_AlbedoMap;
        private EffectParameter Param_NormalMap;
        private EffectParameter Param_DepthMap;

        private EffectParameter Param_SSRMap;
        private EffectParameter Param_FrustumCorners;
        private EffectParameter Param_CameraPositionWS;
        private EffectParameter Param_ReflectionCubeMap;
        private EffectParameter Param_Resolution;
        private EffectParameter Param_FireflyReduction;
        private EffectParameter Param_FireflyThreshold;
        private EffectParameter Param_TransposeView;
        private EffectParameter Param_SpecularStrength;
        private EffectParameter Param_SpecularStrengthRcp;
        private EffectParameter Param_DiffuseStrength;
        private EffectParameter Param_Time;

        public EffectParameter Param_VolumeTex;
        public EffectParameter Param_VolumeTexSize;
        public EffectParameter Param_VolumeTexResolution;

        private EffectParameter Param_InstanceInverseMatrix;
        private EffectParameter Param_InstanceScale;
        private EffectParameter Param_InstanceSDFIndex;
        private EffectParameter Param_InstancesCount;

        public EffectParameter Param_UseSDFAO;


        private bool _fireflyReduction;
        private float _fireflyThreshold;
        private float _specularStrength;
        private float _diffuseStrength;
        private bool _useSDFAO;

        public RenderTargetCube Cubemap
        {
            set { Param_ReflectionCubeMap.SetValue(value); }
        }

        public Texture2D SSRMap
        {
            set { Param_SSRMap.SetValue(value); }
        }

        public Vector3[] FrustumCornersWS
        {
            set { Param_FrustumCorners.SetValue(value); }
        }

        public Vector3 CameraPositionWS
        {
            set { Param_CameraPositionWS.SetValue(value); }
        }

        public Vector2 Resolution
        {
            set { Param_Resolution.SetValue(value); }
        }

        public float Time
        {
            set { Param_Time.SetValue(value); }
        }

        public bool FireflyReduction
        {
            get { return _fireflyReduction; }
            set
            {
                if (value != _fireflyReduction)
                {
                    _fireflyReduction = value;
                    Param_FireflyReduction.SetValue(value);
                }
            }
        }

        public float FireflyThreshold
        {
            get { return _fireflyThreshold; }
            set
            {
                if (Math.Abs(value - _fireflyThreshold) > 0.0001f)
                {
                    _fireflyThreshold = value;
                    Param_FireflyThreshold.SetValue(value);
                }
            }
        }

        public float SpecularStrength
        {
            get { return _specularStrength; }
            set
            {
                if (Math.Abs(value - _specularStrength) > 0.0001f)
                {
                    _specularStrength = value;
                    Param_SpecularStrength.SetValue(value);
                    Param_SpecularStrengthRcp.SetValue(1.0f / value);
                }
            }
        }

        public float DiffuseStrength
        {
            get { return _diffuseStrength; }
            set
            {
                if (Math.Abs(value - _diffuseStrength) > 0.0001f)
                {
                    _diffuseStrength = value;
                    Param_DiffuseStrength.SetValue(value);
                }
            }
        }

        public bool UseSDFAO
        {
            get { return _useSDFAO; }
            set
            {
                if (_useSDFAO != value)
                {
                    _useSDFAO = value;
                    Param_UseSDFAO.SetValue(value);
                }
            }
        }

        public EnvironmentPipelineModule(ContentManager content, string shaderPath)
            : base(content, shaderPath)
        { }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            Param_NormalMap.SetValue(gBufferTarget.Normal);
            Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            this.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            this.Param_InstanceScale.SetValue(scales);
            this.Param_InstanceSDFIndex.SetValue(sdfIndices);
            this.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            this.Param_VolumeTex.SetValue(atlas);
            this.Param_VolumeTexSize.SetValue(texSizes);
            this.Param_VolumeTexResolution.SetValue(texResolutions);
        }

        protected override void Load(ContentManager content, string shaderPath)
        {
            Effect = content.Load<Effect>(shaderPath);

            //Environment
            Param_AlbedoMap = Effect.Parameters["AlbedoMap"];
            Param_NormalMap = Effect.Parameters["NormalMap"];
            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_SSRMap = Effect.Parameters["ReflectionMap"];
            Param_ReflectionCubeMap = Effect.Parameters["ReflectionCubeMap"];
            Param_Resolution = Effect.Parameters["Resolution"];
            Param_FireflyReduction = Effect.Parameters["FireflyReduction"];
            Param_FireflyThreshold = Effect.Parameters["FireflyThreshold"];
            Param_TransposeView = Effect.Parameters["TransposeView"];
            Param_SpecularStrength = Effect.Parameters["EnvironmentMapSpecularStrength"];
            Param_SpecularStrengthRcp = Effect.Parameters["EnvironmentMapSpecularStrengthRcp"];
            Param_DiffuseStrength = Effect.Parameters["EnvironmentMapDiffuseStrength"];
            Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];
            Param_Time = Effect.Parameters["Time"];

            //SDF
            Param_VolumeTex = Effect.Parameters["VolumeTex"];
            Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
            Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];
            Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            Param_InstanceScale = Effect.Parameters["InstanceScale"];
            Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            Param_InstancesCount = Effect.Parameters["InstancesCount"];

            Param_UseSDFAO = Effect.Parameters["UseSDFAO"];

            Pass_Sky = Effect.Techniques["Sky"].Passes[0];
            Pass_Basic = Effect.Techniques["Basic"].Passes[0];
        }


        public void DrawEnvironmentMap(Camera camera, Matrix view, FullscreenTriangleBuffer fullscreenTarget, EnvironmentProbe envSample, GameTime gameTime, bool fireflyReduction, float ffThreshold)
        {
            FireflyReduction = fireflyReduction;
            FireflyThreshold = ffThreshold;

            SpecularStrength = envSample.SpecularStrength;
            DiffuseStrength = envSample.DiffuseStrength;
            CameraPositionWS = camera.Position;

            Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;

            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            UseSDFAO = envSample.UseSDFAO;
            Param_TransposeView.SetValue(Matrix.Transpose(view));
            Pass_Basic.Apply();
            fullscreenTarget.Draw(_graphicsDevice);
        }

        public void DrawSky(GraphicsDevice graphicsDevice, FullscreenTriangleBuffer fullscreenTarget)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Pass_Sky.Apply();
            fullscreenTarget.Draw(graphicsDevice);
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }
}
