using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nozzle;

public sealed class Frame : IDisposable
{
    private unsafe NativeMethods.NozzleFrame* _handle;
    private readonly IFrameNativeApi _api;
    private MappedPixelHandle? _activeMapping;
    private MappedPixelAccess? _activeMappingAccess;

    /// <summary>
    /// Production constructor. Handle must be a valid NozzleFrame* returned by the C API.
    /// Finalizer is enabled as a safety net for undisposed frames.
    /// </summary>
    internal unsafe Frame(NativeMethods.NozzleFrame* handle)
        : this(handle, NativeFrameApi.Instance, suppressFinalizer: false)
    {
    }

    private unsafe Frame(
        NativeMethods.NozzleFrame* handle,
        IFrameNativeApi api,
        bool suppressFinalizer)
    {
        _handle = handle;
        _api = api;

        if (suppressFinalizer)
        {
            // Test/fake handles must never reach the production finalizer path.
            GC.SuppressFinalize(this);
        }
    }

    internal static unsafe Frame CreateForTests(
        NativeMethods.NozzleFrame* handle,
        IFrameNativeApi api)
    {
        return new Frame(handle, api, suppressFinalizer: true);
    }

    internal unsafe IntPtr DangerousGetHandle()
    {
        if (_handle == null)
            throw new ObjectDisposedException(nameof(Frame));
        return (IntPtr)_handle;
    }

    internal void ThrowIfMapped()
    {
        if (_activeMapping != null)
            throw new InvalidOperationException(
                "Cannot commit frame while pixels are mapped. Dispose the MappedPixelHandle first.");
    }

    public FrameInfo GetInfo()
    {
        _api.GetInfo(DangerousGetHandle(), out NativeMethods.FrameInfo info);
        return FrameInfo.FromNative(info);
    }

    public MappedPixelHandle LockPixels(TextureOrigin origin = TextureOrigin.TopLeft)
    {
        var handle = DangerousGetHandle();
        ThrowIfAlreadyMapped();
        _api.LockPixels(handle, (NativeMethods.TextureOrigin)origin, out NativeMethods.MappedPixels pixels);
        var mapping = new MappedPixelHandle(this, MappedPixels.FromNative(pixels), MappedPixelAccess.ReadOnly);
        _activeMapping = mapping;
        _activeMappingAccess = MappedPixelAccess.ReadOnly;
        return mapping;
    }

    public MappedPixelHandle LockWritablePixels(TextureOrigin origin = TextureOrigin.TopLeft)
    {
        var handle = DangerousGetHandle();
        ThrowIfAlreadyMapped();
        _api.LockWritablePixels(handle, (NativeMethods.TextureOrigin)origin, out NativeMethods.MappedPixels pixels);
        var mapping = new MappedPixelHandle(this, MappedPixels.FromNative(pixels), MappedPixelAccess.Writable);
        _activeMapping = mapping;
        _activeMappingAccess = MappedPixelAccess.Writable;
        return mapping;
    }

    internal void OnHandleDisposed(MappedPixelHandle handle)
    {
        if (_activeMapping == null)
            return;

        _activeMapping = null;
        var access = _activeMappingAccess;
        _activeMappingAccess = null;

        var nativeHandle = DangerousGetHandle();
        if (access == MappedPixelAccess.ReadOnly)
            _api.UnlockPixels(nativeHandle);
        else
            _api.UnlockWritablePixels(nativeHandle);
    }

    public void CopyToGlTexture(uint glTextureName, uint glTarget, uint width, uint height, TextureFormat format)
    {
        _api.CopyToGlTexture(DangerousGetHandle(), glTextureName, glTarget,
            width, height, (NativeMethods.TextureFormat)format);
    }

    public ResolvedTextureFormat GetResolvedFormat()
    {
        _api.GetResolvedFormat(DangerousGetHandle(), out NativeMethods.ResolvedTextureFormat resolved);
        return ResolvedTextureFormat.FromNative(resolved);
    }

    public void CopyToNativeTexture(IntPtr nativeTexture, uint width, uint height, TextureFormat format)
    {
        _api.CopyToNativeTexture(DangerousGetHandle(), nativeTexture,
            width, height, (NativeMethods.TextureFormat)format);
    }

    private unsafe void Dispose(bool disposing)
    {
        var handle = _handle;
        if (handle == null) return;
        _handle = null;

        if (disposing)
        {
            var activeMapping = _activeMapping;
            var activeAccess = _activeMappingAccess;
            _activeMapping = null;
            _activeMappingAccess = null;

            try
            {
                if (activeMapping != null)
                {
                    activeMapping.Invalidate();

                    var nativeHandle = (IntPtr)handle;
                    if (activeAccess!.Value == MappedPixelAccess.ReadOnly)
                        _api.UnlockPixels(nativeHandle);
                    else
                        _api.UnlockWritablePixels(nativeHandle);
                }
            }
            finally
            {
                // Native unlock/release are no-throw by contract;
                // finally keeps fake/test failures from skipping release.
                _api.Release((IntPtr)handle);
            }
        }
        else
        {
            try
            {
                NativeMethods.nozzle_frame_release(handle);
            }
            catch
            {
                // Suppress managed P/Invoke setup exceptions only.
                // Does NOT protect against native crashes (access violation etc).
                // Production invariant: handle is always valid C API-returned pointer.
            }
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

    private void ThrowIfAlreadyMapped()
    {
        if (_activeMapping != null)
            throw new InvalidOperationException(
                "Frame already has an active pixel mapping. Dispose the existing MappedPixelHandle first.");
    }
}
