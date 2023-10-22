using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Rendering.PostProcessing
{

    public partial class SSReflectionFx : BaseFx
    {

        private bool _enabled = true;
        public bool Enabled { get => _enabled && RenderingSettings.g_SSReflection; set { _enabled = value; } }


        private SSReflectionFxSetup _effectSetup = new SSReflectionFxSetup();


        /// <summary>
        /// A filter that allows color grading by using Look up tables
        /// </summary>
        public SSReflectionFx(ContentManager content)
        {
        }

        /// <summary>
        /// returns a modified image with color grading applied.
        /// </summary>
        public override RenderTarget2D Draw(RenderTarget2D sourceRT, RenderTarget2D previousRT = null, RenderTarget2D destRT = null)
        {
            if (!this.Enabled)
                return sourceRT;
            return destRT;
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }


    }
}
