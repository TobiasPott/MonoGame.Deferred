using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.PostProcessing
{
    //Just a template
    public class TemporalAAFx : BaseFx
    {

        public Matrix CurrentViewToPreviousViewProjection;


        public TemporalAAFx(ContentManager content, string shaderPath = "Shaders/TemporalAntiAliasing/TemporalAntiAliasing")
        {
            Load(content, shaderPath);
        }

        public Vector3[] FrustumCorners { set { Shaders.TAA.Param_FrustumCorners.SetValue(value); } }
        public Vector2 Resolution { set { Shaders.TAA.Param_Resolution.SetValue(value); } }
        public RenderTarget2D DepthMap { set { Shaders.TAA.Param_DepthMap.SetValue(value); } }

        public bool UseTonemap { get { return Shaders.TAA.Param_UseTonemap.GetValueBoolean(); } set { Shaders.TAA.  Param_UseTonemap.SetValue(value); } }


        public void Load(ContentManager content, string shaderPath)
        {
        }


        public void Draw(RenderTarget2D currentFrame, RenderTarget2D previousFrames, RenderTarget2D output)
        {
            _graphicsDevice.SetRenderTarget(output);
            _graphicsDevice.BlendState = BlendState.Opaque;

            Shaders.TAA.Param_AccumulationMap.SetValue(previousFrames);
            Shaders.TAA.Param_UpdateMap.SetValue(currentFrame);
            Shaders.TAA.Param_CurrentToPrevious.SetValue(CurrentViewToPreviousViewProjection);

            this.Draw(Shaders.TAA.Pass_TemporalAA);

            if (UseTonemap)
            {
                _graphicsDevice.SetRenderTarget(currentFrame);
                Shaders.TAA.Param_UpdateMap.SetValue(output);
                this.Draw(Shaders.TAA.Pass_TonemapInverse);
            }
        }

        public override void Dispose()
        {
        }
    }
}

namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {
        public static class TAA
        {

            public static Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/TemporalAntiAliasing/TemporalAntiAliasing");

            public static EffectPass Pass_TemporalAA = Effect.Techniques["TemporalAntialiasing"].Passes[0];
            public static EffectPass Pass_TonemapInverse = Effect.Techniques["InverseTonemap"].Passes[0];

            public static EffectParameter Param_AccumulationMap = Effect.Parameters["AccumulationMap"];
            public static EffectParameter Param_UpdateMap = Effect.Parameters["UpdateMap"];
            public static EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static EffectParameter Param_CurrentToPrevious = Effect.Parameters["CurrentToPrevious"];
            public static EffectParameter Param_Resolution = Effect.Parameters["Resolution"];
            public static EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            public static EffectParameter Param_UseTonemap = Effect.Parameters["UseTonemap"];

        }
    }
}