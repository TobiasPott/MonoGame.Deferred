using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    public class LineBuffer
    {
        public static short[] Indices = { 0, 1 };
        public VertexPositionColor[] Verts;

        private Vector3 _start;
        private Vector3 _end;

        public short Timer;

        public LineBuffer(Vector3 start, Vector3 end, short time, LineHelperManager lineHelperManager)
            : this(start, end, time, new Color(Color.Red, 0.5f), new Color(Color.Green, 0.5f), lineHelperManager)
        { }
        public LineBuffer(Vector3 start, Vector3 end, short time, Color starColor, Color endColor, LineHelperManager lineHelperManager)
        {
            if (!RenderingSettings.d_Drawlines) return;

            _start = start;
            _end = end;

            Verts = new VertexPositionColor[2];
            Verts[0] = lineHelperManager.GetVertexPositionColor(_start, starColor);
            Verts[1] = lineHelperManager.GetVertexPositionColor(_end, endColor);

            Timer = time;
        }

    }
}
