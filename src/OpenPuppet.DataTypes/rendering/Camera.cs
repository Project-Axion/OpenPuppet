using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public class Camera
    {
        public const float CAMERA_UNITS_BASIS = 3f;

        Matrix4x4 _proj = Matrix4x4.Identity, _view = Matrix4x4.Identity;
        Vector3 _pan = Vector3.Zero;
        float _zoom = 1;
        Vector2 _res = Vector2.One;

        public ref readonly Matrix4x4 Projection => ref _proj;
        public ref readonly Matrix4x4 View => ref _view;

        public Vector3 Pan { get => _pan; set { _pan = value; UpdateView(); } }
        public float Zoom { get => _zoom; set { _zoom = value; UpdateProj(); } }

        public Vector2 Resolution { get => _res; set { _res = value; UpdateProj(); } }

        public Camera(Vector2 res)
        {
            Resolution = res;
        }

        void UpdateProj()
        {
            float aspect = _res.X / _res.Y;

            float halfHeight = (CAMERA_UNITS_BASIS / _zoom) / 2;
            float halfWidth = halfHeight * aspect;

            _proj = Matrix4x4.CreateOrthographicOffCenter(
                -halfWidth,  halfWidth, 
                -halfHeight, halfHeight,
                0.01f,1000f
            );
        }

        void UpdateView() => _view = Matrix4x4.CreateTranslation(_pan);
    }
}
