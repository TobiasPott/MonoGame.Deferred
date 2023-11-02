
//#define FORWARDONLY

using DeferredEngine.Pipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Recources
{
    public static class Names
    {
        public const string Albedo = nameof(Albedo);
        public const string Normal = nameof(Normal);
        public const string Roughness = nameof(Roughness);
        public const string Metallic = nameof(Metallic);
        public const string Mask = nameof(Mask);
        public const string Displacement = nameof(Displacement);
    }

    public class MaterialBase : Effect, IEquatable<MaterialBase>
    {

        public enum MaterialTypes
        {
            Basic = 0,
            ForwardShaded = 1,
            Emissive = 2,
        }

        private Dictionary<string, Texture2D> _maps = new Dictionary<string, Texture2D>();
        private void SetMap(string name, Texture2D map)
        {
            if (!_maps.ContainsKey(name))
            {
                if (map != null)
                    _maps.Add(name, map);
            }
            else
            {
                if (map == null)
                    _maps.Remove(name);
                else _maps[name] = map;
            }
        }

        private MaterialTypes _type = MaterialTypes.Basic;
        public int MaterialTypeNumber;
        public bool RenderCCW = false;
        public bool HasShadow = true; // Unclear if this means cast and/or receive
        public bool IsTransparent = false;


        private Vector3 _baseColor = new Vector3(0.5f, 0.5f, 0.5f);
        public Color BaseColor { get => new Color(_baseColor); set => _baseColor = value.ToVector3(); }
        public Vector3 BaseColorV3 { get => _baseColor; set => _baseColor = value; }


        private float _roughness = 0.5f;
        public float Roughness { get => _roughness; set => _roughness = Math.Max(value, 0.001f); }

        private float _metallic = 0.0f;
        public float Metallic { get => _metallic; set => _metallic = Math.Clamp(value, 0.0f, 1.0f); }

        private float _emissiveStrength = 0.0f;
        public float EmissiveStrength { get => _emissiveStrength; set => _emissiveStrength = value; }



        public bool HasAlbedoMap => _maps.ContainsKey(Names.Albedo);
        public bool HasNormalMap => _maps.ContainsKey(Names.Normal);

        public bool HasRoughnessMap => _maps.ContainsKey(Names.Roughness);
        public bool HasMetallicMap => _maps.ContainsKey(Names.Metallic);

        public bool HasMask => _maps.ContainsKey(Names.Mask);
        public bool HasDisplacementMap => _maps.ContainsKey(Names.Displacement);


        public Texture2D AlbedoMap { get => _maps[Names.Albedo]; set => SetMap(Names.Albedo, value); }
        public Texture2D NormalMap { get => _maps[Names.Normal]; set => SetMap(Names.Normal, value); }
        public Texture2D RoughnessMap { get => _maps[Names.Roughness]; set => SetMap(Names.Roughness, value); }
        public Texture2D MetallicMap { get => _maps[Names.Metallic]; set => SetMap(Names.Metallic, value); }
        public Texture2D Mask { get => _maps[Names.Mask]; set => SetMap(Names.Mask, value); }
        public Texture2D DisplacementMap { get => _maps[Names.Displacement]; set => SetMap(Names.Displacement, value); }

        public MaterialTypes Type
        {
            get { return _type; }
            set
            {
                _type = value;
                MaterialTypeNumber = (int)value;
            }
        }


        public MaterialBase(Effect sourceEffect) : base(sourceEffect)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
        }
        public MaterialBase(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        { }
        public MaterialBase(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        { }
        public MaterialBase(Effect sourceEffect, Color baseColor, float roughness, float metalness,
    MaterialTypes type = MaterialTypes.Basic) : base(sourceEffect)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
            this.Initialize(baseColor, roughness, metalness, null, null, null, null, null, null, type);
        }
        public MaterialBase(Effect sourceEffect, Color baseColor,
            Texture2D albedoMap = null, Texture2D normalMap = null,
            Texture2D roughnessMap = null, Texture2D metallicMap = null,
            MaterialTypes type = MaterialTypes.Basic) : base(sourceEffect)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
            this.Initialize(baseColor, albedoMap, normalMap, roughnessMap, metallicMap, type);
        }

        public void Initialize(Color baseColor, float roughness, float metalness,
    MaterialTypes type = MaterialTypes.Basic)
        {
            this.Initialize(baseColor, roughness, metalness, null, null, null, null, null, null, type, 0);
        }
        public void Initialize(Color baseColor,
            Texture2D albedoMap = null, Texture2D normalMap = null,
            Texture2D roughnessMap = null, Texture2D metallicMap = null,
            MaterialTypes type = MaterialTypes.Basic)
        {
            this.Initialize(baseColor, 0.001f, 0, albedoMap, normalMap, roughnessMap, metallicMap, null, null, type, 0);
        }
        public void Initialize(Color baseColor, float roughness, float metalness,
            Texture2D albedoMap = null, Texture2D normalMap = null,
            Texture2D roughnessMap = null, Texture2D metallicMap = null,
            Texture2D mask = null, Texture2D displacementMap = null,
            MaterialTypes type = MaterialTypes.Basic, float emissiveStrength = 0)
        {
            Type = type;
            _baseColor = baseColor.ToVector3();
            _roughness = roughness;
            _metallic = metalness;

            this.SetMap(Names.Albedo, albedoMap);
            this.SetMap(Names.Normal, normalMap);

            this.SetMap(Names.Roughness, roughnessMap);
            this.SetMap(Names.Metallic, metallicMap);

            this.SetMap(Names.Mask, mask);
            this.SetMap(Names.Displacement, displacementMap);

            //Type = MaterialTypes.Emissive;
            _emissiveStrength = Math.Clamp(emissiveStrength, 0, float.MaxValue);
            //if (_emissiveStrength > 0)
            //    Type = MaterialTypes.Emissive;

#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif

        }

        public void SetGBufferForMaterial(GBufferFxSetup fxSetup)
        {
            if (RenderingSettings.d_DefaultMaterial)
            {
                fxSetup.Param_Material_DiffuseColor.SetValue(Color.Gray.ToVector3());
                fxSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_DefaultRoughness > 0
                                                                                        ? RenderingSettings.m_DefaultRoughness
                                                                                        : 0.3f);
                fxSetup.Param_Material_Metallic.SetValue(0.0f);
                fxSetup.Param_Material_MaterialType.SetValue(0);
                fxSetup.Effect_GBuffer.CurrentTechnique = fxSetup.Technique_DrawBasic;
            }
            else
            {
                fxSetup.Param_Material_AlbedoMap.SetValue(this.HasAlbedoMap ? this.AlbedoMap : null);
                fxSetup.Param_Material_NormalMap.SetValue(this.HasNormalMap ? this.NormalMap : null);
                fxSetup.Param_Material_RoughnessMap.SetValue(this.HasRoughnessMap ? this.RoughnessMap : null);
                fxSetup.Param_Material_MetallicMap.SetValue(this.HasMetallicMap ? this.MetallicMap : null);
                fxSetup.Param_Material_MaskMap.SetValue(this.HasMask ? this.Mask : null);

                fxSetup.Effect_GBuffer.CurrentTechnique = this.GetGBufferTechnique(fxSetup);

                // -------------------------
                // Set value base material parameters
                if (!this.HasAlbedoMap)
                {
                    if (this.Type == MaterialBase.MaterialTypes.Emissive && this.EmissiveStrength > 0)
                    {
                        fxSetup.Param_Material_DiffuseColor.SetValue(this.BaseColor.ToVector3());
                        fxSetup.Param_Material_Metallic.SetValue(this.EmissiveStrength / 8);
                    }
                    else
                    {
                        fxSetup.Param_Material_DiffuseColor.SetValue(this.BaseColor.ToVector3());
                    }
                }

                if (!this.HasRoughnessMap)
                    fxSetup.Param_Material_Roughness.SetValue(RenderingSettings.m_DefaultRoughness > 0
                                                                                            ? RenderingSettings.m_DefaultRoughness
                                                                                            : this.Roughness);
                fxSetup.Param_Material_Metallic.SetValue(this.Metallic);
                fxSetup.Param_Material_MaterialType.SetValue(this.MaterialTypeNumber);
            }
        }

        public EffectTechnique GetGBufferTechnique(GBufferFxSetup effectSetup)
        {
            if (HasDisplacementMap)
            {
                return effectSetup.Technique_DrawDisplacement;
            }
            else if (HasMask) //Has diffuse for sure then
            {
                if (HasNormalMap && HasRoughnessMap)
                    return effectSetup.Technique_DrawTextureSpecularNormalMask;
                else if (HasNormalMap)
                    return effectSetup.Technique_DrawTextureNormalMask;
                else if (HasRoughnessMap)
                    return effectSetup.Technique_DrawTextureSpecularMask;
                else
                    return effectSetup.Technique_DrawTextureSpecularMask;
            }
            else
            {
                if (HasNormalMap && HasRoughnessMap && HasAlbedoMap && HasMetallicMap)
                    return effectSetup.Technique_DrawTextureSpecularNormalMetallic;
                else if (HasNormalMap && HasRoughnessMap && HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureSpecularNormal;
                else if (HasNormalMap && HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureNormal;
                else if (HasMetallicMap && HasRoughnessMap && HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureSpecularMetallic;
                else if (HasRoughnessMap && HasAlbedoMap)
                    return effectSetup.Technique_DrawTextureSpecular;
                else if (HasNormalMap && !HasAlbedoMap)
                    return effectSetup.Technique_DrawNormal;
                else if (HasAlbedoMap)
                    return effectSetup.Technique_DrawTexture;
            }

            return effectSetup.Technique_DrawBasic;
        }


        public bool Equals(MaterialBase b)
        {
            if (b == null) return false;
            if (b._maps.Count != _maps.Count) return false;
            return b == this;
        }

    }
}
