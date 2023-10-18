using DeferredEngine.Entities;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{

    public sealed class DeferredDirectionalLight : TransformableObject
    {
        public class MatrixSet
        {
            public Matrix View;
            public Matrix ViewProjection;
            public Matrix ViewProjection_ViewSpace;
            public Matrix View_ViewSpace;
        }

        public enum ShadowFilteringTypes
        {
            PCF, SoftPCF3x, SoftPCF5x, Poisson, /*VSM*/
        }


        public float Intensity;

        public Vector3 DirectionViewSpace;

        private Vector3 _initialDirection;
        private Vector3 _direction;
        private Color _color;

        // wrap into nested type
        public bool CastShadows;
        public float ShadowSize;
        public float ShadowFarClip;
        public int ShadowResolution;
        public ShadowFilteringTypes ShadowFiltering;
        public RenderTarget2D ShadowMap;

        public readonly MatrixSet Matrices = new MatrixSet();


        public bool HasChanged { get; set; }
        public Vector3 Color_sRGB => _color.ToVector3().Pow(2.2f);

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

        public Color Color { get; set; } = Color.White;

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
            get { return base.Position; }
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

        public void UpdateViewProjection()
        {
            Matrices.View = Matrix.CreateLookAt(Position, Position + Direction, Vector3.Down);
            Matrices.ViewProjection = Matrices.View * Matrix.CreateOrthographic(ShadowSize, ShadowSize, -ShadowFarClip, ShadowFarClip);
        }
        public void UpdateViewSpaceProjection(PipelineMatrices matrices)
        {
            DirectionViewSpace = Vector3.Transform(Direction, matrices.ViewIT);
            Matrices.ViewProjection_ViewSpace = matrices.InverseView * Matrices.ViewProjection;
            Matrices.View_ViewSpace = matrices.InverseView * Matrices.View;
        }

    }
}
