using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.PostProcessing
{
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
    public partial class BloomFx : IDisposable
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


        //RenderTargets
        private RenderTarget2D _bloomRenderTarget2DMip0;
        private RenderTarget2D _bloomRenderTarget2DMip1;
        private RenderTarget2D _bloomRenderTarget2DMip2;
        private RenderTarget2D _bloomRenderTarget2DMip3;
        private RenderTarget2D _bloomRenderTarget2DMip4;
        private RenderTarget2D _bloomRenderTarget2DMip5;

        //Shader + variables
        private Effect Effect;

        private EffectPass Pass_Extract;
        private EffectPass Pass_ExtractLuminance;
        private EffectPass Pass_Downsample;
        private EffectPass Pass_Upsample;

        private EffectParameter Param_ScreenTexture;
        private EffectParameter Param_InverseResolution;
        private EffectParameter Param_Radius;
        private EffectParameter Param_Strength;
        private EffectParameter Param_StreakLength;
        private EffectParameter Param_Threshold;

        //resolution
        private Vector2 _resolution;
        //Preset variables for different mip levels
        private readonly float[] _radius = new float[5];
        private readonly float[] _strength = new float[5];

        private float _radiusMultiplier = 1.0f;


        public bool BloomUseLuminance = true;
        public int BloomDownsamplePasses = 5;

        //Objects
        private GraphicsDevice _graphicsDevice;
        private FullscreenTriangleBuffer _fullscreenTarget;


        #region properties
        public BloomPresets BloomPreset
        {
            get { return _bloomPreset; }
            set
            {
                if (_bloomPreset == value) return;

                _bloomPreset = value;
                SetBloomPreset(_bloomPreset);
            }
        }
        private BloomPresets _bloomPreset;


        private Texture2D BloomScreenTexture { set { Param_ScreenTexture.SetValue(value); } }
        private Vector2 BloomInverseResolution
        {
            get { return _bloomInverseResolutionField; }
            set
            {
                if (value != _bloomInverseResolutionField)
                {
                    _bloomInverseResolutionField = value;
                    Param_InverseResolution.SetValue(_bloomInverseResolutionField);
                }
            }
        }
        private Vector2 _bloomInverseResolutionField;
        private float BloomRadius { set { Param_Radius.SetValue(value * _radiusMultiplier); } }

        private float BloomStrength { set { Param_Strength.SetValue(value); } }
        public float BloomStreakLength { set { Param_StreakLength.SetValue(value); } }
        public float BloomThreshold { set { Param_Threshold.SetValue(value); } }

        #endregion



        public BloomFx(ContentManager content)
        {
            Load(content);
        }
        //Initialize graphicsDevice
        public void Initialize(GraphicsDevice graphicsDevice, Vector2 resolution)
        {
            _fullscreenTarget = FullscreenTriangleBuffer.Instance;

            _graphicsDevice = graphicsDevice;
            UpdateResolution(resolution);

        }

        /// <summary>
        /// Loads all needed components for the BloomEffect. This effect won't work without calling load
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            //Load the shader parameters and passes for cheap and easy access
            Effect = content.Load<Effect>("Shaders/BloomFilter/Bloom");
            Param_InverseResolution = Effect.Parameters["InverseResolution"];
            Param_Radius = Effect.Parameters["Radius"];
            Param_Strength = Effect.Parameters["Strength"];
            Param_StreakLength = Effect.Parameters["StreakLength"];
            Param_Threshold = Effect.Parameters["Threshold"];
            Param_ScreenTexture = Effect.Parameters["ScreenTexture"];

            Pass_Extract = Effect.Techniques["Extract"].Passes[0];
            Pass_ExtractLuminance = Effect.Techniques["ExtractLuminance"].Passes[0];
            Pass_Downsample = Effect.Techniques["Downsample"].Passes[0];
            Pass_Upsample = Effect.Techniques["Upsample"].Passes[0];

            //An interesting blendstate for merging the initial image with the bloom.
            //BlendStateBloom = new BlendState();
            //BlendStateBloom.ColorBlendFunction = BlendFunction.Add;
            //BlendStateBloom.ColorSourceBlend = Blend.BlendFactor;
            //BlendStateBloom.ColorDestinationBlend = Blend.BlendFactor;
            //BlendStateBloom.BlendFactor = new Color(0.5f, 0.5f, 0.5f);

            //Default threshold.
            BloomThreshold = 0.8f;
            //Setup the default preset values.
            BloomPreset = BloomPresets.SuperWide;
            SetBloomPreset(BloomPreset);

            BloomDownsamplePasses = 5;
        }

        /// <summary>
        /// A few presets with different values for the different mip levels of our bloom.
        /// </summary>
        /// <param name="preset">See BloomPresets enums. Example: BloomPresets.Wide</param>
        private void SetBloomPreset(BloomPresets preset)
        {
            switch (preset)
            {
                case BloomPresets.Wide:
                    {
                        Array.Copy(Wide_Strength, _strength, _strength.Length);
                        Array.Copy(Wide_Radius, _radius, _radius.Length);
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.SuperWide:
                    {
                        Array.Copy(SuperWide_Strength, _strength, _strength.Length);
                        Array.Copy(SuperWide_Radius, _radius, _radius.Length);
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Focussed:
                    {
                        Array.Copy(Focused_Strength, _strength, _strength.Length);
                        Array.Copy(Focused_Radius, _radius, _radius.Length);
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Small:
                    {
                        Array.Copy(Small_Strength, _strength, _strength.Length);
                        Array.Copy(Small_Radius, _radius, _radius.Length);
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Cheap:
                    {
                        Array.Copy(Cheap_Strength, _strength, _strength.Length);
                        Array.Copy(Cheap_Radius, _radius, _radius.Length);
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 2;
                        break;
                    }
            }
        }



        /// <summary>
        /// Main draw function
        /// </summary>
        /// <param name="inputTexture">the image from which we want to extract bright parts and blur these</param>
        /// <returns></returns>
        public Texture2D Draw(Texture2D inputTexture)
        {
            //Check if we are initialized
            if (_graphicsDevice == null)
                throw new Exception("Module not yet Loaded / Initialized. Use Load() first");

            ApplyGameSettings();

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;

            //EXTRACT  //Note: Is setRenderTargets(binding better?)
            //We extract the bright values which are above the Threshold and save them to Mip0
            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip0);

            BloomScreenTexture = inputTexture;
            BloomInverseResolution = Vector2.One / _resolution;

            if (BloomUseLuminance) Pass_ExtractLuminance.Apply();
            else Pass_Extract.Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            //Now downsample to the next lower mip texture
            if (BloomDownsamplePasses > 0)
            {
                BloomInverseResolution *= 2;
                //DOWNSAMPLE TO MIP1
                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip1);

                BloomScreenTexture = _bloomRenderTarget2DMip0;
                //Pass
                Pass_Downsample.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);

                if (BloomDownsamplePasses > 1)
                {
                    //Our input resolution is halfed, so our inverse 1/res. must be doubled
                    BloomInverseResolution *= 2;

                    //DOWNSAMPLE TO MIP2
                    _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip2);

                    BloomScreenTexture = _bloomRenderTarget2DMip1;
                    //Pass
                    Pass_Downsample.Apply();
                    _fullscreenTarget.Draw(_graphicsDevice);

                    if (BloomDownsamplePasses > 2)
                    {
                        BloomInverseResolution *= 2;

                        //DOWNSAMPLE TO MIP3
                        _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip3);

                        BloomScreenTexture = _bloomRenderTarget2DMip2;
                        //Pass
                        Pass_Downsample.Apply();
                        _fullscreenTarget.Draw(_graphicsDevice);

                        if (BloomDownsamplePasses > 3)
                        {
                            BloomInverseResolution *= 2;

                            //DOWNSAMPLE TO MIP4
                            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip4);

                            BloomScreenTexture = _bloomRenderTarget2DMip3;
                            //Pass
                            Pass_Downsample.Apply();
                            _fullscreenTarget.Draw(_graphicsDevice);

                            if (BloomDownsamplePasses > 4)
                            {
                                BloomInverseResolution *= 2;

                                //DOWNSAMPLE TO MIP5
                                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip5);

                                BloomScreenTexture = _bloomRenderTarget2DMip4;
                                //Pass
                                Pass_Downsample.Apply();
                                _fullscreenTarget.Draw(_graphicsDevice);

                                ChangeBlendState();

                                //UPSAMPLE TO MIP4
                                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip4);
                                BloomScreenTexture = _bloomRenderTarget2DMip5;

                                BloomStrength = _strength[4];
                                BloomRadius = _radius[4];
                                Pass_Upsample.Apply();
                                _fullscreenTarget.Draw(_graphicsDevice);

                                BloomInverseResolution /= 2;
                            }

                            ChangeBlendState();

                            //UPSAMPLE TO MIP3
                            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip3);
                            BloomScreenTexture = _bloomRenderTarget2DMip4;

                            BloomStrength = _strength[3];
                            BloomRadius = _radius[3];
                            Pass_Upsample.Apply();
                            _fullscreenTarget.Draw(_graphicsDevice);

                            BloomInverseResolution /= 2;

                        }

                        ChangeBlendState();

                        //UPSAMPLE TO MIP2
                        _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip2);
                        BloomScreenTexture = _bloomRenderTarget2DMip3;

                        BloomStrength = _strength[2];
                        BloomRadius = _radius[2];
                        Pass_Upsample.Apply();
                        _fullscreenTarget.Draw(_graphicsDevice);

                        BloomInverseResolution /= 2;

                    }

                    ChangeBlendState();

                    //UPSAMPLE TO MIP1
                    _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip1);
                    BloomScreenTexture = _bloomRenderTarget2DMip2;

                    BloomStrength = _strength[1];
                    BloomRadius = _radius[1];
                    Pass_Upsample.Apply();
                    _fullscreenTarget.Draw(_graphicsDevice);

                    BloomInverseResolution /= 2;
                }

                ChangeBlendState();

                //UPSAMPLE TO MIP0
                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip0);
                BloomScreenTexture = _bloomRenderTarget2DMip1;

                BloomStrength = _strength[0];
                BloomRadius = _radius[0];

                Pass_Upsample.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Note the final step could be done as a blend to the final texture.

            return _bloomRenderTarget2DMip0;
        }

        private void ApplyGameSettings()
        {
            Array.Copy(RenderingSettings.Bloom.Radius, _radius, _radius.Length);
            Array.Copy(RenderingSettings.Bloom.Strength, _strength, _strength.Length);

            BloomThreshold = RenderingSettings.Bloom.Threshold * 0.1f;
        }

        private void ChangeBlendState()
        {
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// Update the InverseResolution of the used rendertargets. This should be the InverseResolution of the processed image
        /// We use SurfaceFormat.Color, but you can use higher precision buffers obviously.
        /// </summary>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        public void UpdateResolution(Vector2 resolution)
        {
            _resolution = resolution;

            if (_bloomRenderTarget2DMip0 != null)
            {
                Dispose();
            }

            _bloomRenderTarget2DMip0 = new RenderTarget2D(_graphicsDevice,
                (int)(resolution.X),
                (int)(resolution.Y), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip1 = new RenderTarget2D(_graphicsDevice,
                (int)(resolution.X / 2),
                (int)(resolution.Y / 2), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip2 = new RenderTarget2D(_graphicsDevice,
                (int)(resolution.X / 4),
                (int)(resolution.Y / 4), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip3 = new RenderTarget2D(_graphicsDevice,
                (int)(resolution.X / 8),
                (int)(resolution.Y / 8), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip4 = new RenderTarget2D(_graphicsDevice,
                (int)(resolution.X / 16),
                (int)(resolution.Y / 16), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip5 = new RenderTarget2D(_graphicsDevice,
                (int)(resolution.X / 32),
                (int)(resolution.Y / 32), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        /// <summary>
        //Dispose our RenderTargets. This is not covered by the Garbage Collector so we have to do it manually
        /// </summary>
        public void Dispose()
        {
            _bloomRenderTarget2DMip0?.Dispose();
            _bloomRenderTarget2DMip1?.Dispose();
            _bloomRenderTarget2DMip2?.Dispose();
            _bloomRenderTarget2DMip3?.Dispose();
            _bloomRenderTarget2DMip4?.Dispose();
            _bloomRenderTarget2DMip5?.Dispose();
            _graphicsDevice?.Dispose();
            Effect?.Dispose();
        }
    }
}
