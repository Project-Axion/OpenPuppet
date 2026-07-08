using OpenPuppet.rendering;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public class RenderSurface
    {
        static GL _gl;

        public static List<RenderSurface> Surfaces { get; } = new();

        public bool Active { get; set; }
        public Camera Camera { get; set; }

        uint _glTex;
        uint _fbo;
        uint _rbo;

        public RenderSurface(Camera camera) 
        {
            Camera = camera;

            _fbo = _gl.GenFramebuffer();

            Resize(camera.Resolution);

            var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != GLEnum.FramebufferComplete)
                throw new Exception($"Could not create render surface framebuffer: {status}");
        }

        public unsafe void Resize(Vector2 size)
        {
            if (size.X <= 0 || size.Y <= 0) return;

            Camera.Resolution = size;

            if (_glTex != 0) _gl.DeleteTexture(_glTex);
            if (_rbo != 0) _gl.DeleteRenderbuffer(_rbo);

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

            _glTex = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, _glTex);

            _gl.TexImage2D(
                TextureTarget.Texture2D,0,InternalFormat.Rgba,
                (uint)size.X, (uint)size.Y,0,
                PixelFormat.Rgba,GLEnum.UnsignedByte,null
            );

            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

            _gl.BindTexture(TextureTarget.Texture2D, 0);

            _gl.FramebufferTexture2D(
                FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, _glTex, 0
            );

            _rbo = _gl.GenRenderbuffer();
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);

            _gl.RenderbufferStorage(
                RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8,
                (uint)size.X, (uint)size.Y
            );

            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, _rbo);

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        public nint GetImage() => (nint)_glTex;

        public static void Init(GL gl) => _gl = gl;

        public static void Unregister(RenderSurface surface) => Surfaces.Remove(surface);
        public static void Register(RenderSurface surface) => Surfaces.Add(surface);
    }
}
