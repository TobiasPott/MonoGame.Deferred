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


        private EnvironmentProbe _currentProbe;
        private FullscreenTriangleBuffer _fullscreenTarget;
        private readonly EnvironmentFxSetup _fxSetup = new EnvironmentFxSetup();

        public Texture2D SSReflectionMap
        { set { _fxSetup.Param_SSRMap.SetValue(value); } }

        public Vector2 Resolution
        { set { _fxSetup.Param_Resolution.SetValue(value); } }
        public float Time
        { set { _fxSetup.Param_Time.SetValue(value); } }


        public bool FireflyReduction
        { set { _fxSetup.Param_FireflyReduction.SetValue(value); } }
        public float FireflyThreshold
        { set { _fxSetup.Param_FireflyThreshold.SetValue(value); } }


        public EnvironmentPipelineModule()
            : base()
        {
            this.FireflyReduction = SSReflectionFx.g_FireflyReduction;
            this.FireflyThreshold = SSReflectionFx.g_FireflyThreshold;
        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            _fxSetup.Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            _fxSetup.Param_NormalMap.SetValue(gBufferTarget.Normal);
            _fxSetup.Param_DepthMap.SetValue(gBufferTarget.Depth);
        }
        public void SetInstanceData(Matrix[] inverseMatrices, Vector3[] scales, float[] sdfIndices, int count)
        {
            _fxSetup.Param_InstanceInverseMatrix.SetValue(inverseMatrices);
            _fxSetup.Param_InstanceScale.SetValue(scales);
            _fxSetup.Param_InstanceSDFIndex.SetValue(sdfIndices);
            _fxSetup.Param_InstancesCount.SetValue((float)count);
        }
        public void SetVolumeTexParams(Texture atlas, Vector3[] texSizes, Vector4[] texResolutions)
        {
            _fxSetup.Param_VolumeTex.SetValue(atlas);
            _fxSetup.Param_VolumeTexSize.SetValue(texSizes);
            _fxSetup.Param_VolumeTexResolution.SetValue(texResolutions);
        }
        public void SetEnvironmentProbe(EnvironmentProbe probe)
        {
            if (_currentProbe != probe)
            {
                _currentProbe = probe;
                if (probe != null)
                {
                    _fxSetup.Param_SpecularStrengthRcp.SetValue(1.0f / probe.SpecularStrength);
                    _fxSetup.Param_SpecularStrength.SetValue(probe.SpecularStrength);
                    _fxSetup.Param_DiffuseStrength.SetValue(probe.DiffuseStrength);
                    _fxSetup.Param_UseSDFAO.SetValue(_currentProbe.UseSDFAO);
                }
                else
                {
                    _fxSetup.Param_SpecularStrengthRcp.SetValue(1.0f);
                    _fxSetup.Param_SpecularStrength.SetValue(0.0f);
                    _fxSetup.Param_DiffuseStrength.SetValue(0.0f);
                    _fxSetup.Param_UseSDFAO.SetValue(false);
                }
            }
        }
        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
        }

        public void SetViewPosition(Vector3 viewPosition)
        {
            _fxSetup.Param_CameraPositionWS.SetValue(viewPosition);
        }


        public void Draw() => DrawEnvironmentMap();
        private void DrawEnvironmentMap()
        {
            if (EnvironmentPipelineModule.ModuleEnabled)
            {
                // ToDo: PRIO IV: Environment spec and diffuse strength don't seem to pass to the shader
                _fxSetup.Param_FrustumCorners.SetValue(this.Frustum.WorldSpaceFrustum);
                _fxSetup.Param_TransposeView.SetValue(Matrix.Transpose(this.Matrices.View));
                _fxSetup.Pass_Basic.Apply();

                _graphicsDevice.SetStates(DepthStencilStateOption.None, RasterizerStateOption.CullCounterClockwise);
                _fullscreenTarget.Draw(_graphicsDevice);

                // sample profiler if set
                this.Profiler?.SampleTimestamp(ProfilerTimestamps.Draw_EnvironmentMap);
            }
        }
        public void DrawSky()
        {
            if (EnvironmentPipelineModule.ModuleEnabled)
            {
                _fxSetup.Param_FrustumCorners.SetValue(this.Frustum.WorldSpaceFrustum);
                _fxSetup.Param_TransposeView.SetValue(Matrix.Transpose(this.Matrices.View));
                _fxSetup.Pass_Sky.Apply();

                _graphicsDevice.SetStates(DepthStencilStateOption.None, RasterizerStateOption.CullCounterClockwise);
                _fullscreenTarget.Draw(_graphicsDevice);
            }
        }


        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }
    }
}
