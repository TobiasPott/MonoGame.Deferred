﻿using DeferredEngine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

namespace DeferredEngine.Pipeline.Lighting
{
    public class PointLight : TransformableObject
    {
        public class MatrixSet
        {
            public readonly Matrix[] ViewProjection = new Matrix[6];

            public Matrix ViewSpace;
            public Matrix WorldViewProj;
            public Matrix WorldMatrix;
        }

        public int ShadowMapRadius = 3;

        public bool HasChanged = true;

        public readonly int ShadowResolution;
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
        public PointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, bool isVolumetric, int shadowResolution, int softShadowBlurAmount, float volumeDensity = 1, bool isEnabled = true)
            : base()
        {
            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            CastShadows = castShadows;
            IsVolumetric = isVolumetric;
            SoftShadowBlurAmount = softShadowBlurAmount;

            ShadowResolution = shadowResolution;
            LightVolumeDensity = volumeDensity;
            IsEnabled = isEnabled;

            Name = GetType().Name + " " + Id;
        }
        protected PointLight()
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

        public Matrix GetProjection() => Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, this.Radius);
        public Matrix GetViewProjection(CubeMapFace cubeMapFace) => Matrices.ViewProjection[(int)cubeMapFace];




        public void GetLightViewMatrices(CubeMapFace cubeMapFace, ref Matrix lightView, ref Matrix lightViewProjection)
        {
            Matrix lightProjection = this.GetProjection();
            // render the scene to all cubemap faces
            switch (cubeMapFace)
            {
                case CubeMapFace.PositiveX:
                    {
                        lightView = Matrix.CreateLookAt(_position, _position + Vector3.UnitX, Vector3.UnitZ);
                        lightViewProjection = lightView * lightProjection;
                        Matrices.ViewProjection[(int)cubeMapFace] = lightViewProjection;
                        break;
                    }
                case CubeMapFace.NegativeX:
                    {
                        lightView = Matrix.CreateLookAt(_position, _position - Vector3.UnitX, Vector3.UnitZ);
                        lightViewProjection = lightView * lightProjection;
                        Matrices.ViewProjection[(int)cubeMapFace] = lightViewProjection;
                        break;
                    }
                case CubeMapFace.PositiveY:
                    {
                        lightView = Matrix.CreateLookAt(_position, _position + Vector3.UnitY, Vector3.UnitZ);
                        lightViewProjection = lightView * lightProjection;
                        Matrices.ViewProjection[(int)cubeMapFace] = lightViewProjection;
                        break;
                    }
                case CubeMapFace.NegativeY:
                    {
                        lightView = Matrix.CreateLookAt(_position, _position - Vector3.UnitY, Vector3.UnitZ);
                        lightViewProjection = lightView * lightProjection;
                        Matrices.ViewProjection[(int)cubeMapFace] = lightViewProjection;
                        break;
                    }
                case CubeMapFace.PositiveZ:
                    {
                        lightView = Matrix.CreateLookAt(_position, _position + Vector3.UnitZ, Vector3.UnitX);
                        lightViewProjection = lightView * lightProjection;
                        Matrices.ViewProjection[(int)cubeMapFace] = lightViewProjection;
                        break;
                    }
                case CubeMapFace.NegativeZ:
                    {
                        lightView = Matrix.CreateLookAt(_position, _position - Vector3.UnitZ, Vector3.UnitX);
                        lightViewProjection = lightView * lightProjection;
                        Matrices.ViewProjection[(int)cubeMapFace] = lightViewProjection;
                        break;
                    }

            }
        }

    }

}
