using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    /// <summary>
    /// primitive stuct to group entities (basic & model), directional & point lights, decals and potential other entity type lists for easier access inside the pipeline
    /// </summary>
    public class EntityScene
    {
        public readonly List<ModelEntity> Entities;
        public readonly List<PointLight> PointLights;
        public readonly List<DirectionalLight> DirectionalLights;
        public readonly List<Decal> Decals;
        public readonly EnvironmentProbe EnvProbe;

        public EntityScene()
        {
            this.Entities = new List<ModelEntity>();
            this.DirectionalLights = new List<DirectionalLight>();
            this.PointLights = new List<PointLight>();
            this.Decals = new List<Decal>();
            this.EnvProbe = new EnvironmentProbe(new Vector3(-45, -5, 5));
        }
        public EntityScene(List<ModelEntity> entities, List<DirectionalLight> directionalLights, List<PointLight> pointLights, List<Decal> decals, EnvironmentProbe envProbe)
        {
            this.Entities = entities;
            this.DirectionalLights = directionalLights;
            this.PointLights = pointLights;
            this.Decals = decals;
            this.EnvProbe = envProbe;
        }


        /// <summary>
        /// Create a basic rendered model with custom material
        /// </summary>
        /// <returns>returns the basicEntity we created</returns>
        public ModelEntity Add(ModelDefinition model, MaterialEffect materialEffect,
            Vector3 position, Vector3 angles, Vector3 scale, DynamicMeshBatcher batcher)
        {
            ModelEntity entity = new ModelEntity(model, materialEffect, position, angles, scale, batcher);
            this.Entities.Add(entity);

            return entity;
        }
    }
}
