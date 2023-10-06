using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public abstract class TransformBase
    {
        public abstract Vector3 Position { get; set; }
        public abstract Matrix RotationMatrix { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Matrix World { get; }

    }
}