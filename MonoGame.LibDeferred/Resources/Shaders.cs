using DeferredEngine.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    public static partial class Shaders
    {
        //A static file which contains all shaders
        //Born out of need for quick thoughtless shader building
        //I am working on making seperate shading modules instead and will slowly shorten this one.


        //Depth Reconstruction
        public static class ReconstructDepth
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ReconstructDepth");

            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
        }


        // PostProcessing

        public static class PostProcssing
        {
            //Vignette and CA
            public static readonly Effect Effect = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
            public static readonly EffectParameter Param_ScreenTexture = Effect.Parameters["ScreenTexture"];
            public static readonly EffectParameter Param_ChromaticAbberationStrength = Effect.Parameters["ChromaticAbberationStrength"];
            public static readonly EffectParameter Param_SCurveStrength = Effect.Parameters["SCurveStrength"];
            public static readonly EffectParameter Param_WhitePoint = Effect.Parameters["WhitePoint"];
            public static readonly EffectParameter Param_PowExposure = Effect.Parameters["PowExposure"];

            public static readonly EffectTechnique Technique_VignetteChroma = Effect.Techniques["VignetteChroma"];
            public static readonly EffectTechnique Technique_Base = Effect.Techniques["Base"];
        }



        //ScreenSpaceReflection Effect
        public static class SSR
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceReflections");

            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static readonly EffectParameter Param_TargetMap = Effect.Parameters["TargetMap"];
            public static readonly EffectParameter Param_Resolution = Effect.Parameters["resolution"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_Time = Effect.Parameters["Time"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            public static readonly EffectParameter Param_FarClip = Effect.Parameters["FarClip"];
            public static readonly EffectParameter Param_NoiseMap = Effect.Parameters["NoiseMap"];

            public static readonly EffectTechnique Technique_Default = Effect.Techniques["Default"];
            public static readonly EffectTechnique Technique_Taa = Effect.Techniques["TAA"];
        }

        //Screen Space Ambient Occlusion Effect

        public static class SSAO
        {
            public static readonly Effect Effect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

            public static readonly EffectParameter Param_SSAOMap = Effect.Parameters["SSAOMap"];
            public static readonly EffectParameter Param_NormalMap = Effect.Parameters["NormalMap"];
            public static readonly EffectParameter Param_DepthMap = Effect.Parameters["DepthMap"];
            public static readonly EffectParameter Param_CameraPosition = Effect.Parameters["CameraPosition"];
            public static readonly EffectParameter Param_InverseViewProjection = Effect.Parameters["InverseViewProjection"];
            public static readonly EffectParameter Param_Projection = Effect.Parameters["Projection"];
            public static readonly EffectParameter Param_ViewProjection = Effect.Parameters["ViewProjection"];

            public static readonly EffectParameter Param_FalloffMin = Effect.Parameters["FalloffMin"];
            public static readonly EffectParameter Param_FalloffMax = Effect.Parameters["FalloffMax"];
            public static readonly EffectParameter Param_Samples = Effect.Parameters["Samples"];
            public static readonly EffectParameter Param_Strength = Effect.Parameters["Strength"];
            public static readonly EffectParameter Param_SampleRadius = Effect.Parameters["SampleRadius"];
            public static readonly EffectParameter Param_InverseResolution = Effect.Parameters["InverseResolution"];
            public static readonly EffectParameter Param_AspectRatio = Effect.Parameters["AspectRatio"];
            public static readonly EffectParameter Param_FrustumCorners = Effect.Parameters["FrustumCorners"];

            public static readonly EffectTechnique Technique_SSAO = Effect.Techniques["SSAO"];
            public static readonly EffectTechnique Technique_BlurHorizontal = Effect.Techniques["BilateralHorizontal"];
            public static readonly EffectTechnique Technique_BlurVertical = Effect.Techniques["BilateralVertical"];

            public static void SetCameraAndMatrices(Vector3 cameraPosition, PipelineMatrices matrices)
            {
                Shaders.SSAO.Param_InverseViewProjection.SetValue(matrices.InverseViewProjection);
                Shaders.SSAO.Param_Projection.SetValue(matrices.Projection);
                Shaders.SSAO.Param_ViewProjection.SetValue(matrices.ViewProjection);

                Shaders.SSAO.Param_CameraPosition.SetValue(cameraPosition);
            }

        }

    }

}
