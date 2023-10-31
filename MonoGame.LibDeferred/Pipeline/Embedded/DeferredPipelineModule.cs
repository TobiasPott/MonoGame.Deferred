using DeferredEngine.Rendering;
using DeferredEngine.Rendering.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline
{
    public enum DeferredColorSpace
    {
        sRGB,
        Linear
    }

    public class DeferredPipelineModule : PipelineModule
    {

        private readonly DeferredFxSetup _fxSetup = new DeferredFxSetup();
        private FullscreenTriangleBuffer _fullscreenTarget;
        private DeferredColorSpace _colorSpace = DeferredColorSpace.Linear;

        public bool UseSSAOMap
        {
            get { return _fxSetup.Param_UseSSAO.GetValueBoolean(); }
            set { _fxSetup.Param_UseSSAO.SetValue(value); }
        }
        public DeferredColorSpace ColorSpace
        {
            get { return _colorSpace; }
            set
            {
                _colorSpace = value;
                _fxSetup.Effect_Compose.CurrentTechnique = value == DeferredColorSpace.Linear ? _fxSetup.Technique_Linear : _fxSetup.Technique_NonLinear;
            }
        }


        public DeferredPipelineModule()
            : base()
        { }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
            _fxSetup.Param_UseSSAO.SetValue(true);
            _fxSetup.Effect_Compose.CurrentTechnique = _colorSpace == DeferredColorSpace.Linear ? _fxSetup.Technique_Linear : _fxSetup.Technique_NonLinear;
        }


        /// <summary>
        /// Compose the render by combining the albedo channel with the light channels
        /// </summary>
        public RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D auxRT, RenderTarget2D destRT)
        {
            // ToDo: Move 'Compose' to DeferredPipelineModule and pass target buffer as RenderTarget2D parameter
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.KeepState, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);
            
            _fxSetup.Param_UseSSAO.SetValue(SSAmbientOcclustionFx.g_ssao_draw);
            _fxSetup.Effect_Compose.CurrentTechnique = _colorSpace == DeferredColorSpace.Linear ? _fxSetup.Technique_Linear : _fxSetup.Technique_NonLinear;
            //combine!
            _fxSetup.Effect_Compose.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            // sample profiler if set
            this.Profiler?.SampleTimestamp(TimestampIndices.Draw_DeferredCompose);
            return destRT;
        }


        public void SetGBufferParams(GBufferTarget gBuffer)
        {
            _fxSetup.Param_ColorMap.SetValue(gBuffer.Albedo);
            _fxSetup.Param_NormalMap.SetValue(gBuffer.Normal);
        }
        public void SetLightingParams(LightingBufferTarget lightBuffer)
        {
            _fxSetup.Param_DiffuseLightMap.SetValue(lightBuffer.Diffuse);
            _fxSetup.Param_SpecularLightMap.SetValue(lightBuffer.Specular);
            _fxSetup.Param_VolumeLightMap.SetValue(lightBuffer.Volume);
        }
        public void SetSSAOMap(RenderTarget2D ssaoTarget)
        {
            _fxSetup.Param_SSAOMap.SetValue(ssaoTarget);
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }
    }

}
