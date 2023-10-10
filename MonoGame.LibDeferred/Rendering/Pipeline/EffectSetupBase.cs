namespace DeferredEngine.Renderer.RenderModules
{
    public abstract class EffectSetupBase : IDisposable
    {
        public string ShaderPath { get; protected set; }

        public EffectSetupBase(string shaderPath)
        {
            ShaderPath = shaderPath;
        }
        public abstract void Dispose();
    }

}
