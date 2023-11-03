using DeferredEngine.Rendering.Helper.HelperGeometry;


namespace DeferredEngine.Pipeline.Utilities
{
    public class HelperRenderModule : PipelineModule, IDisposable
    {
        private readonly HelperGeometryEffectSetup _effectSetup = new HelperGeometryEffectSetup();

        public HelperRenderModule()
        { }


        public void Draw()
        {
            HelperGeometryManager.GetInstance().Draw(_graphicsDevice, this.Matrices.ViewProjection, _effectSetup);
        }

        public override void Dispose()
        {
            _effectSetup?.Dispose();
        }

    }

}