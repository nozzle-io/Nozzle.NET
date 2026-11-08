using System;
using System.Runtime.InteropServices;
using System.Text;

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

    internal static unsafe SenderInfo FromNative(NativeMethods.SenderInfoSnapshot native)
    {
        return new SenderInfo
        {
            Name = SnapshotText.Decode(new ReadOnlySpan<byte>(native.Name, NativeMethods.SenderNameCapacity)),
            ApplicationName = SnapshotText.Decode(new ReadOnlySpan<byte>(native.ApplicationName, NativeMethods.ApplicationNameCapacity)),
            Id = SnapshotText.Decode(new ReadOnlySpan<byte>(native.Id, NativeMethods.SenderIdCapacity)),
            Backend = (BackendType)native.Backend,
        };
    }

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

    internal static unsafe ConnectedSenderInfo FromNative(NativeMethods.ConnectedSenderSnapshot native)
    {
        return new ConnectedSenderInfo
        {
            Name = SnapshotText.Decode(new ReadOnlySpan<byte>(native.Name, NativeMethods.SenderNameCapacity)),
            ApplicationName = SnapshotText.Decode(new ReadOnlySpan<byte>(native.ApplicationName, NativeMethods.ApplicationNameCapacity)),
            Id = SnapshotText.Decode(new ReadOnlySpan<byte>(native.Id, NativeMethods.SenderIdCapacity)),
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

/// <summary>Owned metadata and the receiver's last-observed connection state, not a fresh liveness check.</summary>
public readonly struct ConnectionSnapshot
{
    public bool Connected { get; init; }
    public ConnectedSenderInfo Info { get; init; }

    internal static ConnectionSnapshot FromNative(NativeMethods.ConnectedSenderSnapshot native)
    {
        return new ConnectionSnapshot
        {
            Connected = native.Connected != 0,
            Info = ConnectedSenderInfo.FromNative(native),
        };
    }
}

internal static class SnapshotText
{
    internal static string Decode(ReadOnlySpan<byte> bytes)
    {
        var nul = bytes.IndexOf((byte)0);
        return Encoding.UTF8.GetString(nul < 0 ? bytes : bytes[..nul]);
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
