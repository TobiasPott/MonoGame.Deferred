using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public class Transform : TransformBase
    {
        protected bool _worldHasChanged = false;

        protected Vector3 _position;
        public override Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _worldHasChanged = true;
            }
        }

        protected Matrix _rotationMatrix;
        public override Matrix RotationMatrix
        {
            get => _rotationMatrix;
            set
            {
                _rotationMatrix = value;
                _worldHasChanged = true;
            }
        }

        protected Vector3 _scale;
        public override Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _worldHasChanged = true;
            }
        }


        protected Matrix _world;
        public override Matrix World
        {
            get
            {
                if (_worldHasChanged)
                    UpdateMatrices();
                return _world;
            }
        }

        protected Matrix _inverseWorld;
        public override Matrix InverseWorld
        {
            get
            {
                if (_worldHasChanged)
                    UpdateMatrices();
                return _inverseWorld;
            }
        }


        protected virtual void UpdateMatrices()
        {
            _world = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            _inverseWorld = Matrix.Invert(_world);
            _worldHasChanged = false;
        }
    }
}