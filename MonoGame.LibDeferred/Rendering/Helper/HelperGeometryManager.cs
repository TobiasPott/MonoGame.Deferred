using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper.HelperGeometry
{
    //Singleton
    public class HelperGeometryManager
    {
        private static HelperGeometryManager _instance;

        private LineHelperManager _lineHelperManager;
        private OctahedronHelperManager _octahedronHelperManager;

        public HelperGeometryManager()
        {
            _lineHelperManager = new LineHelperManager();
            _octahedronHelperManager = new OctahedronHelperManager();
        }


        public static HelperGeometryManager GetInstance()
        {
            if (_instance == null) return _instance = new HelperGeometryManager();
            return _instance;
        }

        public void Draw(GraphicsDevice graphics, Matrix viewProjection)
        {
            _lineHelperManager.Draw(graphics, viewProjection, Shaders.HelperGeometry.Param_WorldViewProj, Shaders.HelperGeometry.Pass_VertexColor);
            _octahedronHelperManager.Draw(graphics, viewProjection, Shaders.HelperGeometry.Param_WorldViewProj, Shaders.HelperGeometry.Param_GlobalColor, Shaders.HelperGeometry.Pass_GlobalColor);
        }

        public void AddLineStartDir(Vector3 start, Vector3 dir, short timer, Color startColor, Color endColor)
        {
            _lineHelperManager.AddLineStartDir(start, dir, timer, startColor, endColor);
        }

        public void CreateBoundingBoxLines(BoundingFrustum boundingFrustum)
        {
            _lineHelperManager.CreateBoundingBoxLines(boundingFrustum);
        }

        public void AddLineStartEnd(Vector3 startPosition, Vector3 EndPosition, short timer)
        {
            _lineHelperManager.AddLineStartEnd(startPosition, EndPosition, timer);
        }

        public void AddLineStartEnd(Vector3 start, Vector3 end, short timer, Color startColor, Color endColor)
        {
            _lineHelperManager.AddLineStartEnd(start, end, timer, startColor, endColor);
        }

        public void AddOctahedron(Vector3 position, Vector4 color)
        {
            _octahedronHelperManager.AddOctahedron(position, color);
        }

        public void AddBoundingBox(ModelEntity entity)
        {
            _lineHelperManager.AddBoundingBox(entity);
        }
    }
}
