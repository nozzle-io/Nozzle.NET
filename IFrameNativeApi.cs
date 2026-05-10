using System;

namespace Nozzle;

internal interface IFrameNativeApi
{
    void Release(IntPtr handle);
    void LockPixels(IntPtr handle, NativeMethods.TextureOrigin origin, out NativeMethods.MappedPixels pixels);
    void UnlockPixels(IntPtr handle);
    void LockWritablePixels(IntPtr handle, NativeMethods.TextureOrigin origin, out NativeMethods.MappedPixels pixels);
    void UnlockWritablePixels(IntPtr handle);
    void GetInfo(IntPtr handle, out NativeMethods.FrameInfo info);
    void GetResolvedFormat(IntPtr handle, out NativeMethods.ResolvedTextureFormat resolved);
    void CopyToGlTexture(IntPtr handle, uint glTextureName, uint glTarget, uint width, uint height, NativeMethods.TextureFormat format);
    void CopyToNativeTexture(IntPtr handle, IntPtr nativeTexture, uint width, uint height, NativeMethods.TextureFormat format);
}
