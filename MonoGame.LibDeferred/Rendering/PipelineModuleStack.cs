﻿using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Pipeline.Utilities;
using DeferredEngine.Rendering.SDF;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Rendering
{
    public class PipelineModuleStack : IDisposable
    {
        public readonly GBufferPipelineModule GBuffer;
        public readonly DeferredPipelineModule Deferred;
        public readonly ForwardPipelineModule Forward;
        public readonly ShadowMapPipelineModule ShadowMap;

        public readonly DirectionalLightPipelineModule DirectionalLight;
        public readonly PointLightPipelineModule PointLight;

        public readonly DepthReconstructPipelineModule DepthReconstruct;
        public readonly LightingPipelineModule Lighting;

        public readonly EnvironmentPipelineModule Environment;
        public readonly DistanceFieldRenderModule DistanceField;

        public readonly DecalRenderModule Decal;
        public readonly HelperRenderModule Helper;

        public readonly BillboardRenderModule Billboard;
        public readonly IdAndOutlineRenderModule IdAndOutline;


        public float FarClip
        {
            set
            {
                GBuffer.FarClip = value;
                Decal.FarClip = value;
                PointLight.FarClip = value;
                Billboard.FarClip = value;
                DepthReconstruct.FarClip = value;
            }
        }

        public PipelineModuleStack(ContentManager content)
        {
            GBuffer = new GBufferPipelineModule(content, "Shaders/GbufferSetup/GBuffer");
            Deferred = new DeferredPipelineModule(content, "Shaders/Deferred/DeferredCompose");
            Forward = new ForwardPipelineModule(content, "Shaders/forward/forward");
            ShadowMap = new ShadowMapPipelineModule(content, "Shaders/Shadow/ShadowMap");

            DirectionalLight = new DirectionalLightPipelineModule(content, "Shaders/Deferred/DeferredDirectionalLight");
            PointLight = new PointLightPipelineModule(content, "Shaders/Deferred/DeferredPointLight");
            DepthReconstruct = new DepthReconstructPipelineModule(content, "Shaders/ScreenSpace/ReconstructDepth");
            Lighting = new LightingPipelineModule(content) { PointLightRenderModule = PointLight, DirectionalLightRenderModule = DirectionalLight, DepthPipelineModule = DepthReconstruct };
            Environment = new EnvironmentPipelineModule(content, "Shaders/Deferred/DeferredEnvironmentMap");

            Decal = new DecalRenderModule();
            Helper = new HelperRenderModule();
            DistanceField = new DistanceFieldRenderModule() { EnvironmentProbeRenderModule = Environment, PointLightRenderModule = PointLight };

            Billboard = new BillboardRenderModule();
            IdAndOutline = new IdAndOutlineRenderModule();

            Billboard.IdAndOutlineRenderer = IdAndOutline;
            IdAndOutline.BillboardRenderer = Billboard;
        }
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            GBuffer.Initialize(graphicsDevice, spriteBatch);
            Deferred.Initialize(graphicsDevice, spriteBatch);
            Forward.Initialize(graphicsDevice, spriteBatch);
            ShadowMap.Initialize(graphicsDevice, spriteBatch);

            DirectionalLight.Initialize(graphicsDevice, spriteBatch);
            PointLight.Initialize(graphicsDevice, spriteBatch);
            DepthReconstruct.Initialize(graphicsDevice, spriteBatch);
            Lighting.Initialize(graphicsDevice, spriteBatch);
            Environment.Initialize(graphicsDevice, spriteBatch);

            Decal.Initialize(graphicsDevice);
            Helper.Initialize(graphicsDevice);
            DistanceField.Initialize(graphicsDevice, spriteBatch);

            Billboard.Initialize(graphicsDevice);
            IdAndOutline.Initialize(graphicsDevice);

        }

        public void SetGBufferParams(GBufferTarget gBufferTarget)
        {
            Billboard.DepthMap = gBufferTarget.Depth;

            PointLight.SetGBufferParams(gBufferTarget);
            DirectionalLight.SetGBufferParams(gBufferTarget);
            Environment.SetGBufferParams(gBufferTarget);

            Decal.DepthMap = gBufferTarget.Depth;
            DistanceField.DepthMap = gBufferTarget.Depth;
            DepthReconstruct.DepthMap = gBufferTarget.Depth;

            Deferred.SetGBufferParams(gBufferTarget);
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

