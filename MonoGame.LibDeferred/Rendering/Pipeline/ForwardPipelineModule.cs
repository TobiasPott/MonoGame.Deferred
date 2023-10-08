//#define SHOWTILES

using DeferredEngine.Entities;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class ForwardPipelineModule : RenderingPipelineModule, IRenderModule
    {
        private const int MAXLIGHTS = 40;
        private const int MAXLIGHTSPERTILE = 40;

        private Effect Effect;

        private EffectPass Pass_Default;

        private EffectParameter Param_World;
        private EffectParameter Param_WorldViewProj;
        private EffectParameter Param_WorldViewIT;
        private EffectParameter Param_CameraPositionWS;

        private EffectParameter Param_LightAmount;
        private EffectParameter Param_LightPositionWS;
        private EffectParameter Param_LightRadius;
        private EffectParameter Param_LightIntensity;
        private EffectParameter Param_LightColor;

        private EffectParameter Param_TiledListLength;


        private Vector3[] LightPositionWS;
        private float[] LightRadius;
        private float[] LightIntensity;
        private Vector3[] LightColor;

        private int[][] TiledList;
        private float[] TiledListLength;
        private BoundingFrustumEx _tileFrustum;
        private Vector3[] _tileFrustumCorners = new Vector3[8];

        public ForwardPipelineModule(ContentManager content, string shaderPath = "Shaders/forward/forward")
            : base(content, shaderPath)
        { }

        //public override void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        //{
        //    base.Initialize(graphicsDevice, spriteBatch);
        //}
        protected override void Load(ContentManager content, string shaderPath = "Shaders/forward/forward")
        {
            Effect = content.Load<Effect>(shaderPath);

            Param_World = Effect.Parameters["World"];
            Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            Param_WorldViewIT = Effect.Parameters["WorldViewIT"];

            Param_LightAmount = Effect.Parameters["LightAmount"];

            Param_LightPositionWS = Effect.Parameters["LightPositionWS"];
            Param_LightRadius = Effect.Parameters["LightRadius"];
            Param_LightIntensity = Effect.Parameters["LightIntensity"];
            Param_LightColor = Effect.Parameters["LightColor"];

            Param_TiledListLength = Effect.Parameters["TiledListLength"];

            Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];

            Pass_Default = Effect.Techniques["Default"].Passes[0];
        }


        /// <summary>
        /// Draw forward shaded, alpha blended materials. Very basic and unoptimized algorithm. Can be improved to use tiling in future.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="meshMat"></param>
        /// <param name="viewProjection"></param>
        /// <param name="camera"></param>
        /// <param name="pointLights"></param>
        /// <param name="frustum"></param>
        /// <returns></returns>
        public RenderTarget2D Draw(RenderTarget2D output,
            MeshMaterialLibrary meshMat,
            Matrix viewProjection,
            Camera camera,
            List<DeferredPointLight> pointLights,
            BoundingFrustum frustum)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            SetupLighting(camera, pointLights, frustum);

            //TiledLighting(frustum, pointLights, 20, 10);

            meshMat.Draw(MeshMaterialLibrary.RenderType.Forward, viewProjection, renderModule: this);

            return output;
        }

        private void TiledLighting(BoundingFrustum frustum, List<DeferredPointLight> pointLights, int cols, int rows)
        {
            if (TiledList == null || TiledList.Length != cols * rows)
            {
                TiledList = new int[cols * rows][];
                TiledListLength = new float[cols * rows];

                for (var index = 0; index < TiledList.Length; index++)
                {
                    TiledList[index] = new int[MAXLIGHTSPERTILE];
                }
            }

            if (_tileFrustum == null)
                _tileFrustum = new BoundingFrustumEx(frustum.Matrix);

            Vector3[] mainfrustumCorners = frustum.GetCorners();

            for (float col = 0; col < cols; col++)
            {
                for (float row = 0; row < rows; row++)
                {

                    //top left
                    _tileFrustumCorners[0] = mainfrustumCorners[0]
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * col / cols
                        + (mainfrustumCorners[3] - mainfrustumCorners[0]) * row / rows;

                    //top right
                    _tileFrustumCorners[1] = mainfrustumCorners[0]
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * (col + 1) / cols
                        + (mainfrustumCorners[3] - mainfrustumCorners[0]) * row / rows;


                    //bot right
                    _tileFrustumCorners[2] = mainfrustumCorners[0]
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * (col + 1) / cols
                        + (mainfrustumCorners[2] - mainfrustumCorners[1]) * (row + 1) / rows;

                    //bot left
                    _tileFrustumCorners[3] = mainfrustumCorners[0]
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * (col) / cols
                        + (mainfrustumCorners[2] - mainfrustumCorners[1]) * (row + 1) / rows;

                    _tileFrustumCorners[4] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * col / cols
                                             + (mainfrustumCorners[7] - mainfrustumCorners[4]) * row / rows;

                    _tileFrustumCorners[5] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * (col + 1) / cols
                                             + (mainfrustumCorners[7] - mainfrustumCorners[4]) * row / rows;


                    _tileFrustumCorners[6] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * (col + 1) / cols
                                             + (mainfrustumCorners[6] - mainfrustumCorners[5]) * (row + 1) / rows;

                    _tileFrustumCorners[7] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * (col) / cols
                                             + (mainfrustumCorners[6] - mainfrustumCorners[5]) * (row + 1) / rows;

                    _tileFrustum.SetCorners(ref _tileFrustumCorners);
                    _tileFrustum.CreatePlanesFromCorners();

                    //Now we are ready to frustum cull... phew

                    int index = (int)(row * cols + col);

                    int numberOfLightsInTile = 0;

                    for (var i = 0; i < pointLights.Count; i++)
                    {
                        var pointLight = pointLights[i];
                        ContainmentType containmentType = _tileFrustum.Contains(pointLight.BoundingSphere);

                        if (containmentType == ContainmentType.Intersects ||
                            containmentType == ContainmentType.Contains)
                        {
                            TiledList[index][numberOfLightsInTile] = i;
                            numberOfLightsInTile++;
                        }

                        if (numberOfLightsInTile >= MAXLIGHTSPERTILE) break;
                    }

                    TiledListLength[index] = numberOfLightsInTile;

#if SHOWTILES
                    LineHelperManager.AddFrustum(_tileFrustum, 1,numberOfLightsInTile > 1 ? Color.Red : numberOfLightsInTile > 0 ? Color.Blue : Color.Green);
#endif
                }
            }

            //Note: This needs a custom monogame version, since the default doesn't like to pass int[];
            Param_TiledListLength.SetValue(TiledListLength);
        }

        private void SetupLighting(Camera camera, List<DeferredPointLight> pointLights, BoundingFrustum frustum)
        {
            //Setup camera
            Param_CameraPositionWS.SetValue(camera.Position);

            int count = pointLights.Count > 40 ? MAXLIGHTS : pointLights.Count;

            if (LightPositionWS == null || pointLights.Count != LightPositionWS.Length)
            {
                LightPositionWS = new Vector3[count];
                LightColor = new Vector3[count];
                LightIntensity = new float[count];
                LightRadius = new float[count];
            }

            //Fill
            int lightsInBounds = 0;

            for (var index = 0; index < count; index++)
            {
                DeferredPointLight light = pointLights[index];

                //Check frustum culling
                if (frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint) continue;

                LightPositionWS[lightsInBounds] = light.Position;
                LightColor[lightsInBounds] = light.ColorV3;
                LightIntensity[lightsInBounds] = light.Intensity;
                LightRadius[lightsInBounds] = light.Radius;
                lightsInBounds++;
            }

            Param_LightAmount.SetValue(lightsInBounds);

            Param_LightPositionWS.SetValue(LightPositionWS);
            Param_LightColor.SetValue(LightColor);
            Param_LightIntensity.SetValue(LightIntensity);
            Param_LightRadius.SetValue(LightRadius);
        }


        public override void Dispose()
        {
            Effect?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Param_World.SetValue(localWorldMatrix);
            Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
            Param_WorldViewIT.SetValue(Matrix.Transpose(Matrix.Invert(localWorldMatrix)));

            Pass_Default.Apply();
        }
    }
}
