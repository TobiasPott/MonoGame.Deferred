using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.Helper.HelperGeometry
{
    public class LineHelperManager
    {
        private readonly List<LineBuffer> Lines = new List<LineBuffer>();

        private int _tempVertsPoolLength = 100;

        private VertexPositionColor[] _tempVertsPool;
        private int _tempVertsPoolIndex;
        private int _tempVertsPoolOverCount;

        private readonly Vector3[] _frustumCorners = new Vector3[8];
        private readonly Vector3[] _bBoxCorners = new Vector3[8];

        public LineHelperManager()
        {
            _tempVertsPool = new VertexPositionColor[_tempVertsPoolLength];
        }

        public VertexPositionColor GetVertexPositionColor(Vector3 point, Color color)
        {
            // ToDo: @tpott: change to use either list or 
            if (_tempVertsPoolIndex < _tempVertsPoolLength - 3) //Buffer
            {
                _tempVertsPool[_tempVertsPoolIndex].Position = point;
                _tempVertsPool[_tempVertsPoolIndex].Color = color;
                _tempVertsPoolIndex++;
                return _tempVertsPool[_tempVertsPoolIndex - 1];
            }
            _tempVertsPoolOverCount++;
            return new VertexPositionColor(point, color);
        }

        private void AdjustTempVertsPoolSize()
        {
            if (_tempVertsPoolOverCount > 0)
            {
                _tempVertsPoolLength += _tempVertsPoolOverCount;
                _tempVertsPool = new VertexPositionColor[_tempVertsPoolLength];
            }

            _tempVertsPoolOverCount = 0;
            _tempVertsPoolIndex = 0;
        }

        public void AddLineStartEnd(Vector3 start, Vector3 end, short timer) 
            => Lines.Add(new LineBuffer(start, end, timer, this));

        public void AddLineStartDir(Vector3 start, Vector3 dir, short timer)
            => AddLineStartEnd(start, start + dir, timer);

        public void AddLineStartEnd(Vector3 start, Vector3 end, short timer, Color startColor, Color endColor)
            => Lines.Add(new LineBuffer(start, end, timer, startColor, endColor, this));

        public void AddLineStartDir(Vector3 start, Vector3 dir, short timer, Color startColor, Color endColor)
            => Lines.Add(new LineBuffer(start, start + dir, timer, startColor, endColor, this));

        public void AddFrustum(BoundingFrustumEx frustum, short timer, Color color)
        {
            Vector3[] corners = frustum.GetCornersNoCopy();
            //Front
            Lines.Add(new LineBuffer(corners[0], corners[1], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[1], corners[2], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[2], corners[3], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[3], corners[0], timer, color, color, this));
            //Back
            Lines.Add(new LineBuffer(corners[4], corners[5], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[5], corners[6], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[6], corners[7], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[7], corners[4], timer, color, color, this));
            //Between
            Lines.Add(new LineBuffer(corners[4], corners[0], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[5], corners[1], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[6], corners[2], timer, color, color, this));
            Lines.Add(new LineBuffer(corners[7], corners[3], timer, color, color, this));
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewProjection, EffectParameter Param_WorldViewProjection, EffectPass Pass_VertexColor)
        {
            if (!RenderingSettings.d_EnableLineHelper) return;

            Param_WorldViewProjection.SetValue(viewProjection);

            Pass_VertexColor.Apply();
            // ToDo: @tpott: Change rendering lines to build the vertex and index buffer when adding lines
            //          This should allow to remove the LineHelper type as overhead
            for (int i = 0; i < Lines.Count; i++)
            {
                LineBuffer line = Lines[i];
                if (line != null)
                {

                    //Gather
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, line.Verts, 0, 2, LineBuffer.Indices, 0, 1);

                    line.Timer--;
                    if (line.Timer <= 0)
                    {
                        Lines.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    Lines.RemoveAt(i);
                }

            }
            AdjustTempVertsPoolSize();
        }

        public void CreateBoundingBoxLines(BoundingFrustum boundingFrustumExShadow)
        {
            boundingFrustumExShadow.GetCorners(_frustumCorners);
            Vector3[] vertices = _frustumCorners;
            AddLineStartEnd(vertices[0], vertices[1], 1);
            AddLineStartEnd(vertices[1], vertices[2], 1);
            AddLineStartEnd(vertices[2], vertices[3], 1);
            AddLineStartEnd(vertices[3], vertices[0], 1);

            AddLineStartEnd(vertices[0], vertices[4], 1);
            AddLineStartEnd(vertices[1], vertices[5], 1);
            AddLineStartEnd(vertices[2], vertices[6], 1);
            AddLineStartEnd(vertices[3], vertices[7], 1);

            AddLineStartEnd(vertices[4], vertices[5], 1);
            AddLineStartEnd(vertices[5], vertices[6], 1);
            AddLineStartEnd(vertices[6], vertices[7], 1);
            AddLineStartEnd(vertices[7], vertices[4], 1);
        }

        public void AddBoundingBox(ModelEntity entity)
        {
            if (entity != null)
            {
                entity.BoundingBox.GetCorners(_bBoxCorners);

                Vector3[] vertices = _bBoxCorners;
                //Transform
                for (var index = 0; index < vertices.Length; index++)
                {
                    vertices[index] = Vector3.Transform(vertices[index], entity.World);
                }

                AddLineStartEnd(vertices[0], vertices[1], 1);
                AddLineStartEnd(vertices[1], vertices[2], 1);
                AddLineStartEnd(vertices[2], vertices[3], 1);
                AddLineStartEnd(vertices[3], vertices[0], 1);

                AddLineStartEnd(vertices[0], vertices[4], 1);
                AddLineStartEnd(vertices[1], vertices[5], 1);
                AddLineStartEnd(vertices[2], vertices[6], 1);
                AddLineStartEnd(vertices[3], vertices[7], 1);

                AddLineStartEnd(vertices[4], vertices[5], 1);
                AddLineStartEnd(vertices[5], vertices[6], 1);
                AddLineStartEnd(vertices[6], vertices[7], 1);
                AddLineStartEnd(vertices[7], vertices[4], 1);
            }
        }

    }
}
