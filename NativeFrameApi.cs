using System;

namespace Nozzle;

internal sealed class NativeFrameApi : IFrameNativeApi
{
    public static readonly NativeFrameApi Instance = new();

    private NativeFrameApi() { }

    public unsafe void Release(IntPtr handle)
    {
        NativeMethods.nozzle_frame_release((NativeMethods.NozzleFrame*)handle);
    }

    public unsafe void LockPixels(IntPtr handle, NativeMethods.TextureOrigin origin, out NativeMethods.MappedPixels pixels)
    {
        fixed (NativeMethods.MappedPixels* p = &pixels)
        {
            pixels = default;
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_frame_lock_pixels_with_origin(
                    (NativeMethods.NozzleFrame*)handle, origin, p));
        }
    }

    public unsafe void UnlockPixels(IntPtr handle)
    {
        NativeMethods.nozzle_frame_unlock_pixels((NativeMethods.NozzleFrame*)handle);
    }

    public unsafe void LockWritablePixels(IntPtr handle, NativeMethods.TextureOrigin origin, out NativeMethods.MappedPixels pixels)
    {
        fixed (NativeMethods.MappedPixels* p = &pixels)
        {
            pixels = default;
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_frame_lock_writable_pixels_with_origin(
                    (NativeMethods.NozzleFrame*)handle, origin, p));
        }
    }

    public unsafe void UnlockWritablePixels(IntPtr handle)
    {
        NativeMethods.nozzle_frame_unlock_writable_pixels((NativeMethods.NozzleFrame*)handle);
    }

    public unsafe void GetInfo(IntPtr handle, out NativeMethods.FrameInfo info)
    {
        fixed (NativeMethods.FrameInfo* p = &info)
        {
            info = default;
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_frame_get_info((NativeMethods.NozzleFrame*)handle, p));
        }
    }

    public unsafe void GetResolvedFormat(IntPtr handle, out NativeMethods.ResolvedTextureFormat resolved)
    {
        fixed (NativeMethods.ResolvedTextureFormat* p = &resolved)
        {
            resolved = default;
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_frame_get_resolved_format((NativeMethods.NozzleFrame*)handle, p));
        }
    }

    public unsafe void CopyToGlTexture(IntPtr handle, uint glTextureName, uint glTarget, uint width, uint height, NativeMethods.TextureFormat format)
    {
        ErrorHelper.ThrowIfFailed(
            NativeMethods.nozzle_frame_copy_to_gl_texture(
                (NativeMethods.NozzleFrame*)handle, glTextureName, glTarget,
                width, height, format));
    }

    public unsafe void CopyToNativeTexture(IntPtr handle, IntPtr nativeTexture, uint width, uint height, NativeMethods.TextureFormat format)
    {
        ErrorHelper.ThrowIfFailed(
            NativeMethods.nozzle_frame_copy_to_native_texture(
                (NativeMethods.NozzleFrame*)handle, (void*)nativeTexture,
                width, height, format));
    }
}
