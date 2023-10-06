using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    /// <summary>
    /// Library which has a list of meshlibraries that correspond to a material.
    /// </summary>
    public class MaterialLibrary
    {
        private const int InitialLibrarySize = 2;

        private MaterialEffect _material;

        //Determines how many different meshes we have per texture. 
        private List<MeshLibrary> _meshLib = new List<MeshLibrary>(InitialLibrarySize);

        //efficiency: REnder front to back. So we must know the distance!

        public float DistanceSquared;
        public bool HasChangedThisFrame = true;

        public int Count => _meshLib.Count;


        public void SetMaterial(MaterialEffect mat) => _material = mat;
        public bool HasMaterial(MaterialEffect mat)
        {
            if (!RenderingSettings.g_batchbymaterial) return false;
            return mat.Equals(_material);
        }
        public MaterialEffect GetMaterial() => _material;
        public List<MeshLibrary> GetMeshLibrary() =>_meshLib;


        public void Register(ModelMeshPart mesh, TransformableObject transform, BoundingSphere boundingSphere)
        {
            bool found = false;
            //Check if we already have a model like that, if yes put it in there!
            for (var i = 0; i < this.Count; i++)
            {
                MeshLibrary meshLib = _meshLib[i];
                if (meshLib.HasMesh(mesh))
                {
                    meshLib.Register(transform);
                    found = true;
                    break;
                }
            }

            //We have no Lib yet, make a new one.
            if (!found)
            {
                MeshLibrary meshLib = new MeshLibrary();
                _meshLib.Add(meshLib);
                meshLib.SetMesh(mesh);
                meshLib.SetBoundingSphere(boundingSphere);
                meshLib.Register(transform);
            }

        }

        public bool DeleteFromRegistry(ModelMeshPart mesh, TransformableObject toDelete)
        {
            for (var i = 0; i < this.Count; i++)
            {
                MeshLibrary meshLib = _meshLib[i];
                if (meshLib.HasMesh(mesh))
                {
                    if (meshLib.DeleteFromRegistry(toDelete)) //if true, we can delete it from registry
                    {
                        _meshLib.Remove(meshLib);
                    }
                    break;
                }
            }
            if (this.Count <= 0) return true; //this material is no longer needed.
            return false;
        }
    }
}
