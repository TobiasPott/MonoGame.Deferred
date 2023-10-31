using DeferredEngine.Recources;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public class Camera : TransformableObject
    {
        private Vector3 _up = Vector3.UnitZ;
        private Vector3 _forward = Vector3.Up;
        private float _fieldOfView = (float)Math.PI / 4;
        private float _farClip = 512;

        public bool HasChanged = true;


        protected Matrix _view;
        protected Matrix _projection;
        protected Matrix _viewProjection;


        public virtual Matrix Projection
        {
            get
            {
                if (_worldHasChanged)
                    UpdateMatrices();
                return _projection;
            }
        }
        public virtual Matrix ViewProjection
        {
            get
            {
                if (_worldHasChanged)
                    UpdateMatrices();
                return _viewProjection;
            }
        }

        public virtual Matrix View
        {
            get
            {
                if (_worldHasChanged)
                    UpdateMatrices();
                return _view;
            }
        }


        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                if (base.Position != value)
                {
                    base.Position = value;
                    HasChanged = true;
                }
            }
        }

        public Vector3 Up
        {
            get => _up;
            set
            {
                if (_up != value)
                {
                    _up = value;
                    _worldHasChanged = true;
                    HasChanged = true;
                }
            }
        }

        public Vector3 Forward
        {
            get => _forward;
            set
            {
                if (_forward != value)
                {
                    _forward = value;
                    _worldHasChanged = true;
                    HasChanged = true;
                }
            }
        }

        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;
                _worldHasChanged = true;
                HasChanged = true;
            }
        }

        public float FarClip
        {
            get => _farClip;
            set
            {
                _farClip = value;
                _worldHasChanged = true;
                HasChanged = true;
            }
        }



        public Camera() : this(Vector3.Zero, Vector3.Forward)
        { }
        public Camera(Vector3 position, Vector3 lookat)
        {
            this.Position = position;
            _forward = lookat - position;
            _forward.Normalize();
        }


        public void SetLookAt(Vector3 value)
        {
            value -= Position;
            value.Normalize();
            Forward = value;
        }


        protected override void UpdateMatrices()
        {
            base.UpdateMatrices();
            //View matrix
            _view = Matrix.CreateLookAt(_position, _position + _forward, _up);
            _projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, RenderingSettings.Screen.g_Aspect, 1, _farClip);
            _viewProjection = _view * _projection;
        }


    }
}
