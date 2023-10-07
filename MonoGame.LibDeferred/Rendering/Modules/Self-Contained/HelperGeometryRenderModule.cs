using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class HelperGeometryRenderModule
    {
        //Lines
        private Effect _shader; //= Globals.content.Load<Effect>("Shaders/Editor/LineEffect");
        private EffectParameter _worldViewProjParam;
        private EffectParameter _globalColorParam;
        private EffectPass _vertexColorPass;
        private EffectPass _globalColorPass;

        public Matrix ViewProjection;
        private GraphicsDevice _graphicsDevice;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _worldViewProjParam = _shader.Parameters["WorldViewProj"];
            _globalColorParam = _shader.Parameters["GlobalColor"];

            //Passes
            _vertexColorPass = _shader.Techniques["VertexColor"].Passes[0];
            _globalColorPass = _shader.Techniques["GlobalColor"].Passes[0];

        }

        public HelperGeometryRenderModule(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);
        }

        public void Draw()
        {
            HelperGeometryManager.GetInstance()
                .Draw(_graphicsDevice, ViewProjection, _worldViewProjParam, _globalColorParam, _vertexColorPass, _globalColorPass);
        }
    }
}
