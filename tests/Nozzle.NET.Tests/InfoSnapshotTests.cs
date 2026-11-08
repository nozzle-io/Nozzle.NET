using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Nozzle.Tests;

public class InfoSnapshotTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SenderAlignment
    {
        public byte Prefix;
        public NativeMethods.SenderInfoSnapshot Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ConnectedAlignment
    {
        public byte Prefix;
        public NativeMethods.ConnectedSenderSnapshot Value;
    }

    [Fact]
    public void Snapshot_layouts_match_native_header_including_inline_buffers_and_tail()
    {
        var path = Environment.GetEnvironmentVariable("NOZZLE_INFO_ABI_JSON");
        Assert.False(string.IsNullOrWhiteSpace(path), "NOZZLE_INFO_ABI_JSON is required.");
        Assert.True(File.Exists(path), $"Native ABI probe output missing: {path}");
        var layout = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(path!))!;
        Assert.NotNull(layout);
        Assert.Equal(IntPtr.Size, layout["pointer_size"]);

        var identity = new (string Managed, string Native)[]
        {
            ("Name", "name"), ("ApplicationName", "application_name"),
            ("Id", "id"), ("Backend", "backend"),
        };
        CheckLayout<NativeMethods.SenderInfoSnapshot>(layout, "sender_snapshot", identity);
        CheckLayout<NativeMethods.ConnectedSenderSnapshot>(layout, "connected_snapshot", identity);
        var fields = new (string Managed, string Native)[]
        {
            ("Connected", "connected"), ("Width", "width"), ("Height", "height"),
            ("Format", "format"), ("SemanticFormat", "semantic_format"),
            ("EstimatedFps", "estimated_fps"), ("FrameCounter", "frame_counter"),
            ("LastUpdateTimeNs", "last_update_time_ns"), ("NativeFormatKind", "native_format_kind"),
            ("NativeFormatValue", "native_format_value"), ("NativeFormatModifier", "native_format_modifier"),
        };
        foreach (var field in fields)
            Assert.Equal(layout["connected_snapshot_" + field.Native],
                (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderSnapshot>(field.Managed));

        Assert.Equal(layout["sender_snapshot_alignment"], (int)Marshal.OffsetOf<SenderAlignment>("Value"));
        Assert.Equal(layout["connected_snapshot_alignment"], (int)Marshal.OffsetOf<ConnectedAlignment>("Value"));
        Assert.Equal(sizeof(ulong), layout["connected_snapshot_native_format_modifier_size"]);
        Assert.True(layout["connected_snapshot_native_format_modifier"] +
            layout["connected_snapshot_native_format_modifier_size"] <=
            Marshal.SizeOf<NativeMethods.ConnectedSenderSnapshot>());
        Assert.Equal(typeof(uint), typeof(NativeMethods.ConnectedSenderSnapshot).GetField("Connected")!.FieldType);
    }

    private static void CheckLayout<T>(Dictionary<string, int> layout, string prefix,
        (string Managed, string Native)[] identity) where T : struct
    {
        Assert.Equal(layout[prefix + "_size"], Marshal.SizeOf<T>());
        foreach (var field in identity)
        {
            Assert.Equal(layout[prefix + "_" + field.Native], (int)Marshal.OffsetOf<T>(field.Managed));
            if (field.Managed == "Backend") continue;
            var buffer = typeof(T).GetField(field.Managed)!.GetCustomAttribute<FixedBufferAttribute>()!;
            Assert.NotNull(buffer);
            Assert.Equal(typeof(byte), buffer.ElementType);
            Assert.Equal(layout[prefix + "_" + field.Native + "_capacity"], buffer.Length);
        }
    }

    [Fact]
    public void Decoder_is_bounded_nul_aware_and_replaces_invalid_utf8()
    {
        Assert.Equal("", SnapshotText.Decode(ReadOnlySpan<byte>.Empty));
        Assert.Equal("", SnapshotText.Decode(new byte[] { 0, 65 }));
        Assert.Equal("A", SnapshotText.Decode(new byte[] { 65, 0, 66 }));
        Assert.Equal("AB", SnapshotText.Decode(new byte[] { 65, 66 }));
        Assert.Equal("\u65e5\u672c\U0001f680", SnapshotText.Decode(Encoding.UTF8.GetBytes("\u65e5\u672c\U0001f680")));
        Assert.Equal("\ufffdA", SnapshotText.Decode(new byte[] { 0xff, 65 }));
    }

    [Fact]
    public unsafe void Sender_conversion_owns_strings_and_bounds_nonterminated_buffers()
    {
        var native = new NativeMethods.SenderInfoSnapshot { Backend = NativeMethods.BackendType.Metal };
        new Span<byte>(native.Name, NativeMethods.SenderNameCapacity).Fill((byte)'N');
        Encoding.UTF8.GetBytes("\u65e5\u672c\U0001f680").CopyTo(new Span<byte>(native.ApplicationName, NativeMethods.ApplicationNameCapacity));
        new Span<byte>(native.Id, NativeMethods.SenderIdCapacity).Fill((byte)'I');
        var info = SenderInfo.FromNative(native);
        native = default;
        Assert.Equal(new string('N', NativeMethods.SenderNameCapacity), info.Name);
        Assert.Equal("\u65e5\u672c\U0001f680", info.ApplicationName);
        Assert.Equal(new string('I', NativeMethods.SenderIdCapacity), info.Id);
        Assert.Equal(BackendType.Metal, info.Backend);
        Assert.Equal("", SenderInfo.FromNative(native).Name);
    }

    [Fact]
    public unsafe void Synthetic_connection_conversion_preserves_public_scalars_and_retained_identity()
    {
        var native = new NativeMethods.ConnectedSenderSnapshot
        {
            Connected = 7, Backend = NativeMethods.BackendType.Metal,
            Width = 65, Height = 3,
            Format = NativeMethods.TextureFormat.Bgra8Unorm,
            SemanticFormat = NativeMethods.TextureFormat.Rgb16Float,
            EstimatedFps = 59.9375,
            FrameCounter = 0xfedcba9876543210UL,
            LastUpdateTimeNs = 0xabcdef0123456789UL,
            NativeFormatKind = NativeMethods.NativeFormatKind.MtlPixelFormat,
            NativeFormatValue = 0x12345678,
            NativeFormatModifier = 0x9876543210abcdefUL,
        };
        Encoding.UTF8.GetBytes("camera\U0001f680").CopyTo(new Span<byte>(native.Name, NativeMethods.SenderNameCapacity));
        Encoding.UTF8.GetBytes("\u65e5\u672c").CopyTo(new Span<byte>(native.ApplicationName, NativeMethods.ApplicationNameCapacity));
        Encoding.UTF8.GetBytes("snapshot-id").CopyTo(new Span<byte>(native.Id, NativeMethods.SenderIdCapacity));
        var snapshot = ConnectionSnapshot.FromNative(native);
        native.Connected = 0;
        var disconnected = ConnectionSnapshot.FromNative(native);
        native = default;

        Assert.True(snapshot.Connected);
        Assert.False(disconnected.Connected);
        Assert.Equal(snapshot.Info, disconnected.Info);
        var info = snapshot.Info;
        Assert.Equal("camera\U0001f680", info.Name);
        Assert.Equal("\u65e5\u672c", info.ApplicationName);
        Assert.Equal("snapshot-id", info.Id);
        Assert.Equal(BackendType.Metal, info.Backend);
        Assert.Equal(65u, info.Width);
        Assert.Equal(3u, info.Height);
        Assert.Equal(TextureFormat.Bgra8Unorm, info.Format);
        Assert.Equal(TextureFormat.Rgb16Float, info.SemanticFormat);
        Assert.Equal(59.9375, info.EstimatedFps);
        Assert.Equal(0xfedcba9876543210UL, info.FrameCounter);
        Assert.Equal(0xabcdef0123456789UL, info.LastUpdateTimeNs);
        Assert.Equal(0x9876543210abcdefUL, info.NativeFormatModifier);

        var empty = ConnectionSnapshot.FromNative(native);
        Assert.False(empty.Connected);
        Assert.Equal("", empty.Info.Name);
        Assert.Equal("", empty.Info.ApplicationName);
        Assert.Equal("", empty.Info.Id);
        Assert.Equal(BackendType.Unknown, empty.Info.Backend);
    }
}
