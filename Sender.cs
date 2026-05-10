using System;
using System.Runtime.InteropServices;

namespace Nozzle;

public sealed class Sender : IDisposable
{
    private unsafe NativeMethods.NozzleSender* _handle;

    internal unsafe Sender(NativeMethods.NozzleSender* handle)
    {
        _handle = handle;
    }

    internal static unsafe NativeMethods.SenderDesc BuildSenderDesc(
        byte* pName, byte* pApp,
        uint ringBufferSize, bool allowFormatFallback, FallbackFlags? fallbackFlags)
    {
        var desc = new NativeMethods.SenderDesc
        {
            Name = pName,
            ApplicationName = pApp,
            RingBufferSize = ringBufferSize,
        };

        if (fallbackFlags.HasValue)
        {
            desc.FallbackFlags = (uint)fallbackFlags.Value;
            desc.FallbackFlagsValid = 1;
        }
        else
        {
            desc.AllowFormatFallback = allowFormatFallback ? 1 : 0;
        }

        return desc;
    }

    public static Sender Create(string name, string applicationName, uint ringBufferSize = 3, bool allowFormatFallback = false, FallbackFlags? fallbackFlags = null)
    {
        unsafe
        {
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(name + '\0');
            var appBytes = System.Text.Encoding.UTF8.GetBytes(applicationName + '\0');

            fixed (byte* pName = nameBytes)
            fixed (byte* pApp = appBytes)
            {
                var desc = BuildSenderDesc(pName, pApp, ringBufferSize, allowFormatFallback, fallbackFlags);

                NativeMethods.NozzleSender* sender;
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_sender_create(&desc, &sender));
                return new Sender(sender);
            }
        }
    }

    public static FallbackFlags ResolveFallbackFlags(bool allowFormatFallback = false)
    {
        unsafe
        {
            var desc = new NativeMethods.SenderDesc
            {
                AllowFormatFallback = allowFormatFallback ? 1 : 0,
            };

            uint flags;
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_resolve_fallback_flags(&desc, &flags));
            return (FallbackFlags)flags;
        }
    }

    public static FallbackFlags ResolveFallbackFlags(FallbackFlags callerHint)
    {
        unsafe
        {
            var desc = new NativeMethods.SenderDesc
            {
                FallbackFlags = (uint)callerHint,
                FallbackFlagsValid = 1,
            };

            uint flags;
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_resolve_fallback_flags(&desc, &flags));
            return (FallbackFlags)flags;
        }
    }

    public unsafe Frame AcquireWritableFrame(uint width, uint height, TextureFormat format)
    {
        NativeMethods.NozzleFrame* frame;
        ErrorHelper.ThrowIfFailed(
            NativeMethods.nozzle_sender_acquire_writable_frame(_handle, width, height,
                (NativeMethods.TextureFormat)format, &frame));
        return new Frame(frame);
    }

    public unsafe void CommitFrame(Frame frame)
    {
        frame.ThrowIfMapped();
        var frameHandle = frame.DangerousGetHandle();

        var senderHandle = _handle;
        if (senderHandle == null)
            throw new ObjectDisposedException(nameof(Sender));

        ErrorHelper.ThrowIfFailed(
            NativeMethods.nozzle_sender_commit_frame(senderHandle, (NativeMethods.NozzleFrame*)frameHandle));
    }

    public SenderInfo GetInfo()
    {
        unsafe
        {
            var info = new NativeMethods.SenderInfo();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_sender_get_info(_handle, &info));
            return SenderInfo.FromNative(info);
        }
    }

    public void PublishGlTexture(uint glTextureName, uint glTarget, uint width, uint height, TextureFormat format)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_sender_publish_gl_texture(_handle, glTextureName, glTarget,
                    width, height, (NativeMethods.TextureFormat)format));
        }
    }

    public void PublishNativeTexture(IntPtr nativeTexture, uint width, uint height, TextureFormat format)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_sender_publish_native_texture(_handle, (void*)nativeTexture,
                    width, height, (NativeMethods.TextureFormat)format));
        }
    }

    public void PublishNativeTexture(IntPtr nativeTexture, uint width, uint height, TextureFormat storageFormat, TextureFormat semanticFormat)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(
                NativeMethods.nozzle_sender_publish_native_texture_ex(_handle, (void*)nativeTexture,
                    width, height, (NativeMethods.TextureFormat)storageFormat, (NativeMethods.TextureFormat)semanticFormat));
        }
    }

    private unsafe void Dispose(bool disposing)
    {
        if (_handle != null)
        {
            NativeMethods.nozzle_sender_destroy(_handle);
            _handle = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Sender()
    {
        Dispose(false);
    }
}
