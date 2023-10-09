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

        private bool _fireflyReduction;
        private float _fireflyThreshold;
        private float _specularStrength;
        private float _diffuseStrength;
        private bool _useSDFAO;


        public Texture2D SSRMap
        {
            set { Shaders.Environment.Param_SSRMap.SetValue(value); }
        }

        public Vector3[] FrustumCornersWS
        {
            set { Shaders.Environment.Param_FrustumCorners.SetValue(value); }
        }

        public Vector3 CameraPositionWS
        {
            set { Shaders.Environment.Param_CameraPositionWS.SetValue(value); }
        }

        public Vector2 Resolution
        {
            set { Shaders.Environment.Param_Resolution.SetValue(value); }
        }

        public float Time
        {
            set { Shaders.Environment.Param_Time.SetValue(value); }
        }

        public bool FireflyReduction
        {
            get { return _fireflyReduction; }
            set
            {
                if (value != _fireflyReduction)
                {
                    _fireflyReduction = value;
                    Shaders.Environment.Param_FireflyReduction.SetValue(value);
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
                    Shaders.Environment.Param_FireflyThreshold.SetValue(value);
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
                    Shaders.Environment.Param_SpecularStrength.SetValue(value);
                    Shaders.Environment.Param_SpecularStrengthRcp.SetValue(1.0f / value);
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
                    Shaders.Environment.Param_DiffuseStrength.SetValue(value);
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
                    Shaders.Environment.Param_UseSDFAO.SetValue(value);
                }
            }
        }

        public EnvironmentPipelineModule(ContentManager content, string shaderPath)
            : base(content, shaderPath)
        { }

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

        protected override void Load(ContentManager content, string shaderPath)
        { }


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
            Shaders.Environment.Param_TransposeView.SetValue(Matrix.Transpose(view));
            Shaders.Environment.Pass_Basic.Apply();
            fullscreenTarget.Draw(_graphicsDevice);
        }

        public void DrawSky(GraphicsDevice graphicsDevice, FullscreenTriangleBuffer fullscreenTarget)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.Environment.Pass_Sky.Apply();
            fullscreenTarget.Draw(graphicsDevice);
        }

        public override void Dispose()
        {
            Shaders.Environment.Effect?.Dispose();
        }
    }
}
