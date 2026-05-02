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

    public MappedPixels LockPixels()
    {
        unsafe
        {
            var pixels = new NativeMethods.MappedPixels();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_frame_lock_pixels(_handle, &pixels));
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

    public MappedPixels LockWritablePixels()
    {
        unsafe
        {
            var pixels = new NativeMethods.MappedPixels();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_frame_lock_writable_pixels(_handle, &pixels));
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
