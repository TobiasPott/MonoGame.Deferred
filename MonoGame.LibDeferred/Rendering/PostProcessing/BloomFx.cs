using DeferredEngine.Recources;
using DeferredEngine.Rendering.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
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


        private static readonly RenderTarget2DDefinition Mip0_Definition = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        private static readonly RenderTarget2DDefinition Mip1_Definition = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x1);
        private static readonly RenderTarget2DDefinition Mip2_Definition = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x2);
        private static readonly RenderTarget2DDefinition Mip3_Definition = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x3);
        private static readonly RenderTarget2DDefinition Mip4_Definition = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x4);
        private static readonly RenderTarget2DDefinition Mip5_Definition = new RenderTarget2DDefinition(false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents, ResamplingModes.Downsample_x5);


        //RenderTargets
        private DynamicMultiRenderTarget _mipMaps;


        private bool _enabled = true;
        private Vector2 _resolution;
        //Preset variables for different mip levels
        private readonly float[] _radius = new float[5];
        private readonly float[] _strength = new float[5];


        private float _radiusMultiplier = 1.0f;


        public bool BloomUseLuminance = true;
        public int BloomDownsamplePasses = 5;

        //Objects
        private BloomFxEffectSetup _effectSetup = new BloomFxEffectSetup();
        private GraphicsDevice _graphicsDevice;
        private FullscreenTriangleBuffer _fullscreenTarget;


        public bool Enabled { get => _enabled && RenderingSettings.Bloom.Enabled; set { _enabled = value; } }
        public BloomPresets BloomPreset { set { SetBloomPreset(value); } }


        private Texture2D BloomScreenTexture { set { _effectSetup.Param_ScreenTexture.SetValue(value); } }

        private Vector2 _bloomInverseResolution;
        private Vector2 BloomInverseResolution
        {
            get { return _bloomInverseResolution; }
            set
            {
                if (value != _bloomInverseResolution)
                {
                    _bloomInverseResolution = value;
                    _effectSetup.Param_InverseResolution.SetValue(_bloomInverseResolution);
                }
            }
        }
        private float BloomRadius { set { _effectSetup.Param_Radius.SetValue(value * _radiusMultiplier); } }

        private float BloomStrength { set { _effectSetup.Param_Strength.SetValue(value); } }
        public float BloomStreakLength { set { _effectSetup.Param_StreakLength.SetValue(value); } }
        public float BloomThreshold { set { _effectSetup.Param_Threshold.SetValue(value); } }




        public BloomFx(ContentManager content)
        {
            Load(content);
        }
        //Initialize graphicsDevice
        public void Initialize(GraphicsDevice graphicsDevice, Vector2 resolution)
        {
            _resolution = resolution;

            _fullscreenTarget = FullscreenTriangleBuffer.Instance;
            _graphicsDevice = graphicsDevice;

            _mipMaps = new DynamicMultiRenderTarget(_graphicsDevice, (int)resolution.X, (int)resolution.Y, new[] { Mip0_Definition, Mip1_Definition, Mip2_Definition, Mip3_Definition, Mip4_Definition, Mip5_Definition });
        }

        /// <summary>
        /// Loads all needed components for the BloomEffect. This effect won't work without calling load
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            //An interesting blendstate for merging the initial image with the bloom.
            //BlendStateBloom = new BlendState();
            //BlendStateBloom.ColorBlendFunction = BlendFunction.Add;
            //BlendStateBloom.ColorSourceBlend = Blend.BlendFactor;
            //BlendStateBloom.ColorDestinationBlend = Blend.BlendFactor;
            //BlendStateBloom.BlendFactor = new Color(0.5f, 0.5f, 0.5f);

            //Default threshold.
            BloomThreshold = 0.8f;
            //Setup the default preset values.
            SetBloomPreset(BloomPresets.SuperWide);

            ApplyGameSettings();
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
        public RenderTarget2D Draw(Texture2D inputTexture)
        {
            //Check if we are initialized
            if (_graphicsDevice == null)
                throw new Exception("Module not yet Loaded / Initialized. Use Load() first");

            //EXTRACT  //Note: Is setRenderTargets(binding better?)
            //We extract the bright values which are above the Threshold and save them to Mip0
            _graphicsDevice.SetStates(DepthStencilStateOption.KeepState, RasterizerStateOption.CullNone, BlendStateOption.Opaque);
            _graphicsDevice.SetRenderTarget(_mipMaps[0]);

            BloomScreenTexture = inputTexture;
            BloomInverseResolution = Vector2.One / _resolution;

            if (BloomUseLuminance) _effectSetup.Pass_ExtractLuminance.Apply();
            else _effectSetup.Pass_Extract.Apply();
            _fullscreenTarget.Draw(_graphicsDevice);

            //Now downsample to the next lower mip texture
            if (BloomDownsamplePasses > 0)
            {
                BloomInverseResolution *= 2;
                //DOWNSAMPLE TO MIP1
                _graphicsDevice.SetRenderTarget(_mipMaps[1]);

                BloomScreenTexture = _mipMaps[0];
                //Pass
                _effectSetup.Pass_Downsample.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);

                if (BloomDownsamplePasses > 1)
                {
                    //Our input resolution is halfed, so our inverse 1/res. must be doubled
                    BloomInverseResolution *= 2;

                    //DOWNSAMPLE TO MIP2
                    _graphicsDevice.SetRenderTarget(_mipMaps[2]);

                    BloomScreenTexture = _mipMaps[1];
                    //Pass
                    _effectSetup.Pass_Downsample.Apply();
                    _fullscreenTarget.Draw(_graphicsDevice);

                    if (BloomDownsamplePasses > 2)
                    {
                        BloomInverseResolution *= 2;

                        //DOWNSAMPLE TO MIP3
                        _graphicsDevice.SetRenderTarget(_mipMaps[3]);

                        BloomScreenTexture = _mipMaps[2];
                        //Pass
                        _effectSetup.Pass_Downsample.Apply();
                        _fullscreenTarget.Draw(_graphicsDevice);

                        if (BloomDownsamplePasses > 3)
                        {
                            BloomInverseResolution *= 2;

                            //DOWNSAMPLE TO MIP4
                            _graphicsDevice.SetRenderTarget(_mipMaps[4]);

                            BloomScreenTexture = _mipMaps[3];
                            //Pass
                            _effectSetup.Pass_Downsample.Apply();
                            _fullscreenTarget.Draw(_graphicsDevice);

                            if (BloomDownsamplePasses > 4)
                            {
                                BloomInverseResolution *= 2;

                                //DOWNSAMPLE TO MIP5
                                _graphicsDevice.SetRenderTarget(_mipMaps[5]);

                                BloomScreenTexture = _mipMaps[4];
                                //Pass
                                _effectSetup.Pass_Downsample.Apply();
                                _fullscreenTarget.Draw(_graphicsDevice);

                                ChangeBlendState();

                                //UPSAMPLE TO MIP4
                                _graphicsDevice.SetRenderTarget(_mipMaps[4]);
                                BloomScreenTexture = _mipMaps[5];

                                BloomStrength = _strength[4];
                                BloomRadius = _radius[4];
                                _effectSetup.Pass_Upsample.Apply();
                                _fullscreenTarget.Draw(_graphicsDevice);

                                BloomInverseResolution /= 2;
                            }

                            ChangeBlendState();

                            //UPSAMPLE TO MIP3
                            _graphicsDevice.SetRenderTarget(_mipMaps[3]);
                            BloomScreenTexture = _mipMaps[4];

                            BloomStrength = _strength[3];
                            BloomRadius = _radius[3];
                            _effectSetup.Pass_Upsample.Apply();
                            _fullscreenTarget.Draw(_graphicsDevice);

                            BloomInverseResolution /= 2;

                        }

                        ChangeBlendState();

                        //UPSAMPLE TO MIP2
                        _graphicsDevice.SetRenderTarget(_mipMaps[2]);
                        BloomScreenTexture = _mipMaps[3];

                        BloomStrength = _strength[2];
                        BloomRadius = _radius[2];
                        _effectSetup.Pass_Upsample.Apply();
                        _fullscreenTarget.Draw(_graphicsDevice);

                        BloomInverseResolution /= 2;

                    }

                    ChangeBlendState();

                    //UPSAMPLE TO MIP1
                    _graphicsDevice.SetRenderTarget(_mipMaps[1]);
                    BloomScreenTexture = _mipMaps[2];

                    BloomStrength = _strength[1];
                    BloomRadius = _radius[1];
                    _effectSetup.Pass_Upsample.Apply();
                    _fullscreenTarget.Draw(_graphicsDevice);

                    BloomInverseResolution /= 2;
                }

                ChangeBlendState();

                //UPSAMPLE TO MIP0
                _graphicsDevice.SetRenderTarget(_mipMaps[0]);
                BloomScreenTexture = _mipMaps[1];

                BloomStrength = _strength[0];
                BloomRadius = _radius[0];

                _effectSetup.Pass_Upsample.Apply();
                _fullscreenTarget.Draw(_graphicsDevice);
            }

            //Note the final step could be done as a blend to the final texture.

            return _mipMaps[0];
        }

        private void ApplyGameSettings()
        {
            Array.Copy(RenderingSettings.Bloom.Radius, _radius, _radius.Length);
            Array.Copy(RenderingSettings.Bloom.Strength, _strength, _strength.Length);

            BloomThreshold = RenderingSettings.Bloom.Threshold * 0.1f;
        }

        private void ChangeBlendState()
        {
            _graphicsDevice.SetState(BlendStateOption.AlphaBlend);
        }


        /// <summary>
        //Dispose our RenderTargets. This is not covered by the Garbage Collector so we have to do it manually
        /// </summary>
        public void Dispose()
        {
            _mipMaps?.Dispose();
            _graphicsDevice?.Dispose();
            _effectSetup?.Dispose();
        }
    }

    //enums
    public enum BloomPresets
    {
        Wide,
        Focussed,
        Small,
        SuperWide,
        Cheap
    };

}
