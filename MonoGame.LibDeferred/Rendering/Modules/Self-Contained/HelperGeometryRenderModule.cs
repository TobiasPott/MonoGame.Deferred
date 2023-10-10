using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Renderer.RenderModules
{
    public class HelperGeometryRenderModule
    {
        private Matrix _viewProjection;
        public Matrix ViewProjection { set { _viewProjection = value; } }


        private GraphicsDevice _graphicsDevice;

        public HelperGeometryRenderModule(ContentManager content, string shaderPath = "Shaders/Editor/LineEffect")
        {
            Load(content, shaderPath);
        }

        public void Load(ContentManager content, string shaderPath = "Shaders/Editor/LineEffect")
        {
        }
        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Draw()
        {
            HelperGeometryManager.GetInstance().Draw(_graphicsDevice, _viewProjection);
        }
    }
}


namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {

        //Lines
        public static class HelperGeometry
        {
            public static Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/Editor/LineEffect");

            public static EffectPass Pass_VertexColor = Effect.Techniques["VertexColor"].Passes[0];
            public static EffectPass Pass_GlobalColor = Effect.Techniques["GlobalColor"].Passes[0];

            public static EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            public static EffectParameter Param_GlobalColor = Effect.Parameters["GlobalColor"];
        }

    }

}