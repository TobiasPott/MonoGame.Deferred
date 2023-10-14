using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DeferredEngine.Recources
{

    public class DemoAssets : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Default Meshes + Editor
        public ModelDefinition Plane;

        public ModelDefinition Cube;

        //https://sketchfab.com/models/95c4008c4c764c078f679d4c320e7b18
        public ModelDefinition Tiger;

        public ModelDefinition HumanModel;

        //Default Materials

        public MaterialEffect BaseMaterial;
        public MaterialEffect BaseMaterialGray;
        public MaterialEffect GoldMaterial;
        public MaterialEffect EmissiveMaterial;
        public MaterialEffect EmissiveMaterial2;
        public MaterialEffect EmissiveMaterial3;
        public MaterialEffect EmissiveMaterial4;
        public MaterialEffect SilverMaterial;
        public MaterialEffect HologramMaterial;
        public MaterialEffect MetalRough03Material;
        public MaterialEffect AlphaBlendRim;
        public MaterialEffect MirrorMaterial;

        //Shader stuff

        public static Texture2D BaseTex;

        //Meshes and Materials

        //public Model Trabant;
        //public MaterialEffect TrabantBigParts;

        public ModelDefinition SponzaModel;
        private readonly List<Texture2D> _sponzaTextures = new List<Texture2D>();
        private Texture2D sponza_fabric_metallic;
        private Texture2D sponza_fabric_spec;
        private Texture2D sponza_curtain_metallic;

        public Model SkullModel;

        public Model HelmetModel;

        public ModelDefinition StanfordDragon;
        public ModelDefinition StanfordDragonLowpoly;

        public MaterialEffect RockMaterial;


        public SpriteFont DefaultFont;
        public SpriteFont MonospaceFont;

        public MaterialEffect DragonLowPolyMaterial;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            //Default Meshes + Editor
            Plane = new SdfModelDefinition(content, "Art/Plane", graphicsDevice);

            Cube = new SdfModelDefinition(content, "Art/test/cube", graphicsDevice, true);

            Tiger = new SdfModelDefinition(content, "Art/Tiger/Tiger", graphicsDevice, true);
            HumanModel = new SdfModelDefinition(content, "Art/Human/human", graphicsDevice, true);

            //Default Materials

            BaseMaterial = CreateMaterialEffect(Color.Red, 0.5f, 0, type: MaterialEffect.MaterialTypes.Basic);

            BaseMaterialGray = CreateMaterialEffect(Color.LightGray, 0.8f, 0, type: MaterialEffect.MaterialTypes.Basic);

            MetalRough03Material = CreateMaterialEffect(Color.Silver, 0.2f, 1);
            AlphaBlendRim = CreateMaterialEffect(Color.Silver, 0.05f, 1, type: MaterialEffect.MaterialTypes.ForwardShaded);
            MirrorMaterial = CreateMaterialEffect(Color.White, 0.05f, 1);

            HologramMaterial = CreateMaterialEffect(Color.White, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Hologram, 1);

            EmissiveMaterial = CreateMaterialEffect(Color.White, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.5f);

            EmissiveMaterial2 = CreateMaterialEffect(Color.MonoGameOrange, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.8f);
            EmissiveMaterial3 = CreateMaterialEffect(Color.Violet, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.8f);
            EmissiveMaterial4 = CreateMaterialEffect(Color.LimeGreen, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.8f);

            GoldMaterial = CreateMaterialEffect(Color.Gold, 0.2f, 1);

            SilverMaterial = CreateMaterialEffect(Color.Silver, 0.05f, 1);

            //Shader stuff

            BaseTex = new Texture2D(graphicsDevice, 1, 1);
            BaseTex.SetData(new Color[] { Color.White });

            //Meshes and Materials
            StanfordDragon = new SdfModelDefinition(content, "Art/default/dragon_uv_smooth", graphicsDevice, false, SdfModelDefinition.DefaultSdfResolution * 1.4f);
            StanfordDragonLowpoly = new SdfModelDefinition(content, "Art/default/dragon_lowpoly", graphicsDevice, true, SdfModelDefinition.DefaultSdfResolution * 1.2f);

            DragonLowPolyMaterial = CreateMaterialEffect(Color.Red, 0.5f, 0, type: MaterialEffect.MaterialTypes.Basic, normalMap: content.Load<Texture2D>("Art/default/dragon_normal"));

            HelmetModel = content.Load<Model>("Art/default/daft_helmets");
            SkullModel = content.Load<Model>("Art/default/skull");

            //

            SponzaModel = new SdfModelDefinition(content, "Sponza/Sponza", graphicsDevice, false);
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/background_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/chain_texture_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/chain_texture_mask"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/lion_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/lion2_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/spnza_bricks_a_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/spnza_bricks_a_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_arch_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_arch_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_ceiling_a_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_column_a_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_column_a_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_column_b_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_column_b_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_column_c_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_column_c_ddn"));
            _sponzaTextures.Add(sponza_fabric_spec = content.Load<Texture2D>("Sponza/textures/sponza_fabric_spec"));
            _sponzaTextures.Add(sponza_fabric_metallic = content.Load<Texture2D>("Sponza/textures/sponza_fabric_metallic"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_green_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_blue_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_details_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_flagpole_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_thorn_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_thorn_mask"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_thorn_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/vase_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/vase_plant_mask"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/vase_plant_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/vase_round_ddn"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/vase_round_spec"));

            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_floor_a_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_floor_a_ddn"));

            sponza_curtain_metallic = content.Load<Texture2D>("Sponza/textures/sponza_curtain_metallic");

            ProcessSponza();

            ProcessHelmets();

            RockMaterial = CreateMaterialEffect(Color.White, roughness: 1, metallic: 0,
                albedoMap: content.Load<Texture2D>("Art/test/squarebricks-diffuse"),
                normalMap: content.Load<Texture2D>("Art/test/squarebricks-normal"),
                roughnessMap: null,
                metallicMap: null,
                mask: null,
                displacementMap: content.Load<Texture2D>("Art/test/squarebricks-depth")
            );

            //Fonts

            DefaultFont = content.Load<SpriteFont>("Fonts/defaultFont");
            MonospaceFont = content.Load<SpriteFont>("Fonts/monospace");

        }

        /// <summary>
        /// Create custom materials, you can add certain maps like Albedo, normal, etc. if you like.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="roughness"></param>
        /// <param name="metallic"></param>
        /// <param name="albedoMap"></param>
        /// <param name="normalMap"></param>
        /// <param name="roughnessMap"></param>
        /// <param name="metallicMap"></param>
        /// <param name="mask"></param>
        /// <param name="type">2: hologram, 3:emissive</param>
        /// <param name="emissiveStrength"></param>
        /// <returns></returns>
        private static MaterialEffect CreateMaterialEffect(Color color, float roughness, float metallic, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, Texture2D displacementMap = null, MaterialEffect.MaterialTypes type = 0, float emissiveStrength = 0)
        {
            MaterialEffect mat = new MaterialEffect(Shaders.Deferred.Effect_Clear);
            mat.Initialize(color, roughness, metallic, albedoMap, normalMap, roughnessMap, metallicMap, mask, displacementMap, type, emissiveStrength);
            return mat;
        }

        /// <summary>
        /// The helmets have many submaterials and I want specific values for each one of them!
        /// </summary>
        private void ProcessHelmets()
        {
            for (int i = 0; i < HelmetModel.Meshes.Count; i++)
            {
                ModelMesh mesh = HelmetModel.Meshes[i];
                for (int index = 0; index < mesh.MeshParts.Count; index++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[index];
                    MaterialEffect matEffect = new MaterialEffect(meshPart.Effect) { DiffuseColor = Color.Gray.ToVector3() };

                    if (mesh.Name == "Helmet1_Interior")
                    {
                        matEffect.DiffuseColor = Color.White.ToVector3();
                    }

                    if (i == 5)
                    {
                        matEffect.DiffuseColor = new Color(0, 0.49f, 0.95f).ToVector3();
                        matEffect.Type = MaterialEffect.MaterialTypes.Hologram;
                    }

                    if (i == 0)
                    {
                        matEffect.DiffuseColor = Color.Black.ToVector3();
                        matEffect.Roughness = 0.1f;
                        matEffect.Type = MaterialEffect.MaterialTypes.ProjectHologram;
                    }

                    if (i == 1)
                    {
                        matEffect.DiffuseColor = new Color(0, 0.49f, 0.95f).ToVector3();
                    }

                    if (i == 2)
                    {
                        matEffect.DiffuseColor = Color.Silver.ToVector3();
                        matEffect.Metallic = 1;
                        matEffect.Roughness = 0.1f;
                    }

                    //Helmet color - should be gold!
                    if (i == 4)
                    {
                        matEffect.DiffuseColor = new Color(255, 255, 155).ToVector3() * 0.5f;
                        matEffect.Roughness = 0.3f;
                        matEffect.Metallic = 0.8f;
                    }

                    if (i == 13)
                    {
                        matEffect.DiffuseColor = Color.Black.ToVector3();
                        matEffect.Roughness = 0.05f;
                        matEffect.Type = MaterialEffect.MaterialTypes.ProjectHologram;
                    }

                    meshPart.Effect = matEffect;
                }
            }
        }

        //Assign specific materials to submeshes
        private void ProcessSponza()
        {
            foreach (ModelMesh mesh in SponzaModel.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    MaterialEffect matEffect = new MaterialEffect(meshPart.Effect);

                    BasicEffect oEffect = meshPart.Effect as BasicEffect;

                    //I want to remove this mesh
                    if (mesh.Name == "g sponza_04")
                    {
                        //Put the boudning sphere into space?
                        mesh.BoundingSphere = new BoundingSphere(new Vector3(-100000, 0, 0), 0);

                        //Make it transparent
                        matEffect.IsTransparent = true;
                    }

                    matEffect.DiffuseColor = oEffect.DiffuseColor;

                    if (oEffect.TextureEnabled)
                    {
                        matEffect.AlbedoMap = oEffect.Texture;

                        string[] name = matEffect.AlbedoMap.Name.Split('\\');

                        string compare = name[2].Replace("_0", "");

                        if (compare.Contains("vase_round") || compare.Contains("vase_hanging"))
                        {
                            matEffect.Roughness = 0.1f;
                            matEffect.Metallic = 0.5f;
                        }

                        //Make the vases emissive!

                        //if (compare.Contains("vase_hanging"))
                        //{
                        //    matEffect.EmissiveStrength = 2;
                        //    matEffect.Type = MaterialEffect.MaterialTypes.Emissive;
                        //    matEffect.DiffuseColor = Color.Gold.ToVector3();

                        //    matEffect.AlbedoMap = null;
                        //    matEffect.HasDiffuse = false;
                        //}

                        //if (compare.Contains("floor"))
                        //{
                        //    matEffect.Roughness = 0.2f;
                        //    matEffect.Metallic = 1;
                        //    //matEffect.HasDiffuse = false;
                        //}


                        if (compare.Contains("chain"))
                        {
                            matEffect.Roughness = 0.5f;
                            matEffect.Metallic = 1f;
                        }

                        if (compare.Contains("curtain"))
                        {
                            matEffect.MetallicMap = sponza_curtain_metallic;
                        }

                        if (compare.Contains("sponza_fabric"))
                        {
                            matEffect.MetallicMap = sponza_fabric_metallic;
                            matEffect.RoughnessMap = sponza_fabric_spec;
                        }


                        if (compare.Contains("lion"))
                        {
                            matEffect.Metallic = 0.9f;
                        }

                        if (compare.Contains("_diff"))
                        {
                            compare = compare.Replace("_diff", "");
                        }

                        foreach (Texture2D tex2d in _sponzaTextures)
                        {
                            if (tex2d.Name.Contains(compare))
                            {
                                //We got a match!

                                string ending = tex2d.Name.Replace(compare, "");

                                ending = ending.Replace("Sponza/textures/", "");

                                if (ending == "_spec")
                                {
                                    matEffect.RoughnessMap = tex2d;
                                }

                                if (ending == "_metallic")
                                {
                                    matEffect.MetallicMap = tex2d;
                                }

                                if (ending == "_ddn")
                                {
                                    matEffect.NormalMap = tex2d;
                                }

                                if (ending == "_mask")
                                {
                                    matEffect.Mask = tex2d;
                                }

                            }
                        }


                    }
                    meshPart.Effect = matEffect;
                }


            }
        }

        public void Dispose()
        {
            BaseMaterial?.Dispose();
            GoldMaterial?.Dispose();
            EmissiveMaterial?.Dispose();
            EmissiveMaterial2?.Dispose();
            EmissiveMaterial3?.Dispose();
            EmissiveMaterial4?.Dispose();
            SilverMaterial?.Dispose();
            HologramMaterial?.Dispose();
            MetalRough03Material?.Dispose();
            AlphaBlendRim?.Dispose();
            MirrorMaterial?.Dispose();
            sponza_fabric_metallic?.Dispose();
            sponza_fabric_spec?.Dispose();
            sponza_curtain_metallic?.Dispose();
            RockMaterial?.Dispose();
        }
    }

}
