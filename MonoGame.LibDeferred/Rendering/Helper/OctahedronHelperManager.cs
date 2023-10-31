using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.Helper
{
    public class OctahedronHelperManager
    {
        private OctahedronBuffer _octahedronMesh;

        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Vector4> _colors = new List<Vector4>();
        private readonly Matrix _scale = Matrix.CreateScale(0.005f);

        public void AddOctahedron(Vector3 position, Vector4 color)
        {
            _positions.Add(position);
            _colors.Add(color);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewProjection, EffectParameter Param_WorldViewProjection, EffectParameter Param_GlobalColor, EffectPass Pass_GlobalColor)
        {
            if (_octahedronMesh == null) _octahedronMesh = new OctahedronBuffer(graphicsDevice);

            graphicsDevice.SetVertexBuffer(_octahedronMesh.VertexBuffer);
            graphicsDevice.Indices = _octahedronMesh.IndexBuffer;

            for (int i = 0; i < _positions.Count; i++)
            {

                Matrix wvp = _scale * Matrix.CreateTranslation(_positions[i]) * viewProjection;

                Param_WorldViewProjection.SetValue(wvp);
                Param_GlobalColor.SetValue(_colors[i]);

                Pass_GlobalColor.Apply();

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8);
            }

            //Clear

            _positions.Clear();
            _colors.Clear();
        }

    }
}
