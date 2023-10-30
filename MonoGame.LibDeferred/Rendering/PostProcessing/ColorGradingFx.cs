﻿using Microsoft.Xna.Framework.Graphics;
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
    public partial class ColorGradingFx : PostFx
    {
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);


        protected override bool GetEnabled() => _enabled && ModuleEnabled;


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
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {

            // ToDo: Resolve this Draw method to return either sourceRT or destRT to match the other behaviours
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

        public override void Dispose()
        {
            _fxSetup?.Dispose();
            _renderTarget?.Dispose();
        }


    }
}
