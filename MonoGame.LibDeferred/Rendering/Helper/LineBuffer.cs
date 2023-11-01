using DeferredEngine.Rendering.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.Helper
{
    public class LineBuffer
    {
        public VertexPositionColor[] Verts;

        public short Timer;

        public LineBuffer(Vector3 start, Vector3 end, short time, LineHelperManager lineHelperManager)
            : this(start, end, time, new Color(Color.Red, 0.5f), new Color(Color.Green, 0.5f), lineHelperManager)
        { }
        public LineBuffer(Vector3 start, Vector3 end, short time, Color starColor, Color endColor, LineHelperManager lineHelperManager)
        {
            Verts = new VertexPositionColor[2];
            Verts[0] = lineHelperManager.GetVertexPositionColor(start, starColor);
            Verts[1] = lineHelperManager.GetVertexPositionColor(end, endColor);

            Timer = time;
        }

    }
}
