﻿using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Pipeline.Utilities;
using DeferredEngine.Rendering.SDF;
using Microsoft.Xna.Framework;
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



        public PipelineProfiler Profiler
        { set { foreach (PipelineModule module in _modules) module.Profiler = value; } }
        public PipelineMatrices Matrices
        { set { foreach (PipelineModule module in _modules) module.Matrices = value; } }
        public PipelineFrustum Frustum
        { set { foreach (PipelineModule module in _modules) module.Frustum = value; } }


        public GBufferTarget GBufferTarget
        {
            set
            {
                GBuffer.GBufferTarget = value;
                Billboard.DepthMap = value.Depth;

                PointLight.SetGBufferParams(value);
                DirectionalLight.SetGBufferParams(value);
                Environment.SetGBufferParams(value);

                Decal.DepthMap = value.Depth;
                DistanceField.DepthMap = value.Depth;
                DepthReconstruct.DepthMap = value.Depth;

                Deferred.SetGBufferParams(value);
            }
        }
        public SSFxTargets SSFxTargets
        {
            set
            {
                DirectionalLight.SetScreenSpaceShadowMap(value.AO_Blur_Final);
                Environment.SSRMap = value.SSR_Main;
                Deferred.SetSSAOMap(value.AO_Blur_Final);
            }
        }
        public LightingBufferTarget LightingBufferTarget
        {
            set
            {
                Lighting.LightingBufferTarget = value;
                Deferred.SetLightingParams(value);
            }
        }

        public Vector2 Resolution
        {
            set
            {
                PointLight.Resolution = value;
                Environment.Resolution = value;

                Billboard.AspectRatio = value.X / value.Y;
                IdAndOutline.SetUpRenderTarget(value);
            }
        }


        private List<PipelineModule> _modules = new List<PipelineModule>();

        public PipelineModuleStack()
        {
            DepthReconstruct = new DepthReconstructPipelineModule();
            GBuffer = new GBufferPipelineModule();
            Deferred = new DeferredPipelineModule();
            Forward = new ForwardPipelineModule() { DepthReconstruct = DepthReconstruct };
            ShadowMap = new ShadowMapPipelineModule();

            DirectionalLight = new DirectionalLightPipelineModule();
            PointLight = new PointLightPipelineModule();
            Lighting = new LightingPipelineModule() { PointLightRenderModule = PointLight, DirectionalLightRenderModule = DirectionalLight, DepthPipelineModule = DepthReconstruct };
            Environment = new EnvironmentPipelineModule();

            Decal = new DecalRenderModule();
            Helper = new HelperRenderModule();
            DistanceField = new DistanceFieldRenderModule() { EnvironmentProbeRenderModule = Environment, PointLightRenderModule = PointLight };

            Billboard = new BillboardRenderModule();
            IdAndOutline = new IdAndOutlineRenderModule();

            Billboard.IdAndOutlineRenderer = IdAndOutline;
            IdAndOutline.BillboardRenderer = Billboard;

            _modules.AddRange(new PipelineModule[] {
                GBuffer, Deferred, Forward, ShadowMap, DirectionalLight, PointLight, DepthReconstruct,
                Lighting, Environment, Decal, Helper, DistanceField, Billboard, IdAndOutline
            });
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

            Decal.Initialize(graphicsDevice, spriteBatch);
            Helper.Initialize(graphicsDevice, spriteBatch);
            DistanceField.Initialize(graphicsDevice, spriteBatch);

            Billboard.Initialize(graphicsDevice, spriteBatch);
            IdAndOutline.Initialize(graphicsDevice, spriteBatch);

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

