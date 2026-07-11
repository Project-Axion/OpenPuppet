using Newtonsoft.Json;
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
        float _aspect = 1;

        [JsonIgnore]
        public ref readonly Matrix4x4 Projection => ref _proj;
        [JsonIgnore]
        public ref readonly Matrix4x4 View => ref _view;
        [JsonIgnore]
        public ref readonly float Aspect => ref _aspect;

        public Vector3 Pan { get => _pan; set { _pan = value; UpdateView(); } }
        public float Zoom { get => _zoom; set { _zoom = value; UpdateProj(); } }

        public Vector2 Resolution { get => _res; set { _res = value; UpdateProj(); } }

        public Camera(Vector2 res)
        {
            Resolution = res;
        }

        void UpdateProj()
        {
            _aspect = _res.X / _res.Y;

            float halfHeight = (CAMERA_UNITS_BASIS / _zoom) / 2;
            float halfWidth = halfHeight * _aspect;

            _proj = Matrix4x4.CreateOrthographicOffCenter(
                -halfWidth,  halfWidth, 
                -halfHeight, halfHeight,
                0.01f,1000f
            );
        }

        void UpdateView() => _view = Matrix4x4.CreateTranslation(_pan);
    }
}
