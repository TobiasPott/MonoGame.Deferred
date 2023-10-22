namespace DeferredEngine.Pipeline
{
    public abstract class BaseFxSetup : IDisposable
    {
        public string ShaderPath { get; protected set; }

        public BaseFxSetup(string shaderPath)
        {
            ShaderPath = shaderPath;
        }
        public abstract void Dispose();
    }

}
