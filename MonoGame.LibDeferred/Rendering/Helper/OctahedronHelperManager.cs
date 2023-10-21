using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.Helper
{
    public class OctahedronHelperManager
    {
        private OctahedronBuffer _octahedronMesh;

        private List<Vector3> positions = new List<Vector3>();
        private List<Vector4> colors = new List<Vector4>();
        private Matrix scale = Matrix.CreateScale(.005f);

        public void AddOctahedron(Vector3 position, Vector4 color)
        {
            positions.Add(position);
            colors.Add(color);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewProjection, EffectParameter Param_WorldViewProjection, EffectParameter Param_GlobalColor, EffectPass Pass_GlobalColor)
        {
            if (_octahedronMesh == null) _octahedronMesh = new OctahedronBuffer(graphicsDevice);

            graphicsDevice.SetVertexBuffer(_octahedronMesh.VertexBuffer);
            graphicsDevice.Indices = _octahedronMesh.IndexBuffer;

            for (int i = 0; i < positions.Count; i++)
            {

                Matrix wvp = scale * Matrix.CreateTranslation(positions[i]) * viewProjection;

                Param_WorldViewProjection.SetValue(wvp);
                Param_GlobalColor.SetValue(colors[i]);

                Pass_GlobalColor.Apply();

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8);
            }

            //Clear

            positions.Clear();
            colors.Clear();
        }

    }
}
