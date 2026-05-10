using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Nozzle.Tests;

public class FrameLifetimeTests
{
    private class FakeFrameApi : IFrameNativeApi
    {
        public List<string> CallLog { get; } = [];
        public int ReleaseCount { get; private set; }
        public int UnlockPixelsCount { get; private set; }
        public int UnlockWritablePixelsCount { get; private set; }

        private void Log(string method) => CallLog.Add(method);

        public void Release(IntPtr handle)
        {
            Log("Release");
            ReleaseCount++;
        }

        public unsafe void LockPixels(IntPtr handle, NativeMethods.TextureOrigin origin, out NativeMethods.MappedPixels pixels)
        {
            Log("LockPixels");
            pixels = new NativeMethods.MappedPixels
            {
                Data = (void*)0xABCD,
                RowStrideBytes = 7680,
                Width = 1920,
                Height = 1080,
                Format = NativeMethods.TextureFormat.Rgba8Unorm,
                Origin = NativeMethods.TextureOrigin.TopLeft,
            };
        }

        public void UnlockPixels(IntPtr handle)
        {
            Log("UnlockPixels");
            UnlockPixelsCount++;
        }

        public unsafe void LockWritablePixels(IntPtr handle, NativeMethods.TextureOrigin origin, out NativeMethods.MappedPixels pixels)
        {
            Log("LockWritablePixels");
            pixels = new NativeMethods.MappedPixels
            {
                Data = (void*)0xABCD,
                RowStrideBytes = 7680,
                Width = 1920,
                Height = 1080,
                Format = NativeMethods.TextureFormat.Rgba8Unorm,
                Origin = NativeMethods.TextureOrigin.TopLeft,
            };
        }

        public void UnlockWritablePixels(IntPtr handle)
        {
            Log("UnlockWritablePixels");
            UnlockWritablePixelsCount++;
        }

        public void GetInfo(IntPtr handle, out NativeMethods.FrameInfo info)
        {
            Log("GetInfo");
            info = default;
        }

        public void GetResolvedFormat(IntPtr handle, out NativeMethods.ResolvedTextureFormat resolved)
        {
            Log("GetResolvedFormat");
            resolved = default;
        }

        public void CopyToGlTexture(IntPtr handle, uint glTextureName, uint glTarget, uint width, uint height, NativeMethods.TextureFormat format)
        {
            Log("CopyToGlTexture");
        }

        public void CopyToNativeTexture(IntPtr handle, IntPtr nativeTexture, uint width, uint height, NativeMethods.TextureFormat format)
        {
            Log("CopyToNativeTexture");
        }
    }

    private static unsafe Frame CreateTestFrame(FakeFrameApi api)
    {
        return Frame.CreateForTests((NativeMethods.NozzleFrame*)0x1234, api);
    }

    [Fact]
    public void O1_Dispose_calls_Release_exactly_once()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Equal(1, api.ReleaseCount);
        Assert.Contains("Release", api.CallLog);
    }

    [Fact]
    public void O4a_Active_read_mapping_Dispose_unlocks_then_releases()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.LockPixels();
        frame.Dispose();

        var unlockIdx = api.CallLog.IndexOf("UnlockPixels");
        var releaseIdx = api.CallLog.IndexOf("Release");

        Assert.NotEqual(-1, unlockIdx);
        Assert.NotEqual(-1, releaseIdx);
        Assert.True(unlockIdx < releaseIdx, "UnlockPixels must be called before Release");
    }

    [Fact]
    public void O4b_Active_write_mapping_Dispose_unlocks_then_releases()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.LockWritablePixels();
        frame.Dispose();

        var unlockIdx = api.CallLog.IndexOf("UnlockWritablePixels");
        var releaseIdx = api.CallLog.IndexOf("Release");

        Assert.NotEqual(-1, unlockIdx);
        Assert.NotEqual(-1, releaseIdx);
        Assert.True(unlockIdx < releaseIdx, "UnlockWritablePixels must be called before Release");
    }

    [Fact]
    public void M1_ThrowIfMapped_when_mapped_throws_InvalidOperationException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.LockPixels();

        Assert.Throws<InvalidOperationException>(() => frame.ThrowIfMapped());
    }

    [Fact]
    public void M2_ThrowIfMapped_when_not_mapped_does_not_throw()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.ThrowIfMapped();
    }

    [Fact]
    public void S1_Invalidated_handle_Dispose_is_noop()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        var handle = frame.LockPixels();

        frame.Dispose();

        handle.Dispose();

        Assert.Equal(1, api.UnlockPixelsCount);
    }

    [Fact]
    public void S2_Invalidated_handle_properties_throw_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        var handle = frame.LockPixels();

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = handle.Data);
        Assert.Throws<ObjectDisposedException>(() => _ = handle.RowStrideBytes);
        Assert.Throws<ObjectDisposedException>(() => _ = handle.Width);
        Assert.Throws<ObjectDisposedException>(() => _ = handle.Height);
        Assert.Throws<ObjectDisposedException>(() => _ = handle.Format);
        Assert.Throws<ObjectDisposedException>(() => _ = handle.Origin);
    }

    [Fact]
    public void S3_Invalidate_cuts_frame_reference()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        var handle = frame.LockPixels();

        Assert.True(handle.HasFrameReferenceForTests);

        frame.Dispose();

        Assert.False(handle.HasFrameReferenceForTests);
    }

    [Fact]
    public void S4_Stale_OnHandleDisposed_callback_is_noop()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        var handle = frame.LockPixels();

        frame.Dispose();

        handle.Dispose();

        Assert.Equal(1, api.UnlockPixelsCount);
        Assert.Equal(1, api.ReleaseCount);
    }

    [Fact]
    public void D1_Dispose_then_GetInfo_throws_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() => frame.GetInfo());
    }

    [Fact]
    public void D3_Dispose_then_LockPixels_throws_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() => frame.LockPixels());
    }

    [Fact]
    public void D4_Dispose_then_LockWritablePixels_throws_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() => frame.LockWritablePixels());
    }

    [Fact]
    public void D5_Dispose_then_CopyToGlTexture_throws_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
            frame.CopyToGlTexture(0, 0, 640, 480, TextureFormat.Rgba8Unorm));
    }

    [Fact]
    public void D6_Dispose_then_CopyToNativeTexture_throws_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
            frame.CopyToNativeTexture(IntPtr.Zero, 640, 480, TextureFormat.Rgba8Unorm));
    }

    [Fact]
    public void D7_Dispose_then_GetResolvedFormat_throws_ObjectDisposedException()
    {
        var api = new FakeFrameApi();
        var frame = CreateTestFrame(api);

        frame.Dispose();

        Assert.Throws<ObjectDisposedException>(() => frame.GetResolvedFormat());
    }

    [Fact]
    public void A1_CreateForTests_and_production_constructor_exist()
    {
        var ptrType = typeof(NativeMethods.NozzleFrame).MakePointerType();

        var createForTests = typeof(Frame).GetMethod(
            "CreateForTests",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            [ptrType, typeof(IFrameNativeApi)],
            modifiers: null);

        Assert.NotNull(createForTests);

        var ctor = typeof(Frame).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [ptrType],
            modifiers: null);

        Assert.NotNull(ctor);
    }
}
