using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.PostProcessing
{
    //Just a template
    public class TemporalAntialiasingFx : BaseFx
    {
        private Effect _taaShader;

        private EffectParameter _paramDepthMap;
        private EffectParameter _paramAccumulationMap;
        private EffectParameter _paramUpdateMap;
        private EffectParameter _paramCurrentToPrevious;
        private EffectParameter _paramResolution;
        private EffectParameter _paramFrustumCorners;
        private EffectParameter _paramUseTonemap;

        private Vector3[] _frustumCorners;
        private Vector2 _resolution;
        private bool _useTonemap;
        public Matrix CurrentViewToPreviousViewProjection;

        private RenderTarget2D _depthMap;
        private EffectPass _taaPass;
        private EffectPass _invTonemapPass;


        public TemporalAntialiasingFx(ContentManager content, string shaderPath = "Shaders/TemporalAntiAliasing/TemporalAntiAliasing")
        {
            Load(content, shaderPath);
        }


        public Vector3[] FrustumCorners
        {
            get { return _frustumCorners; }
            set
            {
                _frustumCorners = value;
                _paramFrustumCorners.SetValue(_frustumCorners);
            }
        }
        public Vector2 Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value;
                _paramResolution.SetValue(_resolution);
            }
        }
        public RenderTarget2D DepthMap
        {
            get { return _depthMap; }
            set
            {
                _depthMap = value;
                _paramDepthMap.SetValue(value);
            }
        }
        public bool UseTonemap
        {
            get { return _useTonemap; }
            set
            {
                if (value != _useTonemap)
                {
                    _useTonemap = value;
                    _paramUseTonemap.SetValue(value);
                }
            }
        }



        public override void Initialize(GraphicsDevice graphicsDevice, FullScreenTriangleBuffer fullScreenTriangle)
        {
            base.Initialize(graphicsDevice, fullScreenTriangle);
            _paramAccumulationMap = _taaShader.Parameters["AccumulationMap"];
            _paramUpdateMap = _taaShader.Parameters["UpdateMap"];
            _paramDepthMap = _taaShader.Parameters["DepthMap"];
            _paramCurrentToPrevious = _taaShader.Parameters["CurrentToPrevious"];
            _paramResolution = _taaShader.Parameters["Resolution"];
            _paramFrustumCorners = _taaShader.Parameters["FrustumCorners"];
            _paramUseTonemap = _taaShader.Parameters["UseTonemap"];

            _useTonemap = _paramUseTonemap.GetValueBoolean();

            _taaPass = _taaShader.Techniques["TemporalAntialiasing"].Passes[0];
            _invTonemapPass = _taaShader.Techniques["InverseTonemap"].Passes[0];
        }

        public void Load(ContentManager content, string shaderPath)
        {
            _taaShader = content.Load<Effect>(shaderPath);

        }


        public void Draw(RenderTarget2D currentFrame, RenderTarget2D previousFrames, RenderTarget2D output)
        {
            _graphicsDevice.SetRenderTarget(output);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _paramAccumulationMap.SetValue(previousFrames);
            _paramUpdateMap.SetValue(currentFrame);
            _paramCurrentToPrevious.SetValue(CurrentViewToPreviousViewProjection);

            this.Draw(_taaPass);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(currentFrame);
                _paramUpdateMap.SetValue(output);
                this.Draw(_invTonemapPass);
            }
        }

        public override void Dispose()
        {
            _taaShader?.Dispose();
            _depthMap?.Dispose();
        }
    }
}