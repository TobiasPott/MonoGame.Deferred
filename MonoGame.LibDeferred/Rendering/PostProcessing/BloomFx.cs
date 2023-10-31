using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    //enums
    public enum BloomPresets
    {
        Default,
        Wide,
        Focussed,
        Small,
        SuperWide,
        Cheap
    }


    /// <summary>
    /// Bloom / Blur, 2016 TheKosmonaut
    /// 
    /// High-Quality Bloom filter for high-performance applications
    /// 
    /// Based largely on the implementations in Unreal Engine 4 and Call of Duty AW
    /// For more information look for
    /// "Next Generation Post Processing in Call of Duty Advanced Warfare" by Jorge Jimenez
    /// http://www.iryoku.com/downloads/Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v18.pptx
    /// 
    /// The idea is to have several rendertargets or one rendertarget with several mip maps
    /// so each mip has half resolution (1/2 width and 1/2 height) of the previous one.
    /// 
    /// 32, 16, 8, 4, 2
    /// 
    /// In the first step we extract the bright spots from the original image. If not specified otherwise thsi happens in full resolution.
    /// We can do that based on the average RGB value or Luminance and check whether this value is higher than our Threshold.
    ///     BloomUseLuminance = true / false (default is true)
    ///     BloomThreshold = 0.8f;
    /// 
    /// Then we downscale this extraction layer to the next mip map.
    /// While doing that we sample several pixels around the origin.
    /// We continue to downsample a few more times, defined in
    ///     BloomDownsamplePasses = 5 ( default is 5)
    /// 
    /// Afterwards we upsample again, but blur in this step, too.
    /// The final output should be a blur with a very large kernel and smooth gradient.
    /// 
    /// The output in the draw is only the blurred extracted texture. 
    /// It can be drawn on top of / merged with the original image with an additive operation for example.
    /// 
    /// If you use ToneMapping you should apply Bloom before that step.
    /// </summary>
    /// 
    public partial class BloomFx : PostFx
    {
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);
        public readonly static NotifiedProperty<float> ModuleThreshold = new NotifiedProperty<float>(-1.0f);


        protected override bool GetEnabled() => _enabled && ModuleEnabled;



        //Preset variables for different mip levels
        private readonly float[] _radius = new float[5];
        private readonly float[] _strength = new float[5];

        private readonly float _radiusMultiplier = 1.0f;

        public bool BloomUseLuminance = true;
        public int BloomDownsamplePasses = 5;

        //Objects
        private readonly BloomFxSetup _fxSetup = new BloomFxSetup();
        //RenderTargets
        private DynamicMultiRenderTarget _mipMaps;
        private RenderTarget2D _swapBuffer;


        private Texture2D BloomScreenTexture { set { _fxSetup.Param_ScreenTexture.SetValue(value); } }

        private Vector2 InverseResolution { set { _fxSetup.Param_InverseResolution.SetValue(value); } }

        private float StreakLength { set { _fxSetup.Param_StreakLength.SetValue(value); } }
        private float Threshold { set { _fxSetup.Param_Threshold.SetValue(value); } }


        public BloomFx()
        {
            //An interesting blendstate for merging the initial image with the bloom.
            //BlendStateBloom = new BlendState();
            //BlendStateBloom.ColorBlendFunction = BlendFunction.Add;
            //BlendStateBloom.ColorSourceBlend = Blend.BlendFactor;
            //BlendStateBloom.ColorDestinationBlend = Blend.BlendFactor;
            //BlendStateBloom.BlendFactor = new Color(0.5f, 0.5f, 0.5f);

            // ToDo: Bloom Threshold by UI seems broken (check if manual changing works)
            //          Threshold is only set at BloomFx() and should recieve a notified property for runtime notification
            //Setup the default preset values.
            SetBloomPreset(BloomPresets.Default);

            ModuleThreshold.Changed += Bloom_Threshold_Changed;
        }

        private void Bloom_Threshold_Changed(float threshold)
        {
            Threshold = threshold;
        }
        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FullscreenTriangleBuffer fullscreenTarget)
        {
            base.Initialize(graphicsDevice, spriteBatch, fullscreenTarget);

            _resolution = RenderingSettings.Screen.g_Resolution;

            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
            _graphicsDevice = graphicsDevice;

            _mipMaps = new DynamicMultiRenderTarget(_graphicsDevice, (int)_resolution.X, (int)_resolution.Y, BloomFxPresetsData.Mips);
            _swapBuffer = RenderTarget2DDefinition.SSFx_Bloom.CreateRenderTarget(_graphicsDevice, _resolution);
        }

        /// <summary>
        /// A few presets with different values for the different mip levels of our bloom.
        /// </summary>
        /// <param name="preset">See BloomPresets enums. Example: BloomPresets.Wide</param>
        private void SetBloomPreset(BloomPresets preset)
        {
            if (preset == BloomPresets.Cheap)
            {
                StreakLength = 1;
                BloomDownsamplePasses = 2;
            }
            else
            {
                StreakLength = 1;
                BloomDownsamplePasses = 5;
            }
            BloomFxPresetsData.CopyPreset(preset, _strength, _radius);

        }



        private void SetPassAndDraw(bool downsample, int targetMip, int sourceMip, float strength, float radius)
        {
            _fxSetup.Param_Radius.SetValue(radius * _radiusMultiplier);
            _fxSetup.Param_Strength.SetValue(strength);

            this.SetPassAndDraw(downsample, targetMip, sourceMip);
        }
        private void SetPassAndDraw(bool downsample, int targetMip, int sourceMip)
        {
            _graphicsDevice.SetRenderTarget(_mipMaps[targetMip]);
            BloomScreenTexture = _mipMaps[sourceMip];

            if (downsample)
                _fxSetup.Pass_Downsample.Apply();
            else
                _fxSetup.Pass_Upsample.Apply();
            _fullscreenTarget.Draw(_graphicsDevice);
        }



        /// <summary>
        /// Main draw function
        /// </summary>
        /// <param name="sourceRT">the image from which we want to extract bright parts and blur these</param>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
                return destRT;

            //EXTRACT  //Note: Is setRenderTargets(binding better?)
            //We extract the bright values which are above the Threshold and save them to Mip0
            _graphicsDevice.SetStates(DepthStencilStateOption.KeepState, RasterizerStateOption.CullNone, BlendStateOption.Opaque);
            _graphicsDevice.SetRenderTarget(_mipMaps[0]);

            BloomScreenTexture = destRT;
            Vector2 inverseResolution = Vector2.One / _resolution;

            if (BloomUseLuminance)
                _fxSetup.Pass_ExtractLuminance.Apply();
            else _fxSetup.Pass_Extract.Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            #region Run 1-5 Passes
            //Now downsample to the next lower mip texture
            if (BloomDownsamplePasses > 0)
            {
                this.InverseResolution = inverseResolution *= 2;
                //DOWNSAMPLE TO MIP1
                this.SetPassAndDraw(true, 1, 0);

                if (BloomDownsamplePasses > 1)
                {
                    //Our input resolution is halfed, so our inverse 1/res. must be doubled
                    this.InverseResolution = inverseResolution *= 2;

                    //DOWNSAMPLE TO MIP2
                    this.SetPassAndDraw(true, 2, 1);

                    if (BloomDownsamplePasses > 2)
                    {
                        this.InverseResolution = inverseResolution *= 2;

                        //DOWNSAMPLE TO MIP3
                        this.SetPassAndDraw(true, 3, 2);

                        if (BloomDownsamplePasses > 3)
                        {
                            this.InverseResolution = inverseResolution *= 2;

                            //DOWNSAMPLE TO MIP4
                            this.SetPassAndDraw(true, 4, 3);

                            if (BloomDownsamplePasses > 4)
                            {
                                this.InverseResolution = inverseResolution *= 2;

                                //DOWNSAMPLE TO MIP5
                                this.SetPassAndDraw(true, 5, 4);

                                ChangeBlendState();
                                //UPSAMPLE TO MIP4
                                this.SetPassAndDraw(false, 4, 5, _strength[4], _radius[4]);
                                this.InverseResolution = inverseResolution /= 2;
                            }

                            ChangeBlendState();
                            //UPSAMPLE TO MIP3
                            this.SetPassAndDraw(false, 3, 4, _strength[3], _radius[3]);
                            this.InverseResolution = inverseResolution /= 2;
                        }

                        ChangeBlendState();
                        //UPSAMPLE TO MIP2
                        this.SetPassAndDraw(false, 2, 3, _strength[2], _radius[2]);
                        this.InverseResolution = inverseResolution /= 2;

                    }
                    ChangeBlendState();
                    //UPSAMPLE TO MIP1
                    this.SetPassAndDraw(false, 1, 2, _strength[1], _radius[1]);
                    this.InverseResolution = inverseResolution /= 2;
                }

                ChangeBlendState();
                ////UPSAMPLE TO MIP0
                this.SetPassAndDraw(false, 0, 1, _strength[0], _radius[0]);

                // ToDo: Swap use of render textures to return the sourceRT (is COMPOSE at the moment)
                _graphicsDevice.SetRenderTargets(_swapBuffer);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap);

                _spriteBatch.Draw(destRT, RenderingSettings.Screen.Rect, Color.White);
                _spriteBatch.Draw(_mipMaps[0], RenderingSettings.Screen.Rect, Color.White);

                _spriteBatch.End();
            }
            #endregion

            this.Blit(_swapBuffer, destRT);
            //Note the final step could be done as a blend to the final texture.
            // sample profiler if set
            this.Profiler?.SampleTimestamp(TimestampIndices.Draw_Bloom);

            return destRT;
        }


        private void ChangeBlendState()
        {
            _graphicsDevice.SetState(BlendStateOption.AlphaBlend);
        }


        /// <summary>
        //Dispose our RenderTargets. This is not covered by the Garbage Collector so we have to do it manually
        /// </summary>
        public override void Dispose()
        {
            _mipMaps?.Dispose();
            _fxSetup?.Dispose();
        }
    }


    public class BloomFxPresetsData
    {
        private static readonly float[] Wide_Strength = new float[] { 0.5f, 1, 2, 1, 2 };
        private static readonly float[] Wide_Radius = new float[] { 1.0f, 2.0f, 2.0f, 4.0f, 4.0f };

        private static readonly float[] SuperWide_Strength = new float[] { 0.9f, 1, 1, 2, 6 };
        private static readonly float[] SuperWide_Radius = new float[] { 2.0f, 2.0f, 2.0f, 2.0f, 4.0f };

        private static readonly float[] Focused_Strength = new float[] { 0.9f, 1, 1, 1, 2 };
        private static readonly float[] Focused_Radius = new float[] { 2.0f, 2.0f, 2.0f, 2.0f, 4.0f };

        private static readonly float[] Small_Strength = new float[] { 0.8f, 1, 1, 1, 1 };
        private static readonly float[] Small_Radius = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

        private static readonly float[] Cheap_Strength = new float[] { 0.8f, 2, 0, 0, 0 };
        private static readonly float[] Cheap_Radius = new float[] { 2.0f, 2.0f, 0, 0, 0 };

        private static readonly float[] Default_Strength = new float[] { 0.5f, 1.0f, 1.0f, 1.0f, 1.0f };
        private static readonly float[] Default_Radius = new float[] { 1.0f, 1.0f, 2.0f, 3.0f, 4.0f };


        private static readonly RenderTarget2DDefinition Mip0 = new RenderTarget2DDefinition(nameof(Mip0), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        private static readonly RenderTarget2DDefinition Mip1 = new RenderTarget2DDefinition(nameof(Mip1), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x1);
        private static readonly RenderTarget2DDefinition Mip2 = new RenderTarget2DDefinition(nameof(Mip2), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x2);
        private static readonly RenderTarget2DDefinition Mip3 = new RenderTarget2DDefinition(nameof(Mip3), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x3);
        private static readonly RenderTarget2DDefinition Mip4 = new RenderTarget2DDefinition(nameof(Mip4), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x4);
        private static readonly RenderTarget2DDefinition Mip5 = new RenderTarget2DDefinition(nameof(Mip5), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x5);

        internal static readonly RenderTarget2DDefinition[] Mips = new[] { Mip0, Mip1, Mip2, Mip3, Mip4, Mip5 };


        /// <summary>
        /// A few presets with different values for the different mip levels of our bloom.
        /// </summary>
        /// <param name="preset">See BloomPresets enums. Example: BloomPresets.Wide</param>
        public static void CopyPreset(BloomPresets preset, float[] targetStrength, float[] targetRadius)
        {
            switch (preset)
            {
                case BloomPresets.Wide:
                    {
                        Array.Copy(BloomFxPresetsData.Wide_Strength, targetStrength, targetStrength.Length);
                        Array.Copy(BloomFxPresetsData.Wide_Radius, targetRadius, targetRadius.Length);
                        break;
                    }
                case BloomPresets.SuperWide:
                    {
                        Array.Copy(BloomFxPresetsData.SuperWide_Strength, targetStrength, targetStrength.Length);
                        Array.Copy(BloomFxPresetsData.SuperWide_Radius, targetRadius, targetRadius.Length);
                        break;
                    }
                case BloomPresets.Focussed:
                    {
                        Array.Copy(BloomFxPresetsData.Focused_Strength, targetStrength, targetStrength.Length);
                        Array.Copy(BloomFxPresetsData.Focused_Radius, targetRadius, targetRadius.Length);
                        break;
                    }
                case BloomPresets.Small:
                    {
                        Array.Copy(BloomFxPresetsData.Small_Strength, targetStrength, targetStrength.Length);
                        Array.Copy(BloomFxPresetsData.Small_Radius, targetRadius, targetRadius.Length);
                        break;
                    }
                case BloomPresets.Cheap:
                    {
                        Array.Copy(BloomFxPresetsData.Cheap_Strength, targetStrength, targetStrength.Length);
                        Array.Copy(BloomFxPresetsData.Cheap_Radius, targetRadius, targetRadius.Length);
                        break;
                    }
                case BloomPresets.Default:
                    {
                        Array.Copy(BloomFxPresetsData.Default_Strength, targetStrength, targetStrength.Length);
                        Array.Copy(BloomFxPresetsData.Default_Radius, targetRadius, targetRadius.Length);
                        break;
                    }
            }
        }

    }

}
