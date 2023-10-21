using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{
    /// <summary>
    /// 
    ///     /// Version 1.0, 24. April. 2017
    /// 
    ///     Color Grading / Correction Filter, TheKosmonaut (kosmonaut3d@googlemail.com)
    /// 
    /// A post-process effect that changes colors of the image based on a look-up table (LUT). 
    /// For more information check out the github info file / readme.md
    /// You can use Draw() to apply the color grading / color correction to an image and use the returned texture for output.
    /// You can use CreateLUT to create default Look-up tables with unmodified colors.
    /// </summary>
    public partial class ColorGradingFx : BaseFx
    {

        private bool _enabled = true;
        public bool Enabled { get => _enabled && RenderingSettings.g_ColorGrading; set { _enabled = value; } }

        private int _size;
        private RenderTarget2D _renderTarget;

        private Texture2D _inputTexture;
        private Texture2D _lookupTable;
        private ColorGradingFxEffectSetup _effectSetup = new ColorGradingFxEffectSetup();
        public enum LUTSizes { Size16, Size32 };


        private int Size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    if (value != 16 && value != 32) throw new NotImplementedException("only 16 and 32 supported right now");
                    _size = value;
                    _effectSetup.Param_Size.SetValue((float)_size);
                    _effectSetup.Param_SizeRoot.SetValue((float)(_size == 16 ? 4 : 8));
                }
            }
        }

        private Texture2D InputTexture
        {
            get { return _inputTexture; }
            set
            {
                if (value != _inputTexture)
                {
                    _inputTexture = value;
                    _effectSetup.Param_InputTexture.SetValue(value);
                }
            }
        }

        private Texture2D LookUpTable
        {
            get { return _lookupTable; }
            set
            {
                if (value != _lookupTable)
                {
                    _lookupTable = value;
                    _effectSetup.Param_LUT.SetValue(value);
                }
            }
        }




        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public ColorGradingFx(ContentManager content, string shaderPath = "Shaders/PostProcessing/ColorGrading")
        {
            LookUpTable = content.Load<Texture2D>("Shaders/PostProcessing/lut");
        }


        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {

            //Set up rendertarget
            if (_renderTarget == null || _renderTarget.Width != sourceRT.Width || _renderTarget.Height != sourceRT.Height)
            {
                _renderTarget?.Dispose();
                _renderTarget = new RenderTarget2D(_graphicsDevice, sourceRT.Width, sourceRT.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            InputTexture = sourceRT;
            Size = (_lookupTable.Width == 64) ? 16 : 32;

            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.SetState(BlendStateOption.Opaque);
            this.Draw(_effectSetup.Pass_ApplyLUT);
            return _renderTarget;
        }

        /// <summary>
        /// A function to create and save a new lookup-table with unmodified colors. 
        /// Check the github readme for use.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="lutsize">32 or 16. 32 will result in a larger LUT which results in better images but worse performance</param>
        /// <param name="relativeFilePath">for example "Lut16.png". The base directory is where the .exe is started from</param>
        protected void CreateLUT(LUTSizes lutsize, string relativeFilePath)
        {
            _renderTarget?.Dispose();

            _effectSetup.Param_Size.SetValue((float)(lutsize == LUTSizes.Size16 ? 16 : 32));
            _effectSetup.Param_SizeRoot.SetValue((float)(lutsize == LUTSizes.Size16 ? 4 : 8));
            int size = lutsize == LUTSizes.Size16 ? 16 * 4 : 32 * 8;

            _renderTarget = new RenderTarget2D(_graphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);

            _graphicsDevice.SetRenderTarget(_renderTarget);

            this.Draw(_effectSetup.Pass_CreateLUT);

            //Save this texture
            Stream stream = File.Create(relativeFilePath);
            _renderTarget.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
            stream.Dispose();
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
            _renderTarget?.Dispose();
        }


    }
}
