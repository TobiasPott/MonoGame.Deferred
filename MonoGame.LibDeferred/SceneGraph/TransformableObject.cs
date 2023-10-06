using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public abstract class TransformableObject
    {
        public virtual int Id { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual string Name { get; set; }

        public abstract Vector3 Position { get; set; }
        public abstract Matrix RotationMatrix { get; set; }
        public abstract Vector3 Scale { get; set; }

    }
}