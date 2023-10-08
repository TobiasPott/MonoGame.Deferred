﻿using DeferredEngine.Recources;
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
        public readonly float ShadowDepth;
        public readonly int ShadowResolution;
        private readonly bool _staticShadow;

        public RenderTarget2D ShadowMap;
        public Matrix ShadowViewProjection;

        public ShadowFilteringTypes ShadowFiltering;
        public bool ScreenSpaceShadowBlur;

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
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="direction"></param>
        /// <param name="castShadows"></param>
        /// <param name="shadowSize"></param>
        /// <param name="shadowDepth"></param>
        /// <param name="shadowResolution"></param>
        /// <param name="shadowFiltering"></param>
        /// <param name="screenspaceshadowblur"></param>
        /// <param name="staticshadows"></param>
        public DeferredDirectionalLight(Color color, float intensity, Vector3 direction, Vector3 position = default(Vector3), bool castShadows = false, float shadowSize = 100, float shadowDepth = 100, int shadowResolution = 512, ShadowFilteringTypes shadowFiltering = ShadowFilteringTypes.Poisson, bool screenspaceshadowblur = false, bool staticshadows = false)
        {
            Color = color;
            Intensity = intensity;

            Vector3 normalizedDirection = direction;
            normalizedDirection.Normalize();
            Direction = normalizedDirection;
            _initialDirection = normalizedDirection;

            CastShadows = castShadows;
            ShadowSize = shadowSize;
            ShadowDepth = shadowDepth;
            ShadowResolution = shadowResolution;
            _staticShadow = staticshadows;

            ScreenSpaceShadowBlur = screenspaceshadowblur;

            ShadowFiltering = shadowFiltering;

            Position = position;

            Id = IdGenerator.GetNewId();

            IsEnabled = true;

            _rotationMatrix = Matrix.Identity;

            Name = GetType().Name + " " + Id;
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                ColorV3 = (_color.ToVector3().Pow(2.2f));
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
                TransformAnglesToDirection();
            }
        }

        private void TransformAnglesToDirection()
        {
            Direction = Vector3.Transform(_initialDirection, RotationMatrix);
        }

        public void ApplyShader()
        {
            if (CastShadows)
            {
                //Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                //Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(shadowMap);
                if (ScreenSpaceShadowBlur)
                {
                    throw new NotImplementedException();
                    /*
                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)ShadowFiltering);
                    Shaders.deferredDirectionalLightSSShadowed.Passes[0].Apply();  
                    */
                }
                else
                {
                    Shaders.deferredDirectionalLightParameterLightView.SetValue(LightView_ViewSpace);
                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameterLightFarClip.SetValue(ShadowDepth);
                    Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(ShadowMap);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)ShadowFiltering);
                    Shaders.deferredDirectionalLightParameter_ShadowMapSize.SetValue((float)ShadowResolution);
                    Shaders.deferredDirectionalLightShadowed.Passes[0].Apply();
                }
            }
            else
            {
                Shaders.deferredDirectionalLightUnshadowed.Passes[0].Apply();
            }
        }
    }
}
