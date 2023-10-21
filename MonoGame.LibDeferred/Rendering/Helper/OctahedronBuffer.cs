using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.Helper
{
    public class OctahedronBuffer : IDisposable
    {
        private static short[] IndexBufferData = new short[] { 2, 0, 1, 1, 0, 4, 4, 0, 3, 3, 0, 2, 5, 2, 1, 5, 1, 4, 5, 4, 3, 5, 3, 2 };
        private static VertexPosition[] VertexBufferData = new VertexPosition[] {
                                                            new VertexPosition(new Vector3(0, 0, -1)),
                                                            new VertexPosition(new Vector3(0, -1, 0)),
                                                            new VertexPosition(new Vector3(1, 0, 0)),
                                                            new VertexPosition(new Vector3(0, 1, 0)),
                                                            new VertexPosition(new Vector3(-1, 0, 0)),
                                                            new VertexPosition(new Vector3(0, 0, 1))
                                                        };

        public VertexBuffer VertexBuffer { get; protected set; }
        public IndexBuffer IndexBuffer { get; protected set; }

        public OctahedronBuffer(GraphicsDevice graphics)
        {
            VertexBuffer = new VertexBuffer(graphics, VertexPosition.VertexDeclaration, 6, BufferUsage.WriteOnly);
            IndexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);

            VertexBuffer.SetData(VertexBufferData);
            IndexBuffer.SetData(IndexBufferData);
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}
