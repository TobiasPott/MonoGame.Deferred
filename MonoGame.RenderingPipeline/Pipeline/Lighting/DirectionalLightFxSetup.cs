using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Pipeline.Lighting
{

    //Directional light
    public class DirectionalLightFxSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectTechnique Technique_Unshadowed { get; protected set; }
        public EffectTechnique Technique_SSShadowed { get; protected set; }
        public EffectTechnique Technique_Shadowed { get; protected set; }
        public EffectTechnique Technique_ShadowOnly { get; protected set; }

        public EffectParameter Param_ShadowMap { get; protected set; }
        public EffectParameter Param_SSShadowMap { get; protected set; }

        public EffectParameter Param_ViewProjection { get; protected set; }
        public EffectParameter Param_FrustumCorners { get; protected set; }
        public EffectParameter Param_CameraPosition { get; protected set; }
        public EffectParameter Param_InverseViewProjection { get; protected set; }
        public EffectParameter Param_LightViewProjection { get; protected set; }
        public EffectParameter Param_LightView { get; protected set; }
        public EffectParameter Param_LightFarClip { get; protected set; }

        public EffectParameter Param_LightColor { get; protected set; }
        public EffectParameter Param_LightIntensity { get; protected set; }
        public EffectParameter Param_LightDirection { get; protected set; }
        public EffectParameter Param_ShadowFiltering { get; protected set; }
        public EffectParameter Param_ShadowMapSize { get; protected set; }

        public EffectParameter Param_AlbedoMap { get; protected set; }
        public EffectParameter Param_NormalMap { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }


        public DirectionalLightFxSetup(string shaderPath = "Shaders/Deferred/DeferredDirectionalLight") : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Technique_Unshadowed = Effect.Techniques["Unshadowed"];
            Technique_SSShadowed = Effect.Techniques["SSShadowed"];
            Technique_Shadowed = Effect.Techniques["Shadowed"];
            Technique_ShadowOnly = Effect.Techniques["ShadowOnly"];

            Param_ViewProjection = Effect.Parameters["ViewProjection"];
            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_CameraPosition = Effect.Parameters["cameraPosition"];
            Param_InverseViewProjection = Effect.Parameters["InvertViewProjection"];
            Param_LightViewProjection = Effect.Parameters["LightViewProjection"];
            Param_LightView = Effect.Parameters["LightView"];
            Param_LightFarClip = Effect.Parameters["LightFarClip"];

            Param_LightColor = Effect.Parameters["LightColor"];
            Param_LightIntensity = Effect.Parameters["LightIntensity"];
            Param_LightDirection = Effect.Parameters["LightVector"];
            Param_ShadowFiltering = Effect.Parameters["ShadowFiltering"];
            Param_ShadowMapSize = Effect.Parameters["ShadowMapSize"];

            Param_AlbedoMap = Effect.Parameters[Names.Sampler("AlbedoMap")];
            Param_NormalMap = Effect.Parameters[Names.Sampler("NormalMap")];
            Param_DepthMap = Effect.Parameters[Names.Sampler("DepthMap")];

            Param_ShadowMap = Effect.Parameters[Names.Sampler("ShadowMap")];
            Param_SSShadowMap = Effect.Parameters[Names.Sampler("SSShadowMap")];
        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            Param_AlbedoMap.SetValue(gBufferTarget.Albedo);
            Param_NormalMap.SetValue(gBufferTarget.Normal);
            Param_DepthMap.SetValue(gBufferTarget.Depth);
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
