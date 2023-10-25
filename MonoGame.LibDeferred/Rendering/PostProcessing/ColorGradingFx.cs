using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using SharpDX.Direct3D9;

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
        protected override bool GetEnabled() => _enabled && RenderingSettings.g_ColorGrading;


        private RenderTarget2D _renderTarget;

        private Texture2D _lookupTable;
        private ColorGradingFxSetup _fxSetup = new ColorGradingFxSetup();
        public enum LUTSizes { Size16, Size32 };


        public Texture2D LookUpTable
        {
            set
            {
                _lookupTable = value;
                _fxSetup.Param_LUT.SetValue(value);
                int size = (_lookupTable.Width == 64) ? 16 : 32;
                _fxSetup.Param_Size.SetValue((float)size);
                _fxSetup.Param_SizeRoot.SetValue((float)(size == 16 ? 4 : 8));

            }
        }




        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public ColorGradingFx(ContentManager content, string shaderPath = "Shaders/PostProcessing/ColorGrading")
        {
            _lookupTable = content.Load<Texture2D>("Shaders/PostProcessing/lut");
            _fxSetup.Param_LUT.SetValue(_lookupTable);
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
            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.SetState(BlendStateOption.Opaque);

            _fxSetup.Param_InputTexture.SetValue(sourceRT);
            this.Draw(_fxSetup.Pass_ApplyLUT);
            return _renderTarget;
        }

        /// <summary>
        /// A function to create and save a new lookup-table with unmodified colors. 
        /// Check the github readme for use.
        /// </summary>
        /// <param name="lutsize">32 or 16. 32 will result in a larger LUT which results in better images but worse performance</param>
        /// <param name="relativeFilePath">for example "Lut16.png". The base directory is where the .exe is started from</param>
        protected void CreateLUT(LUTSizes lutsize, string relativeFilePath)
        {
            _renderTarget?.Dispose();

            int size = lutsize == LUTSizes.Size16 ? 16 * 4 : 32 * 8;
            _renderTarget = new RenderTarget2D(_graphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);
            _graphicsDevice.SetRenderTarget(_renderTarget);


            _fxSetup.Param_Size.SetValue((float)(lutsize == LUTSizes.Size16 ? 16 : 32));
            _fxSetup.Param_SizeRoot.SetValue((float)(lutsize == LUTSizes.Size16 ? 4 : 8));

            this.Draw(_fxSetup.Pass_CreateLUT);

            //Save this texture
            Stream stream = File.Create(relativeFilePath);
            _renderTarget.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
            stream.Dispose();
        }

        public override void Dispose()
        {
            _fxSetup?.Dispose();
            _renderTarget?.Dispose();
        }


    }
}
