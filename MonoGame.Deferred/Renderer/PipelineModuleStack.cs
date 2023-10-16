using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.PostProcessing;
using DeferredEngine.Renderer.RenderModules;
using DeferredEngine.Renderer.RenderModules.SDF;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public class PipelineModuleStack : IDisposable
    {
        public readonly GBufferPipelineModule GBuffer;
        public readonly DeferredPipelineModule Deferred;
        public readonly ForwardPipelineModule Forward;
        public readonly ShadowMapPipelineModule ShadowMap;

        public readonly DirectionalLightPipelineModule DirectionalLight;
        public readonly PointLightPipelineModule PointLight;
        public readonly LightingPipelineModule Lighting;

        public readonly EnvironmentPipelineModule Environment;
        public readonly DistanceFieldRenderModule DistanceField;

        public readonly DecalRenderModule Decal;
        public readonly HelperRenderModule Helper;

        public PipelineModuleStack(ContentManager content)
        {
            GBuffer = new GBufferPipelineModule(content, "Shaders/GbufferSetup/GBuffer");
            Deferred = new DeferredPipelineModule(content, "Shaders/Deferred/DeferredCompose");
            Forward = new ForwardPipelineModule(content, "Shaders/forward/forward");
            ShadowMap = new ShadowMapPipelineModule(content, "Shaders/Shadow/ShadowMap");

            DirectionalLight = new DirectionalLightPipelineModule(content, "Shaders/Deferred/DeferredDirectionalLight");
            PointLight = new PointLightPipelineModule(content, "Shaders/Deferred/DeferredPointLight");
            Lighting = new LightingPipelineModule(content) { PointLightRenderModule = PointLight, DirectionalLightRenderModule = DirectionalLight };
            Environment = new EnvironmentPipelineModule(content, "Shaders/Deferred/DeferredEnvironmentMap");

            Decal = new DecalRenderModule();
            Helper = new HelperRenderModule();
            DistanceField = new DistanceFieldRenderModule() { EnvironmentProbeRenderModule = Environment, PointLightRenderModule = PointLight };

        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            GBuffer.Initialize(graphicsDevice, spriteBatch);
            Deferred.Initialize(graphicsDevice, spriteBatch);
            Forward.Initialize(graphicsDevice, spriteBatch);
            ShadowMap.Initialize(graphicsDevice, spriteBatch);

            DirectionalLight.Initialize(graphicsDevice, spriteBatch);
            PointLight.Initialize(graphicsDevice, spriteBatch);
            Lighting.Initialize(graphicsDevice, spriteBatch);
            Environment.Initialize(graphicsDevice, spriteBatch);

            Decal.Initialize(graphicsDevice);
            Helper.Initialize(graphicsDevice);
            DistanceField.Initialize(graphicsDevice, spriteBatch);

        }


        public void Dispose()
        {
            GBuffer?.Dispose();
            Deferred?.Dispose();
            Forward?.Dispose();
            ShadowMap?.Dispose();

            DirectionalLight?.Dispose();
            PointLight?.Dispose();
            Lighting?.Dispose();
            Environment?.Dispose();

            Decal?.Dispose();
            Helper?.Dispose();
            DistanceField?.Dispose();

        }

    }

}

