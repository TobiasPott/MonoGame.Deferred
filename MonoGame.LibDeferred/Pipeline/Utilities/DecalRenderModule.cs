using DeferredEngine.Entities;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Utilities
{
    public class DecalRenderModule : PipelineModule, IDisposable
    {
        public static readonly short[] indicesCage = new short[]
        {
                0, 1, 1,
                3, 3, 2,
                2, 0, 4,
                5, 5, 7,
                7, 6, 6,
                4, 0, 4,
                1, 5, 2,
                6, 3, 7,
        };
        public static readonly short[] indicesCube = new short[]
        {
                0,4,1,
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
                1,3,2
        };

        //Deferred Decals
        public static bool g_EnableDecals = true;

        private readonly DecalEffectSetup _effectSetup = new DecalEffectSetup();

        private BlendState _decalBlend;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBufferCage;
        private IndexBuffer _indexBufferCube;


        public Texture2D DepthMap { set { _effectSetup.Param_DepthMap.SetValue(value); } }


        public DecalRenderModule()
            : base()
        { }


        public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            base.Initialize(graphicsDevice, spriteBatch);

            _decalBlend = new BlendState()
            {
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.SourceAlpha,
            };


            Vector3 a = -Vector3.One;
            Vector3 b = Vector3.One;
            Color color = Color.White;
            Color colorUpper = new Color(0, 255, 0, 255);

            VertexPositionColor[] verts = new VertexPositionColor[8];
            verts[0] = new VertexPositionColor(new Vector3(a.X, a.Y, a.Z), color);
            verts[1] = new VertexPositionColor(new Vector3(b.X, a.Y, a.Z), color);
            verts[2] = new VertexPositionColor(new Vector3(a.X, b.Y, a.Z), color);
            verts[3] = new VertexPositionColor(new Vector3(b.X, b.Y, a.Z), color);

            verts[4] = new VertexPositionColor(new Vector3(a.X, a.Y, b.Z), colorUpper);
            verts[5] = new VertexPositionColor(new Vector3(b.X, a.Y, b.Z), colorUpper);
            verts[6] = new VertexPositionColor(new Vector3(a.X, b.Y, b.Z), colorUpper);
            verts[7] = new VertexPositionColor(new Vector3(b.X, b.Y, b.Z), colorUpper);


            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, 8, BufferUsage.WriteOnly);
            _indexBufferCage = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);
            _indexBufferCube = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);

            _vertexBuffer.SetData(verts);
            _indexBufferCage.SetData(indicesCage);
            _indexBufferCube.SetData(indicesCube);
        }

        public void Draw(EntitySceneGroup scene, RenderTarget2D sourceRT, RenderTarget2D auxRT, RenderTarget2D destRT) => Draw(scene.Decals, sourceRT, auxRT, destRT, this.Matrices.View, this.Matrices.ViewProjection, this.Matrices.InverseView);
        public void Draw(List<Decal> decals, RenderTarget2D sourceRT, RenderTarget2D auxRT, RenderTarget2D destRT, Matrix view, Matrix viewProjection, Matrix inverseView)
        {
            // blit current buffer to aux to work on this (decals are not full GBuffer support and misses normals)
            this.Blit(destRT, auxRT);

            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBufferCube;
            _graphicsDevice.SetState(RasterizerStateOption.CullClockwise);
            _graphicsDevice.BlendState = _decalBlend;


            _effectSetup.Param_FarClip.SetValue(this.Frustum.FarClip);

            foreach (Decal decal in decals)
            {
                Matrix localMatrix = decal.World;

                _effectSetup.Param_DecalMap.SetValue(decal.Texture);
                _effectSetup.Param_WorldView.SetValue(localMatrix * view);
                _effectSetup.Param_WorldViewProj.SetValue(localMatrix * viewProjection);
                _effectSetup.Param_InverseWorldView.SetValue(inverseView * decal.InverseWorld);

                _effectSetup.Pass_Decal.Apply();
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }

            this.Blit(auxRT, destRT);
        }

        public void DrawOutlines(Decal decal)
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBufferCage;

            Matrix localMatrix = decal.World;
            _effectSetup.Param_WorldView.SetValue(localMatrix * this.Matrices.View);
            _effectSetup.Param_WorldViewProj.SetValue(localMatrix * this.Matrices.ViewProjection);

            _effectSetup.Param_FarClip.SetValue(this.Frustum.FarClip);

            _effectSetup.Pass_Outline.Apply();

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
        }

        public override void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBufferCage?.Dispose();
            _indexBufferCube?.Dispose();
            _decalBlend?.Dispose();
            _effectSetup?.Dispose();
        }
    }

}
