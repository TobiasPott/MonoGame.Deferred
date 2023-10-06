using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace DeferredEngine.Renderer.Helper
{
    //The individual model mesh, and a library or different world coordinates basically is what we need
    public class MeshLibrary
    {
        const int InitialLibrarySize = 4;


        private ModelMeshPart _mesh;
        public BoundingSphere MeshBoundingSphere;

        private List<TransformableObject> _transforms = new List<TransformableObject>(InitialLibrarySize);
        private List<Vector3> _worldBoundingCenters = new List<Vector3>(InitialLibrarySize);
        public List<bool> Rendered = new List<bool>(InitialLibrarySize);

        public int Count => _transforms.Count;

        public void SetMesh(ModelMeshPart mesh)
        {
            _mesh = mesh;
        }

        public bool HasMesh(ModelMeshPart mesh)
        {
            return mesh == _mesh;
        }

        public ModelMeshPart GetMesh()
        {
            return _mesh;
        }

        public List<TransformableObject> GetTransforms()
        {
            return _transforms;
        }

        public Vector3 GetBoundingCenterWorld(int index)
        {
            return _worldBoundingCenters[index];
        }

        //IF a submesh belongs to an entity that has moved we need to update the BoundingBoxWorld Position!
        //returns the mean distance of all objects iwth that material
        public float? UpdatePositionAndCheckRender(bool cameraHasChanged, BoundingFrustum viewFrustumEx, Vector3 cameraPosition, BoundingSphere sphere)
        {
            float? distance = null;

            bool hasAnythingChanged = false;

            for (var i = 0; i < _transforms.Count; i++)
            {
                TransformableObject transform = _transforms[i];

                _worldBoundingCenters[i] = Vector3.Transform(MeshBoundingSphere.Center, transform.World);

                //If either the trafomatrix or the camera has changed we need to check visibility
                if (cameraHasChanged)
                {
                    sphere.Center = _worldBoundingCenters[i];
                    sphere.Radius = MeshBoundingSphere.Radius * transform.Scale.X;
                    if (viewFrustumEx.Contains(sphere) == ContainmentType.Disjoint)
                    {
                        Rendered[i] = false;
                    }
                    else
                    {
                        Rendered[i] = true;
                    }

                    //we just register that something has changed
                    hasAnythingChanged = true;
                }

            }

            //We need to calcualte a new average distance
            if (hasAnythingChanged && RenderingSettings.g_cpusort)
            {
                distance = 0;

                for (var i = 0; i < _worldBoundingCenters.Count; i++)
                {
                    distance += Vector3.DistanceSquared(cameraPosition, _worldBoundingCenters[i]);
                }
            }

            return distance;
        }

        //Basically no chance we have the same model already. We should be fine just adding it to the list if we did everything else right.
        public void Register(TransformableObject transform)
        {
            _transforms.Add(transform);
            Rendered.Add(true);
            _worldBoundingCenters.Add(Vector3.Transform(MeshBoundingSphere.Center, transform.World));
        }

        public bool DeleteFromRegistry(TransformableObject toDelete)
        {
            int index = _transforms.IndexOf(toDelete);
            if (index != -1)
            {
                Debug.WriteLine("DeleteFrom MeshLibrary: " + index);
                _transforms.RemoveAt(index);
                Rendered.RemoveAt(index);
                _worldBoundingCenters.RemoveAt(index);
            }
            if (_transforms.Count <= 0) return true; //this meshtype no longer needed!
            return false;
        }

        public void SetBoundingSphere(BoundingSphere boundingSphere)
        {
            MeshBoundingSphere = boundingSphere;
        }
    }
}
