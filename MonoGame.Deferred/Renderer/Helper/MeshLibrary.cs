using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    //The individual model mesh, and a library or different world coordinates basically is what we need
    public class MeshLibrary
    {
        private ModelMeshPart _mesh;
        public BoundingSphere MeshBoundingSphere;

        const int InitialLibrarySize = 4;
        private TransformMatrix[] _worldMatrices = new TransformMatrix[InitialLibrarySize];

        //the local displacement of the boundingsphere!
        private Vector3[] _worldBoundingCenters = new Vector3[InitialLibrarySize];
        //the local mode - either rendered or not!
        public bool[] Rendered = new bool[InitialLibrarySize];

        public int Index;

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

        public TransformMatrix[] GetWorldMatrices()
        {
            return _worldMatrices;
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

            for (var i = 0; i < Index; i++)
            {
                TransformMatrix trafoMatrix = _worldMatrices[i];


                if (trafoMatrix.HasChanged)
                {
                    _worldBoundingCenters[i] = trafoMatrix.TransformMatrixSubModel(MeshBoundingSphere.Center);
                }

                //If either the trafomatrix or the camera has changed we need to check visibility
                if (trafoMatrix.HasChanged || cameraHasChanged)
                {
                    sphere.Center = _worldBoundingCenters[i];
                    sphere.Radius = MeshBoundingSphere.Radius * trafoMatrix.Scale.X;
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
            if (hasAnythingChanged && GameSettings.g_cpusort)
            {
                distance = 0;

                for (var i = 0; i < Index; i++)
                {
                    distance += Vector3.DistanceSquared(cameraPosition, _worldBoundingCenters[i]);
                }
            }

            return distance;
        }

        //Basically no chance we have the same model already. We should be fine just adding it to the list if we did everything else right.
        public void Register(TransformMatrix world)
        {
            _worldMatrices[Index] = world;
            Rendered[Index] = true;
            _worldBoundingCenters[Index] = world.TransformMatrixSubModel(MeshBoundingSphere.Center);

            Index++;

            //mesh.Effect = Shaders.AmbientEffect; //Just so it has few properties!

            if (Index >= _worldMatrices.Length)
            {
                TransformMatrix[] tempLib = new TransformMatrix[Index + 1];
                _worldMatrices.CopyTo(tempLib, 0);
                _worldMatrices = tempLib;

                Vector3[] tempLib2 = new Vector3[Index + 1];
                _worldBoundingCenters.CopyTo(tempLib2, 0);
                _worldBoundingCenters = tempLib2;

                bool[] tempRendered = new bool[Index + 1];
                Rendered.CopyTo(tempRendered, 0);
                Rendered = tempRendered;
            }
        }

        public bool DeleteFromRegistry(TransformMatrix worldMatrix)
        {
            for (var i = 0; i < Index; i++)
            {
                TransformMatrix trafoMatrix = _worldMatrices[i];

                if (trafoMatrix == worldMatrix)
                {
                    //delete this value!
                    for (var j = i; j < Index - 1; j++)
                    {
                        //slide down one
                        _worldMatrices[j] = _worldMatrices[j + 1];
                        Rendered[j] = Rendered[j + 1];
                        _worldBoundingCenters[j] = _worldBoundingCenters[j + 1];
                    }
                    Index--;
                    break;
                }
            }
            if (Index <= 0) return true; //this meshtype no longer needed!
            return false;
        }

        public void SetBoundingSphere(BoundingSphere boundingSphere)
        {
            MeshBoundingSphere = boundingSphere;
        }
    }
}
