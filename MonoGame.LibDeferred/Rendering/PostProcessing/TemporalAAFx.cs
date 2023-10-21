using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Renderer.PostProcessing
{
    //Just a template
    public class TemporalAAFx : BaseFx
    {

        private TemporalAAFxEffectSetup _effectSetup = new TemporalAAFxEffectSetup();
        private bool _enabled = true;
        private bool _useTonemapping = true;
        private HaltonSequence _haltonSequence = new HaltonSequence();


        public bool Enabled { get => _enabled && RenderingSettings.TAA.Enabled; set { _enabled = value; } }
        public PipelineMatrices Matrices { get; set; }
        public bool IsOffFrame { get; protected set; } = true;
        public int JitterMode = 2;

        public Vector3[] FrustumCorners { set { _effectSetup.Param_FrustumCorners.SetValue(value); } }
        public Vector2 Resolution { set { _effectSetup.Param_Resolution.SetValue(value); } }
        public RenderTarget2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }

        public bool UseTonemap
        {
            get { return _useTonemapping && RenderingSettings.TAA.UseTonemapping; }
            set { _useTonemapping = value; _effectSetup.Param_UseTonemap.SetValue(value); }
        }


        public TemporalAAFx()
        { }

        public void SwapOffFrame()
        { IsOffFrame = !IsOffFrame; }

        public void Draw(RenderTarget2D currentFrame, RenderTarget2D lastFrame, RenderTarget2D output)
        {
            _graphicsDevice.SetRenderTarget(output);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _effectSetup.Param_UpdateMap.SetValue(currentFrame);
            _effectSetup.Param_AccumulationMap.SetValue(lastFrame);
            _effectSetup.Param_CurrentToPrevious.SetValue(Matrices.CurrentViewToPreviousViewProjection);

            this.Draw(_effectSetup.Pass_TemporalAA);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(currentFrame);
                _effectSetup.Param_UpdateMap.SetValue(output);
                this.Draw(_effectSetup.Pass_TonemapInverse);
            }
        }

        public bool UpdateViewProjection(PipelineMatrices matrices)
        {
            return matrices.ApplyViewProjectionJitter(this.JitterMode, this.IsOffFrame, _haltonSequence);
            //switch (this.JitterMode)
            //{
            //    case 0: //2 frames, just basic translation. Worst taa implementation. Not good with the continous integration used
            //        {
            //            Vector2 translation = Vector2.One * (this.IsOffFrame ? 0.5f : -0.5f);
            //            matrices.ViewProjection *= (translation / RenderingSettings.g_ScreenResolution).ToMatrixTranslationXY();
            //            return true;
            //        }
            //    case 1: // Just random translation
            //        {
            //            float randomAngle = FastRand.NextAngle();
            //            Vector2 translation = (new Vector2((float)Math.Sin(randomAngle), (float)Math.Cos(randomAngle)) / RenderingSettings.g_ScreenResolution) * 0.5f;
            //            matrices.ViewProjection *= translation.ToMatrixTranslationXY();
            //            return true;
            //        }
            //    case 2: // Halton sequence, default
            //        {
            //            Vector3 translation = _haltonSequence.GetNext();
            //            matrices.ViewProjection *= Matrix.CreateTranslation(translation);
            //            return true;
            //        }
            //}
            //return false;
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }
    }
}
