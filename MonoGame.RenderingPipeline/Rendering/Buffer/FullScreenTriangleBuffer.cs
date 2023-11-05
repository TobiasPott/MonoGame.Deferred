using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace DeferredEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClipVertexPositionTexture : IVertexType
    {
        public Vector2 Position;
        public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public ClipVertexPositionTexture(Vector2 position, Vector2 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        public override int GetHashCode()
        {
            return (Position.GetHashCode() * 397) ^ TextureCoordinate.GetHashCode();
        }

        public override string ToString()
        {
            string[] obj = new string[5] { "{{Position:", null, null, null, null };
            Vector2 position = Position;
            obj[1] = position.ToString();
            obj[2] = " TextureCoordinate:";
            Vector2 textureCoordinate = TextureCoordinate;
            obj[3] = textureCoordinate.ToString();
            obj[4] = "}}";
            return string.Concat(obj);
        }

        public static bool operator ==(ClipVertexPositionTexture left, ClipVertexPositionTexture right)
        {
            if (left.Position == right.Position)
            {
                return left.TextureCoordinate == right.TextureCoordinate;
            }

            return false;
        }

        public static bool operator !=(ClipVertexPositionTexture left, ClipVertexPositionTexture right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return this == (ClipVertexPositionTexture)obj;
        }

        static ClipVertexPositionTexture()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                );
        }
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


        private static ClipVertexPositionTexture[] Vertices = new[] {
            new ClipVertexPositionTexture(new Vector2(-1, -1), new Vector2(0, 1)),
            new ClipVertexPositionTexture(new Vector2(-1, 1), new Vector2(0, 0)),
            new ClipVertexPositionTexture(new Vector2(1, -1), new Vector2(1, 1)),
            new ClipVertexPositionTexture(new Vector2(1, 1), new Vector2(1, 0))
        };
        private static ushort[] Indices = new ushort[] { 0, 1, 2, 2, 1, 3 };

        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        public FullscreenTriangleBuffer(GraphicsDevice graphics)
        {
            _vertexBuffer = new VertexBuffer(graphics, ClipVertexPositionTexture.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(Vertices);
            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, Indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(Indices);
        }

        public void Draw(GraphicsDevice graphics)
        {
            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.Indices = null;
            //graphics.Indices = _indexBuffer;
            //graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            graphics.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
        }
    }
}
