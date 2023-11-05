using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    public struct FullScreenQuadVertex
    {
        // Stores the starting position of the particle.
        public Vector2 Position;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0)
        );

        public FullScreenQuadVertex(Vector2 position)
        {
            Position = position;
        }

        public const int SizeInBytes = 8;
    }

    public class FullscreenTriangleBuffer
    {
        #region Singleton & Static Load/Unload
        public static FullscreenTriangleBuffer Instance { get; private set; }
        public static void InitClass(GraphicsDevice graphicsDevice)
        {
            Instance ??= new FullscreenTriangleBuffer(graphicsDevice);
        }
        public static void UnloadClass() => Instance?.Dispose();

        #endregion


        private static VertexPositionTexture[] Vertices = new[] {
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-1, 3, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(3, -1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(3, 3, 0), new Vector2(1, 1))
        };
        private static ushort[] Indices = new ushort[] { 0, 1, 2, 2, 1, 3 };

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        public FullscreenTriangleBuffer(GraphicsDevice graphics)
        {
            _vertexBuffer = new VertexBuffer(graphics, VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(Vertices);
            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, Indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(Indices);
        }

        public void Draw(GraphicsDevice graphics)
        {
            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.Indices = _indexBuffer;
            graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
        }
    }
}
