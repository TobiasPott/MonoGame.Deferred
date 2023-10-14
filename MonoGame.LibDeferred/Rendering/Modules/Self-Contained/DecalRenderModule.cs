using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class DecalRenderModule : IDisposable
    {
        private readonly DecalEffectSetup _effectSetup = new DecalEffectSetup();

        private BlendState _decalBlend;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBufferCage;
        private IndexBuffer _indexBufferCube;

        private GraphicsDevice _graphicsDevice;

        public float FarClip { set { _effectSetup.Param_FarClip.SetValue(value); } }
        public Texture2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }


        public DecalRenderModule()
        { }


        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _decalBlend = new BlendState()
            {
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.SourceAlpha,
            };

            VertexPositionColor[] verts = new VertexPositionColor[8];

            Vector3 a = -Vector3.One;
            Vector3 b = Vector3.One;
            Color color = Color.White;
            Color colorUpper = new Color(0, 255, 0, 255);
            verts[0] = new VertexPositionColor(new Vector3(a.X, a.Y, a.Z), color);
            verts[1] = new VertexPositionColor(new Vector3(b.X, a.Y, a.Z), color);
            verts[2] = new VertexPositionColor(new Vector3(a.X, b.Y, a.Z), color);
            verts[3] = new VertexPositionColor(new Vector3(b.X, b.Y, a.Z), color);
            verts[4] = new VertexPositionColor(new Vector3(a.X, a.Y, b.Z), colorUpper);
            verts[5] = new VertexPositionColor(new Vector3(b.X, a.Y, b.Z), colorUpper);
            verts[6] = new VertexPositionColor(new Vector3(a.X, b.Y, b.Z), colorUpper);
            verts[7] = new VertexPositionColor(new Vector3(b.X, b.Y, b.Z), colorUpper);

            short[] Indices = new short[24];

            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 1;
            Indices[3] = 3;
            Indices[4] = 3;
            Indices[5] = 2;
            Indices[6] = 2;
            Indices[7] = 0;

            Indices[8] = 4;
            Indices[9] = 5;
            Indices[10] = 5;
            Indices[11] = 7;
            Indices[12] = 7;
            Indices[13] = 6;
            Indices[14] = 6;
            Indices[15] = 4;

            Indices[16] = 0;
            Indices[17] = 4;
            Indices[18] = 1;
            Indices[19] = 5;
            Indices[20] = 2;
            Indices[21] = 6;
            Indices[22] = 3;
            Indices[23] = 7;

            //short[] Indices2 = new short[36];
            short[] Indices2 = new short[] {0,4,1,
                1,4,5,
                1,5,3,
                3,5,7,
                2,3,7,
                7,6,2,
                2,6,0,
                0,6,4,
                5,4,7,
                7,4,6,
                2,0,1,
                1,3,2 };

            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, 8, BufferUsage.WriteOnly);
            _indexBufferCage = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);
            _indexBufferCube = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);

            _vertexBuffer.SetData(verts);
            _indexBufferCage.SetData(Indices);
            _indexBufferCube.SetData(Indices2);
        }

        public void Draw(List<Decal> decals, PipelineMatrices matrices) => Draw(decals, matrices.View, matrices.ViewProjection, matrices.InverseView);
        public void Draw(List<Decal> decals, Matrix view, Matrix viewProjection, Matrix inverseView)
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBufferCube;
            _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            _graphicsDevice.BlendState = _decalBlend;

            for (int index = 0; index < decals.Count; index++)
            {
                Decal decal = decals[index];

                Matrix localMatrix = decal.World;

                _effectSetup.Param_DecalMap.SetValue(decal.Texture);
                _effectSetup.Param_WorldView.SetValue(localMatrix * view);
                _effectSetup.Param_WorldViewProj.SetValue(localMatrix * viewProjection);
                _effectSetup.Param_InverseWorldView.SetValue(inverseView * decal.InverseWorld);

                _effectSetup.Pass_Decal.Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }
        }

        public void DrawOutlines(Decal decal, PipelineMatrices matrices)
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBufferCage;

            Matrix localMatrix = decal.World;

            _effectSetup.Param_WorldView.SetValue(localMatrix * matrices.View);
            _effectSetup.Param_WorldViewProj.SetValue(localMatrix * matrices.ViewProjection);

            _effectSetup.Pass_Outline.Apply();

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBufferCage?.Dispose();
            _indexBufferCube?.Dispose();
            _decalBlend?.Dispose();
            _effectSetup?.Dispose();
        }
    }

    public class DecalEffectSetup : EffectSetupBase
    {
        public Effect Effect { get; protected set; }

        public EffectPass Pass_Decal { get; protected set; }
        public EffectPass Pass_Outline { get; protected set; }

        public EffectParameter Param_DecalMap { get; protected set; }
        public EffectParameter Param_WorldView { get; protected set; }
        public EffectParameter Param_WorldViewProj { get; protected set; }
        public EffectParameter Param_InverseWorldView { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }
        public EffectParameter Param_FarClip { get; protected set; }


        public DecalEffectSetup(string shaderPath = "Shaders/Deferred/DeferredDecal")
              : base(shaderPath)
        {
            Effect = ShaderGlobals.content.Load<Effect>(shaderPath);

            Pass_Decal = Effect.Techniques["Decal"].Passes[0];
            Pass_Outline = Effect.Techniques["Outline"].Passes[0];

            Param_DecalMap = Effect.Parameters["DecalMap"];
            Param_WorldView = Effect.Parameters["WorldView"];
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_InverseWorldView = Effect.Parameters["InverseWorldView"];
            Param_DepthMap = Effect.Parameters["DepthMap"];
            Param_FarClip = Effect.Parameters["FarClip"];
        }



        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
