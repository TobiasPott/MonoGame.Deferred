﻿using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering
{
    /// <summary>
    /// Library which has a list of meshlibraries that correspond to a material.
    /// </summary>
    public class MaterialBatch
    {
        private const int InitialLibrarySize = 2;

        public MaterialEffect Material { get; protected set; }

        //Determines how many different meshes we have per texture. 
        private List<MeshBatch> _batches = new List<MeshBatch>(InitialLibrarySize);

        //efficiency: REnder front to back. So we must know the distance!

        public float DistanceSquared;

        public int Count => _batches.Count;
        public bool IsAnyRendered => _batches.Any(x => x.IsAnyRendered);
        public MeshBatch this[int index] => _batches[index];



        public void SetMaterial(MaterialEffect mat) => Material = mat;
        public bool HasMaterial(MaterialEffect mat)
        {
            return mat.Equals(Material);
        }
        public List<MeshBatch> GetMeshLibrary() => _batches;

        public void Register(ModelMeshPart mesh, TransformableObject transform, BoundingSphere boundingSphere)
        {
            bool found = false;
            //Check if we already have a model like that, if yes put it in there!
            for (var i = 0; i < this.Count; i++)
            {
                MeshBatch meshLib = _batches[i];
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
                MeshBatch batch = new MeshBatch();
                _batches.Add(batch);
                batch.SetMesh(mesh);
                batch.SetBoundingSphere(boundingSphere);
                batch.Register(transform);
            }

        }

        public bool DeleteFromRegistry(ModelMeshPart mesh, TransformableObject toDelete)
        {
            for (var i = 0; i < this.Count; i++)
            {
                MeshBatch meshLib = _batches[i];
                if (meshLib.HasMesh(mesh))
                {
                    if (meshLib.DeleteFromRegistry(toDelete)) //if true, we can delete it from registry
                    {
                        _batches.Remove(meshLib);
                    }
                    break;
                }
            }
            if (this.Count <= 0) return true; //this material is no longer needed.
            return false;
        }
    }
}
