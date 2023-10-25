using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class SSReflectionFx : BaseFx
    {       
        // SSR
        public static bool g_SSReflection { get; set; } = true;
        public static bool g_SSReflection_FireflyReduction { get; set; } = true;



        public static float g_SSReflection_FireflyThreshold { get; set; } = 1.75f;


        public static bool g_SSReflectionNoise { get; set; } = true;
        public static bool g_SSReflectionTaa { get; set; } = true;

        //5 and 5 are good, 3 and 3 are cheap
        public static int g_SSReflections_Samples { get; set; } = 3;
        public static int g_SSReflections_RefinementSamples { get; set; } = 3;




        private SSReflectionFxSetup _fxSetup = new SSReflectionFxSetup();

        public PipelineMatrices Matrices { get; set; }

        public float Time { set { _fxSetup.Param_Time.SetValue(value); } }
        public float FarClip { set { _fxSetup.Param_FarClip.SetValue(value); } }

        public Vector3[] FrustumCorners { set { _fxSetup.Param_FrustumCorners.SetValue(value); } }
        public Vector2 Resolution { set { _fxSetup.Param_Resolution.SetValue(value); } }

        public RenderTarget2D DepthMap { set { _fxSetup.Param_DepthMap.SetValue(value); } }
        public RenderTarget2D NormalMap { set { _fxSetup.Param_NormalMap.SetValue(value); } }
        public RenderTarget2D TargetMap { set { _fxSetup.Param_TargetMap.SetValue(value); } }



        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public SSReflectionFx(ContentManager content)
        {
        }

        protected override bool GetEnabled() => _enabled && SSReflectionFx.g_SSReflection;
        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
                return sourceRT;

            //todo: more samples for more reflective materials!
            _graphicsDevice.SetRenderTarget(destRT);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            _fxSetup.Param_Projection.SetValue(this.Matrices.Projection);

            _fxSetup.Param_Samples.SetValue(SSReflectionFx.g_SSReflections_Samples);
            _fxSetup.Param_SecondarySamples.SetValue(SSReflectionFx.g_SSReflections_RefinementSamples);
            
            _fxSetup.Effect.CurrentTechnique = SSReflectionFx.g_SSReflectionTaa ? _fxSetup.Technique_Taa : _fxSetup.Technique_Default;
            _fxSetup.Effect.CurrentTechnique.Passes[0].Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            return destRT;
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
        }


    }
}
