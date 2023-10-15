using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Entities
{
    public sealed class DeferredDirectionalLight : TransformableObject
    {
        public float Intensity;
        private Vector3 _direction;
        private Vector3 _initialDirection;
        public bool HasChanged;

        public Vector3 DirectionViewSpace;

        private Color _color;
        public Vector3 ColorV3;

        public readonly bool CastShadows;
        public readonly float ShadowSize;
        public readonly float ShadowFarClip;
        public readonly int ShadowResolution;

        public RenderTarget2D ShadowMap;
        public Matrix ShadowViewProjection;

        public ShadowFilteringTypes ShadowFiltering;

        public Matrix LightViewProjection;
        public Matrix LightView;
        public Matrix LightViewProjection_ViewSpace;
        public Matrix LightView_ViewSpace;

        public enum ShadowFilteringTypes
        {
            PCF, SoftPCF3x, SoftPCF5x, Poisson, /*VSM*/
        }

        /// <summary>
        /// Create a Directional light, shadows are optional
        /// </summary>
        public DeferredDirectionalLight(Color color, float intensity, Vector3 direction, Vector3 position = default(Vector3), 
            bool castShadows = false, float shadowSize = 100, float shadowFarClip = 100, int shadowMapResolution = 512, 
            ShadowFilteringTypes shadowFiltering = ShadowFilteringTypes.Poisson)
        {
            Id = IdGenerator.GetNewId();

            Color = color;
            Intensity = intensity;

            Vector3 normalizedDirection = direction;
            normalizedDirection.Normalize();
            Direction = normalizedDirection;
            _initialDirection = normalizedDirection;

            CastShadows = castShadows;
            ShadowSize = shadowSize;
            ShadowFarClip = shadowFarClip;
            ShadowResolution = shadowMapResolution;

            ShadowFiltering = shadowFiltering;

            Position = position;

            _rotationMatrix = Matrix.Identity;

            Name = GetType().Name + " " + Id;
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                ColorV3 = (_color.ToVector3().Pow(2.2f)); // applies sRGB gamma correction
            }
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                HasChanged = true;
            }
        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                base.Position = value;
                HasChanged = true;
            }
        }

        public override Matrix RotationMatrix
        {
            get { return _rotationMatrix; }
            set
            {
                base.RotationMatrix = value;
                Direction = Vector3.Transform(_initialDirection, RotationMatrix);
            }
        }

    }
}
