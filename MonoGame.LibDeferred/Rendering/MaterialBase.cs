
//#define FORWARDONLY

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Recources
{
    public class MaterialBase : Effect, IEquatable<MaterialBase>
    {
        public enum MaterialTypes
        {
            Basic = 0,
            ForwardShaded = 1,
            Emissive = 2,
        }


        private Texture2D _albedoMap;
        private Texture2D _normalMap;

        private Texture2D _roughnessMap;
        private Texture2D _metallicMap;

        private Texture2D _mask;
        private Texture2D _displacementMap;

        public bool IsTransparent = false;


        public Vector3 DiffuseColor = Color.Gray.ToVector3();

        private float _roughness = 0.5f;
        private float _metallic;
        private float _emissiveStrength;

        private MaterialTypes _type = MaterialTypes.Basic;
        public int MaterialTypeNumber;
        public bool RenderCCW = false;
        public bool HasShadow = true; // Unclear if this means cast and/or receive


        public float Roughness { get => _roughness; set => _roughness = Math.Max(value, 0.001f); }
        public float Metallic { get => _metallic; set => _metallic = Math.Clamp(value, 0.0f, 1.0f); }
        public float EmissiveStrength { get => _emissiveStrength; set => _emissiveStrength = value; }



        public bool HasAlbedoMap { get => _albedoMap != null; }
        public bool HasNormalMap { get => _normalMap != null; }

        public bool HasRoughnessMap { get => _roughnessMap != null; }
        public bool HasMetallicMap { get => _metallicMap != null; }

        public bool HasMask { get => _mask != null; }
        public bool HasDisplacementMap { get => _displacementMap != null; }


        public Texture2D AlbedoMap { get => _albedoMap; set => _albedoMap = value; }
        public Texture2D RoughnessMap { get => _roughnessMap; set => _roughnessMap = value; }
        public Texture2D MetallicMap { get => _metallicMap; set => _metallicMap = value; }
        public Texture2D NormalMap { get => _normalMap; set => _normalMap = value; }
        public Texture2D Mask { get => _mask; set => _mask = value; }
        public Texture2D DisplacementMap { get => _displacementMap; set => _displacementMap = value; }

        public MaterialTypes Type
        {
            get { return _type; }
            set
            {
                _type = value;
                MaterialTypeNumber = (int)value;
            }
        }


        public MaterialBase(Effect cloneSource) : base(cloneSource)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
        }
        public MaterialBase(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        { }
        public MaterialBase(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        { }


        public void Initialize(Color diffuseColor, float roughness, float metalness,
            Texture2D albedoMap = null, Texture2D normalMap = null,
            Texture2D roughnessMap = null, Texture2D metallicMap = null,
            Texture2D mask = null, Texture2D displacementMap = null,
            MaterialTypes type = MaterialTypes.Basic, float emissiveStrength = 0)
        {
            DiffuseColor = diffuseColor.ToVector3();
            Roughness = roughness;
            _metallic = metalness;

            _albedoMap = albedoMap;
            _normalMap = normalMap;

            _roughnessMap = roughnessMap;
            _metallicMap = metallicMap;

            _mask = mask;
            _displacementMap = displacementMap;

            Type = type;

#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif

            if (emissiveStrength > 0)
            {
                //Type = MaterialTypes.Emissive;
                _emissiveStrength = emissiveStrength;
            }
        }


        public bool Equals(MaterialBase b)
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

            if (Math.Abs(_metallic - b._metallic) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (NormalMap != b.NormalMap) return false;

            return true;
        }

    }
}
