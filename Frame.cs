using System;
using System.Runtime.InteropServices;

namespace Nozzle;

public sealed class Frame : IDisposable
{
    private unsafe NativeMethods.NozzleFrame* _handle;
    private readonly bool _ownsHandle;

    internal unsafe Frame(NativeMethods.NozzleFrame* handle, bool ownsHandle)
    {
        _handle = handle;
        _ownsHandle = ownsHandle;
    }

    internal unsafe NativeMethods.NozzleFrame* Handle => _handle;

    public FrameInfo GetInfo()
    {
        unsafe
        {
            var info = new NativeMethods.FrameInfo();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_frame_get_info(_handle, &info));
            return FrameInfo.FromNative(info);
        }
    }

    public MappedPixels LockPixels(TextureOrigin origin = TextureOrigin.TopLeft)
    {
        unsafe
        {
            var pixels = new NativeMethods.MappedPixels();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_frame_lock_pixels_with_origin(
                _handle, (NativeMethods.TextureOrigin)origin, &pixels));
            return MappedPixels.FromNative(pixels);
        }
    }

    public void UnlockPixels()
    {
        unsafe
        {
            NativeMethods.nozzle_frame_unlock_pixels(_handle);
        }
    }

    public MappedPixels LockWritablePixels(TextureOrigin origin = TextureOrigin.TopLeft)
    {
        unsafe
        {
            var pixels = new NativeMethods.MappedPixels();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_frame_lock_writable_pixels_with_origin(
                _handle, (NativeMethods.TextureOrigin)origin, &pixels));
            return MappedPixels.FromNative(pixels);
        }
    }

    public void UnlockWritablePixels()
    {
        unsafe
        {
            NativeMethods.nozzle_frame_unlock_writable_pixels(_handle);
        }
    }

    public void CopyToGlTexture(uint glTextureName, uint glTarget, uint width, uint height, TextureFormat format)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_frame_copy_to_gl_texture(_handle, glTextureName, glTarget,
                    width, height, (NativeMethods.TextureFormat)format));
        }
    }

    public ResolvedTextureFormat GetResolvedFormat()
    {
        unsafe
        {
            var resolved = new NativeMethods.ResolvedTextureFormat();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_frame_get_resolved_format(_handle, &resolved));
            return ResolvedTextureFormat.FromNative(resolved);
        }
    }

    public void CopyToNativeTexture(IntPtr nativeTexture, uint width, uint height, TextureFormat format)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_frame_copy_to_native_texture(_handle, (void*)nativeTexture,
                    width, height, (NativeMethods.TextureFormat)format));
        }
    }

    private unsafe void Dispose(bool disposing)
    {
        if (_handle != null && _ownsHandle)
        {
            NativeMethods.nozzle_frame_release(_handle);
            _handle = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Frame()
    {
        Dispose(false);
    }
}
