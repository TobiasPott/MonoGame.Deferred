using DeferredEngine.Entities;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class DeferredPointLight : TransformableObject
    {
        public class MatrixSet
        {
            public Matrix ViewProjectionPositiveX;
            public Matrix ViewProjectionNegativeX;
            public Matrix ViewProjectionPositiveY;
            public Matrix ViewProjectionNegativeY;
            public Matrix ViewProjectionPositiveZ;
            public Matrix ViewProjectionNegativeZ;

            public Matrix ViewSpace;
            public Matrix WorldViewProj;
            public Matrix WorldMatrix;
        }

        public int ShadowMapRadius = 3;

        public bool HasChanged = true;

        public readonly int ShadowResolution;
        public readonly bool StaticShadows;
        public RenderTarget2D ShadowMap;

        public BoundingSphere BoundingSphere;

        public bool CastShadows;
        public bool CastSDFShadows;
        public int SoftShadowBlurAmount = 0;

        private float _radius;
        private Color _color;
        public float Intensity;
        public readonly bool IsVolumetric;
        public readonly float LightVolumeDensity = 1;


        public readonly MatrixSet Matrices = new MatrixSet();

        public Vector3 Color_sRGB => _color.ToVector3().Pow(2.2f);

        /// <summary>
        /// A point light is a light that shines in all directions
        /// </summary>
        public DeferredPointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, bool isVolumetric, int shadowResolution, int softShadowBlurAmount, bool staticShadow, float volumeDensity = 1, bool isEnabled = true)
        {
            Id = IdGenerator.GetNewId();

            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            CastShadows = castShadows;
            IsVolumetric = isVolumetric;
            SoftShadowBlurAmount = softShadowBlurAmount;

            ShadowResolution = shadowResolution;
            StaticShadows = staticShadow;
            LightVolumeDensity = volumeDensity;
            IsEnabled = isEnabled;

            Name = GetType().Name + " " + Id;
        }
        protected DeferredPointLight()
        {

        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                base.Position = value;
                BoundingSphere.Center = value;
                Matrices.WorldMatrix = Matrix.CreateScale(Radius * 1.1f) * Matrix.CreateTranslation(Position);
                HasChanged = true;
            }
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                BoundingSphere.Radius = value;
                Matrices.WorldMatrix = Matrix.CreateScale(Radius * 1.1f) * Matrix.CreateTranslation(Position);
                HasChanged = true;
            }
        }


        public Matrix GetViewProjection(CubeMapFace face)
        {
            return face switch
            {
                CubeMapFace.NegativeX => Matrices.ViewProjectionNegativeX,
                CubeMapFace.NegativeY => Matrices.ViewProjectionNegativeY,
                CubeMapFace.NegativeZ => Matrices.ViewProjectionNegativeZ,
                CubeMapFace.PositiveX => Matrices.ViewProjectionPositiveX,
                CubeMapFace.PositiveY => Matrices.ViewProjectionPositiveY,
                CubeMapFace.PositiveZ => Matrices.ViewProjectionPositiveZ,
                _ => Matrix.Identity,
            };
        }

    }

}
