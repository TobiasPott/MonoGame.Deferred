
//#define FORWARDONLY

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Recources
{
    public class MaterialEffect : Effect, IEquatable<MaterialEffect>
    {
        private Texture2D _albedoMap;
        private Texture2D _roughnessMap;
        private Texture2D _mask;
        private Texture2D _normalMap;
        private Texture2D _metallicMap;
        private Texture2D _displacementMap;

        public bool IsTransparent = false;

        public bool HasShadow = true;


        public Vector3 DiffuseColor = Color.Gray.ToVector3();

        private float _roughness = 0.5f;

        public float Metallic;
        public float EmissiveStrength;

        public float Roughness { get { return _roughness; } set { _roughness = Math.Max(value, 0.001f); } }

        public bool HasAlbedoMap { get; protected set; }
        public bool HasRoughnessMap { get; protected set; }
        public bool HasMask { get; protected set; }
        public bool HasNormalMap { get; protected set; }
        public bool HasMetallicMap { get; protected set; }
        public bool HasDisplacementMap { get; protected set; }


        public Texture2D AlbedoMap
        {
            get { return _albedoMap; }
            set
            {
                _albedoMap = value;
                HasAlbedoMap = _albedoMap != null;
            }
        }

        public Texture2D RoughnessMap
        {
            get { return _roughnessMap; }
            set
            {
                _roughnessMap = value;
                HasRoughnessMap = _albedoMap != null;
            }
        }

        public Texture2D MetallicMap
        {
            get { return _metallicMap; }
            set
            {
                _metallicMap = value;
                HasMetallicMap = _metallicMap != null;
            }
        }

        public Texture2D NormalMap
        {
            get { return _normalMap; }
            set
            {
                _normalMap = value;
                HasNormalMap = _normalMap != null;
            }
        }

        public Texture2D DisplacementMap
        {
            get { return _displacementMap; }
            set
            {
                _displacementMap = value;
                HasDisplacementMap = _displacementMap != null;
            }
        }

        public Texture2D Mask
        {
            get { return _mask; }
            set
            {
                _mask = value;
                HasMask = _mask != null;
            }
        }

        private MaterialTypes _type = MaterialTypes.Basic;
        public int MaterialTypeNumber;
        public bool RenderCClockwise = false;

        public enum MaterialTypes
        {
            Basic = 0,
            ForwardShaded = 1, // ToDo: Values related to shader code?!
            Emissive = 2,
        }

        public MaterialTypes Type
        {
            get { return _type; }
            set
            {
                _type = value;
                MaterialTypeNumber = (int)value;
            }
        }


        public void Initialize(Color diffuseColor, float roughness, float metalness, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, Texture2D displacementMap = null, MaterialTypes type = MaterialTypes.Basic, float emissiveStrength = 0)
        {
            DiffuseColor = diffuseColor.ToVector3();
            Roughness = roughness;
            Metallic = metalness;

            AlbedoMap = albedoMap;
            NormalMap = normalMap;
            RoughnessMap = roughnessMap;
            MetallicMap = metallicMap;
            DisplacementMap = displacementMap;
            Mask = mask;

            Type = type;

#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif

            if (emissiveStrength > 0)
            {
                //Type = MaterialTypes.Emissive;
                EmissiveStrength = emissiveStrength;
            }
        }

        public MaterialEffect(Effect cloneSource) : base(cloneSource)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
        }

        public MaterialEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        { }

        public MaterialEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        { }


        public bool Equals(MaterialEffect b)
        {
            if (b == null) return false;

            if (HasAlbedoMap != b.HasAlbedoMap) return false;

            if (HasRoughnessMap != b.HasRoughnessMap) return false;

            if (IsTransparent != b.IsTransparent) return false;

            if (HasMask != b.HasMask) return false;

            if (HasNormalMap != b.HasNormalMap) return false;

            if (HasShadow != b.HasShadow) return false;

            if (HasDisplacementMap != b.HasDisplacementMap) return false;

            if (Vector3.DistanceSquared(DiffuseColor, b.DiffuseColor) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (Type != b.Type) return false;

            if (Math.Abs(Roughness - b.Roughness) > 0.01f) return false;

            if (Math.Abs(Metallic - b.Metallic) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (NormalMap != b.NormalMap) return false;

            return true;
        }

    }
}
