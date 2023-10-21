using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Content;
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

        private DeferredEffectSetup _effectSetup = new DeferredEffectSetup();
        private FullscreenTriangleBuffer _fullscreenTarget;
        private DeferredColorSpace _colorSpace = DeferredColorSpace.Linear;

        public bool UseSSAOMap
        {
            get { return _effectSetup.Param_UseSSAO.GetValueBoolean(); }
            set { _effectSetup.Param_UseSSAO.SetValue(value); }
        }
        public DeferredColorSpace ColorSpace
        {
            get { return _colorSpace; }
            set
            {
                _colorSpace = value;
                _effectSetup.Effect_Compose.CurrentTechnique = value == DeferredColorSpace.Linear ? _effectSetup.Technique_Linear : _effectSetup.Technique_NonLinear;
            }
        }


        public DeferredPipelineModule(ContentManager content, string shaderPath)
            : base(content, shaderPath)
        {
            Load(content, shaderPath);
        }

        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
            _effectSetup.Param_UseSSAO.SetValue(true);
            _effectSetup.Effect_Compose.CurrentTechnique = _colorSpace == DeferredColorSpace.Linear ? _effectSetup.Technique_Linear : _effectSetup.Technique_NonLinear;
        }

        protected override void Load(ContentManager content, string shaderPath = "Shaders/Deferred/DeferredCompose")
        { }



        /// <summary>
        /// Compose the render by combining the albedo channel with the light channels
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D destination)
        {
            // ToDo: Move 'Compose' to DeferredPipelineModule and pass target buffer as RenderTarget2D parameter
            _graphicsDevice.SetRenderTarget(destination);
            _graphicsDevice.SetStates(DepthStencilStateOption.KeepState, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            //combine!
            _effectSetup.Effect_Compose.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            return destination;
        }


        public void SetGBufferParams(GBufferTarget gBuffer)
        {
            _effectSetup.Param_ColorMap.SetValue(gBuffer.Albedo);
            _effectSetup.Param_NormalMap.SetValue(gBuffer.Normal);
        }
        public void SetLightingParams(LightingBufferTarget lightBuffer)
        {
            _effectSetup.Param_DiffuseLightMap.SetValue(lightBuffer.Diffuse);
            _effectSetup.Param_SpecularLightMap.SetValue(lightBuffer.Specular);
            _effectSetup.Param_VolumeLightMap.SetValue(lightBuffer.Volume);
        }
        public void SetSSAOMap(RenderTarget2D ssaoTarget)
        {
            _effectSetup.Param_SSAOMap.SetValue(ssaoTarget);
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }
    }

}
