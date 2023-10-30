using DeferredEngine.Entities;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline
{
    //Just a template
    public partial class EnvironmentPipelineModule : PipelineModule
    {
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);


        private FullscreenTriangleBuffer _fullscreenTarget;
        private EnvironmentFxSetup _effectSetup = new EnvironmentFxSetup();

        public Texture2D SSRMap
        { set { _effectSetup.Param_SSRMap.SetValue(value); } }

        public Vector2 Resolution
        { set { _effectSetup.Param_Resolution.SetValue(value); } }
        public float Time
        { set { _effectSetup.Param_Time.SetValue(value); } }


        public bool FireflyReduction
        { set { _effectSetup.Param_FireflyReduction.SetValue(value); } }
        public float FireflyThreshold
        { set { _effectSetup.Param_FireflyThreshold.SetValue(value); } }

        public float SpecularStrength
        {
            set
            {
                _effectSetup.Param_SpecularStrength.SetValue(value);
                _effectSetup.Param_SpecularStrengthRcp.SetValue(1.0f / value);
            }
        }
        public float DiffuseStrength
        { set { _effectSetup.Param_DiffuseStrength.SetValue(value); } }


        public bool UseSDFAO
        { set { _effectSetup.Param_UseSDFAO.SetValue(value); } }


        public EnvironmentPipelineModule()
            : base()
        {
            this.FireflyReduction = SSReflectionFx.g_FireflyReduction;
            this.FireflyThreshold = SSReflectionFx.g_FireflyThreshold;
        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            _effectSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _effectSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _effectSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            _effectSetup.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            _effectSetup.Param_InstanceScale.SetValue(scales);
            _effectSetup.Param_InstanceSDFIndex.SetValue(sdfIndices);
            _effectSetup.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            _effectSetup.Param_VolumeTex.SetValue(atlas);
            _effectSetup.Param_VolumeTexSize.SetValue(texSizes);
            _effectSetup.Param_VolumeTexResolution.SetValue(texResolutions);
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

        public void SetViewPosition(Vector3 viewPosition)
        {
            _effectSetup.Param_CameraPositionWS.SetValue(viewPosition);
        }


        public void Draw() => DrawEnvironmentMap();
        private void DrawEnvironmentMap()
        {
            if (EnvironmentPipelineModule.ModuleEnabled)
            {
                _effectSetup.Param_FrustumCorners.SetValue(this.Frustum.WorldSpaceFrustum);
                _effectSetup.Param_TransposeView.SetValue(Matrix.Transpose(this.Matrices.View));
                _effectSetup.Pass_Basic.Apply();

                _graphicsDevice.SetStates(DepthStencilStateOption.None, RasterizerStateOption.CullCounterClockwise);
                _fullscreenTarget.Draw(_graphicsDevice);

                // sample profiler if set
                this.Profiler?.SampleTimestamp(TimestampIndices.Draw_EnvironmentMap);
            }
        }
        public void DrawSky()
        {
            _effectSetup.Param_FrustumCorners.SetValue(this.Frustum.WorldSpaceFrustum);
            _effectSetup.Param_TransposeView.SetValue(Matrix.Transpose(this.Matrices.View));
            _effectSetup.Pass_Sky.Apply();

            _graphicsDevice.SetStates(DepthStencilStateOption.None, RasterizerStateOption.CullCounterClockwise);
            _fullscreenTarget.Draw(_graphicsDevice);
        }


        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }
    }
}
