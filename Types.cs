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

public readonly struct ResolvedTextureFormat
{
    public TextureFormat StorageFormat { get; init; }
    public TextureFormat SemanticFormat { get; init; }
    public FormatSource FormatSource { get; init; }
    public BackendType NativeBackend { get; init; }
    public NativeFormatKind NativeKind { get; init; }
    public uint NativeValue { get; init; }
    public uint ChannelOrder { get; init; }
    public uint ComponentType { get; init; }
    public byte ComponentBits { get; init; }
    public byte ChannelCount { get; init; }
    public byte BytesPerPixel { get; init; }

    internal static ResolvedTextureFormat FromNative(NativeMethods.ResolvedTextureFormat native)
    {
        return new ResolvedTextureFormat
        {
            StorageFormat = (TextureFormat)native.StorageFormat,
            SemanticFormat = (TextureFormat)native.SemanticFormat,
            FormatSource = (FormatSource)native.FormatSource,
            NativeBackend = (BackendType)native.NativeBackend,
            NativeKind = (NativeFormatKind)native.NativeKind,
            NativeValue = native.NativeValue,
            ChannelOrder = native.ChannelOrder,
            ComponentType = native.ComponentType,
            ComponentBits = native.ComponentBits,
            ChannelCount = native.ChannelCount,
            BytesPerPixel = native.BytesPerPixel,
        };
    }
}

public readonly struct MappedPixels
{
    public nint Data { get; init; }
    public long RowStrideBytes { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public TextureFormat Format { get; init; }
    public TextureOrigin Origin { get; init; }

    internal static MappedPixels FromNative(NativeMethods.MappedPixels native)
    {
        unsafe
        {
            return new MappedPixels
            {
                Data = (nint)native.Data,
                RowStrideBytes = native.RowStrideBytes,
                Width = native.Width,
                Height = native.Height,
                Format = (TextureFormat)native.Format,
                Origin = (TextureOrigin)native.Origin,
            };
        }
    }
}
