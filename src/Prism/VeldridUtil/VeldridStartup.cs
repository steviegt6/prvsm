using System;
using System.Diagnostics;
using SDL2;
using Veldrid;
using Veldrid.OpenGL;

namespace Prism.VeldridUtil;

internal static class VeldridStartup {
    private static readonly object gl_version_lock = new();
    private static (int Major, int Minor)? maxSupportedGlVersion;

    /*public static GraphicsDevice CreateGraphicsDevice(GraphicsDeviceOptions options, nint sdlHandle) {
        return CreateDefaultOpenGlGraphicsDevice(options, sdlHandle);
    }*/

    public static GraphicsDevice CreateDefaultOpenGlGraphicsDevice(GraphicsDeviceOptions options, nint sdlHandle) {
        SDL.SDL_ClearError();
        SDL.SDL_ClearError();

        SDL.SDL_SysWMinfo sysWmInfo = default;
        SDL.SDL_GetVersion(out sysWmInfo.version);
        SDL.SDL_GetWindowWMInfo(sdlHandle, ref sysWmInfo);

        SetSdlGlContextAttributes(options);

        var contextHandle = SDL.SDL_GL_CreateContext(sdlHandle);
        var error = SDL.SDL_GetError();

        if (!string.IsNullOrEmpty(error)) {
            throw new VeldridException(
                $"Unable to create OpenGL Context: \"{error}\". This may indicate that the system does not support the requested OpenGL profile, version, or Swapchain format.");
        }

        _ = SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, out _);
        _ = SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, out _);

        _ = SDL.SDL_GL_SetSwapInterval(options.SyncToVerticalBlank ? 1 : 0);

        var platformInfo = new OpenGLPlatformInfo(
            contextHandle,
            SDL.SDL_GL_GetProcAddress,
            context => _ = SDL.SDL_GL_MakeCurrent(sdlHandle, context),
            SDL.SDL_GL_GetCurrentContext,
            () => _ = SDL.SDL_GL_MakeCurrent(IntPtr.Zero, IntPtr.Zero),
            SDL.SDL_GL_DeleteContext,
            () => SDL.SDL_GL_SwapWindow(sdlHandle),
            sync => _ = SDL.SDL_GL_SetSwapInterval(sync ? 1 : 0));

        SDL.SDL_GetWindowSize(sdlHandle, out var width, out var height);
        return GraphicsDevice.CreateOpenGL(
            options,
            platformInfo,
            (uint)width,
            (uint)height
        );
    }

    public static void SetSdlGlContextAttributes(GraphicsDeviceOptions options) {
        var contextFlags = options.Debug
            ? SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG | SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG
            : SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG;

        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)contextFlags);

        var (major, minor) = GetMaxGlVersion();

        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);

        var depthBits = 0;
        var stencilBits = 0;

        if (options.SwapchainDepthFormat.HasValue) {
            switch (options.SwapchainDepthFormat) {
                case PixelFormat.R16_UNorm:
                    depthBits = 16;
                    break;

                case PixelFormat.D24_UNorm_S8_UInt:
                    depthBits = 24;
                    stencilBits = 8;
                    break;

                case PixelFormat.R32_Float:
                    depthBits = 32;
                    break;

                case PixelFormat.D32_Float_S8_UInt:
                    depthBits = 32;
                    stencilBits = 8;
                    break;

                default:
                    throw new VeldridException("Invalid depth format: " + options.SwapchainDepthFormat.Value);
            }
        }

        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, depthBits);
        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, stencilBits);

        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_FRAMEBUFFER_SRGB_CAPABLE, options.SwapchainSrgbFormat ? 1 : 0);
    }

    private static (int Major, int Minor) GetMaxGlVersion() {
        lock (gl_version_lock) {
            var maxVer = maxSupportedGlVersion;

            if (maxVer == null) {
                maxVer = TestMaxVersion();
                maxSupportedGlVersion = maxVer;
            }

            return maxVer.Value;
        }
    }

    private static (int Major, int Minor) TestMaxVersion() {
        (int, int)[] testVersions = { (4, 6), (4, 3), (4, 0), (3, 3), (3, 0) };

        foreach (var (major, minor) in testVersions) {
            if (TestIndividualGlVersion(major, minor)) {
                return (major, minor);
            }
        }

        return (0, 0);
    }

    private static  bool TestIndividualGlVersion(int major, int minor) {
        const SDL.SDL_GLprofile profile_mask = SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE;

        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)profile_mask);
        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
        _ = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);

        var window = SDL.SDL_CreateWindow(
            string.Empty,
            0,
            0,
            1,
            1,
            SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
        );
        var error = SDL.SDL_GetError();

        if (window == IntPtr.Zero || !string.IsNullOrEmpty(error)) {
            SDL.SDL_ClearError();
            Debug.WriteLine($"Unable to create version {major}.{minor} {profile_mask} context.");
            return false;
        }

        var context = SDL.SDL_GL_CreateContext(window);
        error = SDL.SDL_GetError();

        if (!string.IsNullOrEmpty(error)) {
            SDL.SDL_ClearError();
            Debug.WriteLine($"Unable to create version {major}.{minor} {profile_mask} context.");
            SDL.SDL_DestroyWindow(window);
            return false;
        }

        SDL.SDL_GL_DeleteContext(context);
        SDL.SDL_DestroyWindow(window);
        return true;
    }
}
