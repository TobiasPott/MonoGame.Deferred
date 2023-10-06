using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public class EnvironmentProbe : TransformableObject
    {
        public bool NeedsUpdate = true;

        public float SpecularStrength = 1;
        public float DiffuseStrength = 0.2f;

        public bool AutoUpdate = true;

        public bool UseSDFAO = false;


        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                base.Position = value;
                if(AutoUpdate)
                    NeedsUpdate = true;
            }
        }


        public EnvironmentProbe(Vector3 position)
        {
            Position = position;
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
        }

        public void Update()
        {
            NeedsUpdate = true;
        }
    }
    
}
