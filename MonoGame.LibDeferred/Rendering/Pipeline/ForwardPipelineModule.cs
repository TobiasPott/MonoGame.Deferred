//#define SHOWTILES

using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class ForwardPipelineModule : PipelineModule, IRenderModule
    {
        private const int MAXLIGHTS = 40;
        private const int MAXLIGHTSPERTILE = 40;

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
        { }

        /// <summary>
        /// Draw forward shaded, alpha blended materials. Very basic and unoptimized algorithm. Can be improved to use tiling in future.
        /// </summary>
        public RenderTarget2D Draw(DynamicMeshBatcher meshMat, RenderTarget2D output, Matrix viewProjection)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshMat.Draw(DynamicMeshBatcher.RenderType.Forward, viewProjection, renderModule: this);

            return output;
        }

        public void PrepareDraw(Camera camera, List<DeferredPointLight> pointLights, BoundingFrustum frustum)
        {
            SetupLighting(camera, pointLights, frustum);

            //TiledLighting(frustum, pointLights, 20, 10);
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
            Shaders.Forward.Param_TiledListLength.SetValue(TiledListLength);
        }
        private void SetupLighting(Camera camera, List<DeferredPointLight> pointLights, BoundingFrustum frustum)
        {
            //Setup camera
            Shaders.Forward.Param_CameraPositionWS.SetValue(camera.Position);

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

            Shaders.Forward.Param_LightAmount.SetValue(lightsInBounds);
            Shaders.Forward.Param_LightPositionWS.SetValue(LightPositionWS);
            Shaders.Forward.Param_LightColor.SetValue(LightColor);
            Shaders.Forward.Param_LightIntensity.SetValue(LightIntensity);
            Shaders.Forward.Param_LightRadius.SetValue(LightRadius);
        }


        public override void Dispose()
        {
            Shaders.Forward.Effect?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            Shaders.Forward.Param_World.SetValue(localWorldMatrix);
            Shaders.Forward.Param_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
            Shaders.Forward.Param_WorldViewIT.SetValue(Matrix.Transpose(Matrix.Invert(localWorldMatrix)));
            Shaders.Forward.Pass_Default.Apply();
        }
    }
}

namespace DeferredEngine.Recources
{
    public static partial class Shaders
    {

        // Forward
        public static class Forward
        {
            public static Effect Effect = ShaderGlobals.content.Load<Effect>("Shaders/forward/forward");

            public static EffectPass Pass_Default = Effect.Techniques["Default"].Passes[0];

            public static EffectParameter Param_World = Effect.Parameters["World"];
            public static EffectParameter Param_WorldViewProj = Effect.Parameters["WorldViewProj"];
            public static EffectParameter Param_WorldViewIT = Effect.Parameters["WorldViewIT"];
            public static EffectParameter Param_LightAmount = Effect.Parameters["LightAmount"];
            public static EffectParameter Param_LightPositionWS = Effect.Parameters["LightPositionWS"];
            public static EffectParameter Param_LightRadius = Effect.Parameters["LightRadius"];
            public static EffectParameter Param_LightIntensity = Effect.Parameters["LightIntensity"];
            public static EffectParameter Param_LightColor = Effect.Parameters["LightColor"];
            public static EffectParameter Param_TiledListLength = Effect.Parameters["TiledListLength"];
            public static EffectParameter Param_CameraPositionWS = Effect.Parameters["CameraPositionWS"];

        }

    }
}