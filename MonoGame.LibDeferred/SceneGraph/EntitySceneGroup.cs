using DeferredEngine.Pipeline.Lighting;

namespace DeferredEngine.Entities
{
    /// <summary>
    /// primitive stuct to group entities (basic & model), directional & point lights, decals and potential other entity type lists for easier access inside the pipeline
    /// </summary>
    public struct EntitySceneGroup
    {
        public readonly List<ModelEntity> Entities;
        public readonly List<DeferredPointLight> PointLights;
        public readonly List<DeferredDirectionalLight> DirectionalLights;
        public readonly List<Decal> Decals;
        public readonly EnvironmentProbe EnvProbe;

        public EntitySceneGroup(List<ModelEntity> entities, List<DeferredDirectionalLight> directionalLights, List<DeferredPointLight> pointLights, List<Decal> decals, EnvironmentProbe envProbe)
        {
            this.Entities = entities;
            this.DirectionalLights = directionalLights;
            this.PointLights = pointLights;
            this.Decals = decals;
            this.EnvProbe = envProbe;
        }
    }
}
