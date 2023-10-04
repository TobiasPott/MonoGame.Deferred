using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUutilities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework.Graphics;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Quaternion = BEPUutilities.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{

    public sealed class BasicEntity : EntityBase
    {
        //Avoid nesting, but i could also just provide the ModelDefinition instead
        public readonly ModelDefinition ModelDefinition;
        public readonly Model Model;
        public readonly SignedDistanceField SignedDistanceField;
        public readonly MaterialEffect Material;

        private Entity _dynamicPhysicsObject;
        public StaticMesh StaticPhysicsObject = null;

        public override TransformableObject Clone {
            get
            {
                //Not very clean...
                return new BasicEntity(ModelDefinition, Material, Position, RotationMatrix, Scale );   
            }  
        }

        public BasicEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale, MeshMaterialLibrary library = null, Entity physicsObject = null)
            : base(modelbb.BoundingBox, modelbb.BoundingBoxOffset)
        {
            ModelDefinition = modelbb;
            Model = modelbb.Model;
            SignedDistanceField = modelbb.SDF;
            
            Material = material;
            Position = position;
            Scale = scale;
            
            RotationMatrix = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                  Matrix.CreateRotationZ((float)angleZ);

            if (library != null)
                RegisterInLibrary(library);

            if (physicsObject != null)
                RegisterPhysics(physicsObject);

            WorldTransform.World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            WorldTransform.Scale = Scale;
            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
        }

        public BasicEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, Matrix rotationMatrix, Vector3 scale)
            :base(modelbb.BoundingBox, modelbb.BoundingBoxOffset)
        {
            Model = modelbb.Model;
            ModelDefinition = modelbb;
            SignedDistanceField = modelbb.SDF;

            Material = material;
            Position = position;
            RotationMatrix = rotationMatrix;
            Scale = scale;
            RotationMatrix = rotationMatrix;

            WorldTransform.World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            WorldTransform.Scale = Scale;
            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
        }

        public void RegisterInLibrary(MeshMaterialLibrary library)
        {
            library.Register(Material, Model, WorldTransform);
        }

        private void RegisterPhysics(Entity physisEntity)
        {
            _dynamicPhysicsObject = physisEntity;
            _dynamicPhysicsObject.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
        }

        public void Dispose(MeshMaterialLibrary library)
        {
            library.DeleteFromRegistry(this);
        }

        public void ApplyTransformation()
        {
            if (_dynamicPhysicsObject == null)
            {
                //RotationMatrix = Matrix.CreateRotationX((float) AngleX)*Matrix.CreateRotationY((float) AngleY)*
                //                  Matrix.CreateRotationZ((float) AngleZ);
                Matrix scaleMatrix = Matrix.CreateScale(Scale);
                _worldOldMatrix = scaleMatrix* RotationMatrix * Matrix.CreateTranslation(Position);

                WorldTransform.Scale = Scale;
                WorldTransform.World = _worldOldMatrix;

                WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
                
                if (StaticPhysicsObject != null && !RenderingSettings.e_enableeditor)
                {
                    AffineTransform change = new AffineTransform(
                            new BEPUutilities.Vector3(Scale.X, Scale.Y, Scale.Z),
                            Quaternion.CreateFromRotationMatrix(MathConverter.Convert(RotationMatrix)),
                            MathConverter.Convert(Position));

                    if (!MathConverter.Equals(change.Matrix, StaticPhysicsObject.WorldTransform.Matrix))
                    {
                        //StaticPhysicsMatrix = MathConverter.Copy(Change.Matrix);

                        StaticPhysicsObject.WorldTransform = change;
                    }
                }
            }
            else
            {
                //Has something changed?
                WorldTransform.Scale = Scale;
                _worldOldMatrix = Extensions.CopyFromBepuMatrix(_worldOldMatrix, _dynamicPhysicsObject.WorldTransform);
                Matrix scaleMatrix = Matrix.CreateScale(Scale);
                //WorldOldMatrix = Matrix.CreateScale(Scale)*WorldOldMatrix; 
                WorldTransform.World = scaleMatrix * _worldOldMatrix;

                WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));

            }
        }

        internal void CheckPhysics()
        {
            if (_dynamicPhysicsObject == null) return;

            _worldNewMatrix = Extensions.CopyFromBepuMatrix(_worldNewMatrix, _dynamicPhysicsObject.WorldTransform);

            if (_worldNewMatrix != _worldOldMatrix)
            {
                WorldTransform.HasChanged = true;
                _worldOldMatrix = _worldNewMatrix;
                Position = _worldOldMatrix.Translation;
            }
            else
            {
                if (Position != _worldNewMatrix.Translation && RenderingSettings.e_enableeditor)
                {
                    //DynamicPhysicsObject.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
                    _dynamicPhysicsObject.Position = MathConverter.Convert(Position);
                }
                //    DynamicPhysicsObject.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
                //    //WorldNewMatrix = Extensions.CopyFromBepuMatrix(WorldNewMatrix, DynamicPhysicsObject.WorldTransform);
                //    //if (Position != WorldNewMatrix.Translation)
                //    //{
                //    //    var i = 0;
                //    //}

                //}
                //else
                //{
                //    //Position = WorldOldMatrix.Translation;
                //}
            }

            
        }
    }
}
