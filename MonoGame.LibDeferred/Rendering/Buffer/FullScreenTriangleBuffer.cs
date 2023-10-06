using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    public class FullScreenTriangleBuffer
    {
        public static FullScreenTriangleBuffer Instamce {  get; private set; }
        public static void InitClass(GraphicsDevice graphicsDevice)
        { Instamce = new FullScreenTriangleBuffer(graphicsDevice); }



        private VertexBuffer vertexBuffer;

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

        public FullScreenTriangleBuffer(GraphicsDevice graphics)
        {
            FullScreenQuadVertex[] vertices = new FullScreenQuadVertex[3];
            vertices[0] = new FullScreenQuadVertex(new Vector2(-1, -1));
            vertices[1] = new FullScreenQuadVertex(new Vector2(-1, 3));
            vertices[2] = new FullScreenQuadVertex(new Vector2(3, -1));

            vertexBuffer = new VertexBuffer(graphics, FullScreenQuadVertex.VertexDeclaration, 3, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        public void Draw(GraphicsDevice graphics)
        {
            graphics.SetVertexBuffer(vertexBuffer);
            graphics.Indices = null;

            graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }

        public void Dispose()
        {
            vertexBuffer?.Dispose();
        }
    }
}
