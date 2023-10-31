using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using DeferredEngine.Rendering.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    public class DemoRenderingPipeline : DeferredRenderingPipeline
    {
        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            base.Initialize(graphicsDevice);

            RenderingSettings.Screen.g_FarClip.Changed += FarClip_OnChanged;
            RenderingSettings.Screen.g_FarClip.Set(512);
            SSReflectionFx.ModuleEnabled.Changed += SSR_Enabled_Changed;
            BloomFx.ModuleThreshold.Set(0.0f);
        }

        private void SSR_Enabled_Changed(bool enabled)
        {
            // clear SSReflection buffer if disabled/enabled
            if (!enabled)
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.SSR_Main);
                _graphicsDevice.Clear(new Color(0, 0, 0, 0.0f));
            }
        }
        private void FarClip_OnChanged(float farClip)
        {
            _frustum.FarClip = farClip;
        }
    }

}

