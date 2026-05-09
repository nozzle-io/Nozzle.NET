using System;
using System.Runtime.InteropServices;

namespace Nozzle;

public readonly struct FrameInfo
{
    public ulong FrameIndex { get; init; }
    public ulong TimestampNs { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public TextureFormat Format { get; init; }
    public TextureFormat SemanticFormat { get; init; }
    public TransferMode TransferMode { get; init; }
    public SyncMode SyncMode { get; init; }
    public uint DroppedFrameCount { get; init; }

    internal static FrameInfo FromNative(NativeMethods.FrameInfo native)
    {
        return new FrameInfo
        {
            FrameIndex = native.FrameIndex,
            TimestampNs = native.TimestampNs,
            Width = native.Width,
            Height = native.Height,
            Format = (TextureFormat)native.Format,
            SemanticFormat = (TextureFormat)native.SemanticFormat,
            TransferMode = (TransferMode)native.TransferMode,
            SyncMode = (SyncMode)native.SyncMode,
            DroppedFrameCount = native.DroppedFrameCount,
        };
    }
}

public readonly struct SenderInfo
{
    public string Name { get; init; }
    public string ApplicationName { get; init; }
    public string Id { get; init; }
    public BackendType Backend { get; init; }

    public SenderInfo() { }

    internal static SenderInfo FromNative(NativeMethods.SenderInfo native)
    {
        unsafe
        {
            return new SenderInfo
            {
                Name = Marshal.PtrToStringUTF8((nint)native.Name) ?? "",
                ApplicationName = Marshal.PtrToStringUTF8((nint)native.ApplicationName) ?? "",
                Id = Marshal.PtrToStringUTF8((nint)native.Id) ?? "",
                Backend = (BackendType)native.Backend,
            };
        }
    }
}

public readonly struct ConnectedSenderInfo
{
    public string Name { get; init; }
    public string ApplicationName { get; init; }
    public string Id { get; init; }
    public BackendType Backend { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public TextureFormat Format { get; init; }
    public TextureFormat SemanticFormat { get; init; }
    public double EstimatedFps { get; init; }
    public ulong FrameCounter { get; init; }
    public ulong LastUpdateTimeNs { get; init; }
    public ulong NativeFormatModifier { get; init; }

    public ConnectedSenderInfo() { }

    internal static ConnectedSenderInfo FromNative(NativeMethods.ConnectedSenderInfo native)
    {
        unsafe
        {
            return new ConnectedSenderInfo
            {
                Name = Marshal.PtrToStringUTF8((nint)native.Name) ?? "",
                ApplicationName = Marshal.PtrToStringUTF8((nint)native.ApplicationName) ?? "",
                Id = Marshal.PtrToStringUTF8((nint)native.Id) ?? "",
                Backend = (BackendType)native.Backend,
                Width = native.Width,
                Height = native.Height,
                Format = (TextureFormat)native.Format,
                SemanticFormat = (TextureFormat)native.SemanticFormat,
                EstimatedFps = native.EstimatedFps,
                FrameCounter = native.FrameCounter,
                LastUpdateTimeNs = native.LastUpdateTimeNs,
                NativeFormatModifier = native.NativeFormatModifier,
            };
        }
    }
}

public readonly struct MappedPixels
{
    public nint Data { get; init; }
    public uint RowBytes { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public TextureFormat Format { get; init; }

    internal static MappedPixels FromNative(NativeMethods.MappedPixels native)
    {
        unsafe
        {
            return new MappedPixels
            {
                Data = (nint)native.Data,
                RowBytes = native.RowBytes,
                Width = native.Width,
                Height = native.Height,
                Format = (TextureFormat)native.Format,
            };
        }
    }
}
