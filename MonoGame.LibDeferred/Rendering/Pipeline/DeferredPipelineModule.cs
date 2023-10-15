using Microsoft.Xna.Framework.Content;

namespace DeferredEngine.Renderer.RenderModules
{

    public class DeferredPipelineModule : PipelineModule
    {

        private DeferredEffectSetup _effectSetup = new DeferredEffectSetup();


        public DeferredPipelineModule(ContentManager content, string shaderPath)
            : base(content, shaderPath)
        {
            Load(content, shaderPath);
        }

        //public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        //{
        //    base.Initialize(graphicsDevice, spriteBatch);
        //}
        protected override void Load(ContentManager content, string shaderPath = "Shaders/Deferred/DeferredCompose")
        { }

        // ToDo: @tpott: extract the deferred rendering part out of the rendering pipeline class
        public override void Dispose()
        {

        }
    }

}
