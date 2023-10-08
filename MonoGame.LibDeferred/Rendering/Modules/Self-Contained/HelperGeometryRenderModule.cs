using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class HelperGeometryRenderModule
    {
        //Lines
        private Effect Effect;

        private EffectPass Pass_VertexColor;
        private EffectPass Pass_GlobalColor;

        private EffectParameter Param_WorldViewProj;
        private EffectParameter Param_GlobalColor;

        private Matrix _viewProjection;
        public Matrix ViewProjection { set { _viewProjection = value; } }


        private GraphicsDevice _graphicsDevice;

        public HelperGeometryRenderModule(ContentManager content, string shaderPath = "Shaders/Editor/LineEffect")
        {
            Load(content, shaderPath);
        }

        public void Load(ContentManager content, string shaderPath = "Shaders/Editor/LineEffect")
        {
            Effect = content.Load<Effect>(shaderPath);
            //Passes
            Pass_VertexColor = Effect.Techniques["VertexColor"].Passes[0];
            Pass_GlobalColor = Effect.Techniques["GlobalColor"].Passes[0];

            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_GlobalColor = Effect.Parameters["GlobalColor"];


        }
        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Draw()
        {
            HelperGeometryManager.GetInstance()
                .Draw(_graphicsDevice, _viewProjection, Param_WorldViewProj, Param_GlobalColor, Pass_VertexColor, Pass_GlobalColor);
        }
    }
}
