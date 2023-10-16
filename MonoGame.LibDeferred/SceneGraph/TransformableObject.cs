namespace DeferredEngine.Entities
{
    public abstract class TransformableObject : Transform
    {
        public virtual int Id { get; set; }
        public virtual bool IsEnabled { get; set; } = true;
        public virtual string Name { get; set; }

    }
}