using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Renderer.RenderModules
{
    public class HelperGeometryRenderModule
    {
        private HelperGeometryEffectSetup _effectSetup = new HelperGeometryEffectSetup();

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
            HelperGeometryManager.GetInstance().Draw(_graphicsDevice, _viewProjection, _effectSetup);
        }
    }


    public class HelperGeometryEffectSetup : EffectSetupBase
    {

        //Lines
        public Effect Effect { get; protected set; } //= ShaderGlobals.content.Load<Effect>("Shaders/Editor/LineEffect");

        public EffectPass Pass_VertexColor { get; protected set; }// = Effect.Techniques["VertexColor"].Passes[0];
        public EffectPass Pass_GlobalColor { get; protected set; }// = Effect.Techniques["GlobalColor"].Passes[0];

        public EffectParameter Param_WorldViewProj { get; protected set; }// = Effect.Parameters["WorldViewProj"];
        public EffectParameter Param_GlobalColor { get; protected set; } //= Effect.Parameters["GlobalColor"];


        public HelperGeometryEffectSetup(string shaderPath = "Shaders/Editor/LineEffect")
              : base(shaderPath)
        {
            Effect = ShaderGlobals.content.Load<Effect>(shaderPath);

            Pass_VertexColor = Effect.Techniques["VertexColor"].Passes[0];
            Pass_GlobalColor = Effect.Techniques["GlobalColor"].Passes[0];

            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_GlobalColor = Effect.Parameters["GlobalColor"];
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }
}