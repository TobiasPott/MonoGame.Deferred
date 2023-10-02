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
        private MaterialEffect _material;

        //Determines how many different meshes we have per texture. 
        const int InitialLibrarySize = 2;
        private MeshLibrary[] _meshLib = new MeshLibrary[InitialLibrarySize];
        public int Index;

        //efficiency: REnder front to back. So we must know the distance!

        public float DistanceSquared;
        public bool HasChangedThisFrame = true;

        public void SetMaterial(ref MaterialEffect mat)
        {
            _material = mat;
        }

        public bool HasMaterial(MaterialEffect mat)
        {
            if (!GameSettings.g_batchbymaterial) return false;
            return mat.Equals(_material);
        }

        public MaterialEffect GetMaterial()
        {
            return _material;
        }

        public MeshLibrary[] GetMeshLibrary()
        {
            return _meshLib;
        }

        public void Register(ModelMeshPart mesh, TransformMatrix worldMatrix, BoundingSphere boundingSphere)
        {
            bool found = false;
            //Check if we already have a model like that, if yes put it in there!
            for (var i = 0; i < Index; i++)
            {
                MeshLibrary meshLib = _meshLib[i];
                if (meshLib.HasMesh(mesh))
                {
                    meshLib.Register(worldMatrix);
                    found = true;
                    break;
                }
            }

            //We have no Lib yet, make a new one.
            if (!found)
            {
                _meshLib[Index] = new MeshLibrary();
                _meshLib[Index].SetMesh(mesh);
                _meshLib[Index].SetBoundingSphere(boundingSphere);
                _meshLib[Index].Register(worldMatrix);
                Index++;
            }

            //If we exceeded our array length, make the array bigger.
            if (Index >= _meshLib.Length)
            {
                MeshLibrary[] tempLib = new MeshLibrary[Index + 1];
                _meshLib.CopyTo(tempLib, 0);
                _meshLib = tempLib;
            }
        }

        public bool DeleteFromRegistry(ModelMeshPart mesh, TransformMatrix worldMatrix)
        {
            for (var i = 0; i < Index; i++)
            {
                MeshLibrary meshLib = _meshLib[i];
                if (meshLib.HasMesh(mesh))
                {
                    if (meshLib.DeleteFromRegistry(worldMatrix)) //if true, we can delete it from registry
                    {
                        for (var j = i; j < Index - 1; j++)
                        {
                            //slide down one
                            _meshLib[j] = _meshLib[j + 1];

                        }
                        Index--;
                    }
                    break;
                }
            }
            if (Index <= 0) return true; //this material is no longer needed.
            return false;
        }
    }
}
