using DeferredEngine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    //The individual model mesh, and a library or different world coordinates basically is what we need
    public class MeshBatch
    {
        private const int InitialLibrarySize = 4;


        private ModelMeshPart _mesh;
        private BoundingSphere _meshBoundingSphere;

        private readonly List<TransformableObject> _transforms = new List<TransformableObject>(InitialLibrarySize);
        private readonly List<Vector3> _worldBoundingCenters = new List<Vector3>(InitialLibrarySize);
        private readonly List<bool> _rendered = new(InitialLibrarySize);

        public int Count => _transforms.Count;

        public void SetMesh(ModelMeshPart mesh) => _mesh = mesh;
        public bool HasMesh(ModelMeshPart mesh) => _mesh == mesh;
        public ModelMeshPart GetMesh() => _mesh;
        public List<TransformableObject> GetTransforms() => _transforms;
        public List<bool> Rendered => _rendered;


        public bool IsAnyRendered => _rendered.Any(x => x);
        public bool AllRendered { set { for (int i = 0; i < _rendered.Count; i++) _rendered[i] = true; } }


        //IF a submesh belongs to an entity that has moved we need to update the BoundingBoxWorld Position!
        //returns the mean distance of all objects iwth that material
        public void UpdatePositionAndCheckRender(bool cameraHasChanged, BoundingFrustum viewFrustumEx, Vector3 cameraPosition, BoundingSphere sphere)
        {
            for (var i = 0; i < _transforms.Count; i++)
            {
                TransformableObject transform = _transforms[i];

                _worldBoundingCenters[i] = Vector3.Transform(_meshBoundingSphere.Center, transform.World);

                //If either the trafomatrix or the camera has changed we need to check visibility
                if (cameraHasChanged)
                {
                    sphere.Center = _worldBoundingCenters[i];
                    sphere.Radius = _meshBoundingSphere.Radius * transform.World.M11; // previously .Scale.X;
                    if (viewFrustumEx.Contains(sphere) == ContainmentType.Disjoint)
                    {
                        _rendered[i] = false;
                    }
                    else
                    {
                        _rendered[i] = true;
                    }

                }

            }

        }

        //Basically no chance we have the same model already. We should be fine just adding it to the list if we did everything else right.
        public void Register(TransformableObject transform)
        {
            _transforms.Add(transform);
            _rendered.Add(true);
            _worldBoundingCenters.Add(Vector3.Transform(_meshBoundingSphere.Center, transform.World));
        }

        public bool DeleteFromRegistry(TransformableObject toDelete)
        {
            int index = _transforms.IndexOf(toDelete);
            if (index != -1)
            {
                _transforms.RemoveAt(index);
                _rendered.RemoveAt(index);
                _worldBoundingCenters.RemoveAt(index);
            }
            if (_transforms.Count <= 0) return true; //this meshtype no longer needed!
            return false;
        }

        public void SetBoundingSphere(BoundingSphere boundingSphere)
        {
            _meshBoundingSphere = boundingSphere;
        }
    }
}
