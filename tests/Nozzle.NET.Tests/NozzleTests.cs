using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace Nozzle.Tests;

public class ErrorCodeTests
{
    [Fact]
    public void Ok_has_value_zero()
    {
        Assert.Equal(0, (int)ErrorCode.Ok);
    }

    [Fact]
    public void All_error_codes_have_distinct_values()
    {
        var values = Enum.GetValues<ErrorCode>();
        var distinct = new HashSet<int>(values.Select(v => (int)v));
        Assert.Equal(values.Length, distinct.Count);
    }

    [Fact]
    public void Error_codes_convert_to_int_and_back()
    {
        foreach (ErrorCode code in Enum.GetValues<ErrorCode>())
        {
            Assert.Equal(code, (ErrorCode)(int)code);
        }
    }
}

public class BackendTypeTests
{
    [Fact]
    public void Unknown_is_zero()
    {
        Assert.Equal(0, (int)BackendType.Unknown);
    }

    [Fact]
    public void Known_backends_have_nonzero_values()
    {
        Assert.NotEqual(0, (int)BackendType.D3D11);
        Assert.NotEqual(0, (int)BackendType.Metal);
        Assert.NotEqual(0, (int)BackendType.OpenGL);
        Assert.NotEqual(0, (int)BackendType.DmaBuf);
    }

    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)BackendType.Unknown);
        Assert.Equal(1, (int)BackendType.D3D11);
        Assert.Equal(2, (int)BackendType.Metal);
        Assert.Equal(3, (int)BackendType.OpenGL);
        Assert.Equal(4, (int)BackendType.DmaBuf);
    }
}

public class TextureFormatTests
{
    [Fact]
    public void Unknown_is_zero()
    {
        Assert.Equal(0, (int)TextureFormat.Unknown);
    }

    [Fact]
    public void All_formats_have_distinct_values()
    {
        var values = Enum.GetValues<TextureFormat>();
        var distinct = new HashSet<int>(values.Select(v => (int)v));
        Assert.Equal(values.Length, distinct.Count);
    }

    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)TextureFormat.Unknown);
        Assert.Equal(1, (int)TextureFormat.R8Unorm);
        Assert.Equal(2, (int)TextureFormat.Rg8Unorm);
        Assert.Equal(3, (int)TextureFormat.Rgb8Unorm);
        Assert.Equal(4, (int)TextureFormat.Rgba8Unorm);
        Assert.Equal(5, (int)TextureFormat.Bgra8Unorm);
        Assert.Equal(6, (int)TextureFormat.Rgba8Srgb);
        Assert.Equal(7, (int)TextureFormat.Bgra8Srgb);
        Assert.Equal(8, (int)TextureFormat.R16Unorm);
        Assert.Equal(9, (int)TextureFormat.Rg16Unorm);
        Assert.Equal(10, (int)TextureFormat.Rgb16Unorm);
        Assert.Equal(11, (int)TextureFormat.Rgba16Unorm);
        Assert.Equal(12, (int)TextureFormat.R16Float);
        Assert.Equal(13, (int)TextureFormat.Rg16Float);
        Assert.Equal(14, (int)TextureFormat.Rgb16Float);
        Assert.Equal(15, (int)TextureFormat.Rgba16Float);
        Assert.Equal(16, (int)TextureFormat.R32Float);
        Assert.Equal(17, (int)TextureFormat.Rg32Float);
        Assert.Equal(18, (int)TextureFormat.Rgb32Float);
        Assert.Equal(19, (int)TextureFormat.Rgba32Float);
        Assert.Equal(20, (int)TextureFormat.R32Uint);
        Assert.Equal(21, (int)TextureFormat.Rgba32Uint);
        Assert.Equal(22, (int)TextureFormat.Rgb32Uint);
        Assert.Equal(23, (int)TextureFormat.Depth32Float);
    }

    [Fact]
    public void Has_24_values_matching_c_enum()
    {
        Assert.Equal(24, Enum.GetValues<TextureFormat>().Length);
    }
}

public class TransferModeTests
{
    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)TransferMode.Unknown);
        Assert.Equal(1, (int)TransferMode.ZeroCopySharedTexture);
        Assert.Equal(2, (int)TransferMode.GpuCopy);
        Assert.Equal(3, (int)TransferMode.CpuCopy);
    }
}

public class SyncModeTests
{
    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)SyncMode.None);
        Assert.Equal(1, (int)SyncMode.AccessGuarded);
        Assert.Equal(2, (int)SyncMode.GpuFenceBestEffort);
    }
}

public class ReceiveModeTests
{
    [Fact]
    public void LatestOnly_is_zero()
    {
        Assert.Equal(0, (int)ReceiveMode.LatestOnly);
    }

    [Fact]
    public void SequentialBestEffort_is_one()
    {
        Assert.Equal(1, (int)ReceiveMode.SequentialBestEffort);
    }
}

public class FrameStatusTests
{
    [Fact]
    public void New_is_zero()
    {
        Assert.Equal(0, (int)FrameStatus.New);
    }

    [Fact]
    public void All_statuses_have_distinct_values()
    {
        var values = Enum.GetValues<FrameStatus>();
        var distinct = new HashSet<int>(values.Select(v => (int)v));
        Assert.Equal(values.Length, distinct.Count);
    }
}

public class NozzleExceptionTests
{
    [Fact]
    public void Constructor_sets_error_code()
    {
        var ex = new NozzleException(ErrorCode.ErrorInvalidArgument);
        Assert.Equal(ErrorCode.ErrorInvalidArgument, ex.ErrorCode);
    }

    [Fact]
    public void Constructor_with_message_sets_both()
    {
        var ex = new NozzleException(ErrorCode.ErrorTimeout, "test message");
        Assert.Equal(ErrorCode.ErrorTimeout, ex.ErrorCode);
        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void Constructor_without_message_uses_error_code_tostring()
    {
        var ex = new NozzleException(ErrorCode.ErrorSenderNotFound);
        Assert.Equal(ErrorCode.ErrorSenderNotFound.ToString(), ex.Message);
    }

    [Fact]
    public void Is_exception()
    {
        var ex = new NozzleException(ErrorCode.Ok);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

public class FrameInfoTests
{
    [Fact]
    public void Default_values_are_zero_or_unknown()
    {
        var info = new FrameInfo();
        Assert.Equal(0ul, info.FrameIndex);
        Assert.Equal(0ul, info.TimestampNs);
        Assert.Equal(0u, info.Width);
        Assert.Equal(0u, info.Height);
        Assert.Equal(TextureFormat.Unknown, info.Format);
        Assert.Equal(TextureFormat.Unknown, info.SemanticFormat);
        Assert.Equal(TransferMode.Unknown, info.TransferMode);
        Assert.Equal(SyncMode.None, info.SyncMode);
        Assert.Equal(0u, info.DroppedFrameCount);
    }

    [Fact]
    public void Init_properties_work()
    {
        var info = new FrameInfo
        {
            FrameIndex = 42,
            TimestampNs = 12345,
            Width = 1920,
            Height = 1080,
            Format = TextureFormat.Rgba32Float,
            SemanticFormat = TextureFormat.Rgba8Unorm,
            TransferMode = TransferMode.ZeroCopySharedTexture,
            SyncMode = SyncMode.AccessGuarded,
            DroppedFrameCount = 3,
        };

        Assert.Equal(42ul, info.FrameIndex);
        Assert.Equal(12345ul, info.TimestampNs);
        Assert.Equal(1920u, info.Width);
        Assert.Equal(1080u, info.Height);
        Assert.Equal(TextureFormat.Rgba32Float, info.Format);
        Assert.Equal(TextureFormat.Rgba8Unorm, info.SemanticFormat);
        Assert.Equal(TransferMode.ZeroCopySharedTexture, info.TransferMode);
        Assert.Equal(SyncMode.AccessGuarded, info.SyncMode);
        Assert.Equal(3u, info.DroppedFrameCount);
    }
}

public class SenderInfoTests
{
    [Fact]
    public void Default_constructor_exists()
    {
        var info = new SenderInfo();
        Assert.Null(info.Name);
        Assert.Null(info.ApplicationName);
        Assert.Null(info.Id);
        Assert.Equal(BackendType.Unknown, info.Backend);
    }

    [Fact]
    public void Init_properties_work()
    {
        var info = new SenderInfo
        {
            Name = "test_sender",
            ApplicationName = "TestApp",
            Id = "abc123",
            Backend = BackendType.Metal,
        };

        Assert.Equal("test_sender", info.Name);
        Assert.Equal("TestApp", info.ApplicationName);
        Assert.Equal("abc123", info.Id);
        Assert.Equal(BackendType.Metal, info.Backend);
    }
}

public class ConnectedSenderInfoTests
{
    [Fact]
    public void Default_constructor_exists()
    {
        var info = new ConnectedSenderInfo();
        Assert.Null(info.Name);
        Assert.Null(info.ApplicationName);
        Assert.Null(info.Id);
        Assert.Equal(BackendType.Unknown, info.Backend);
        Assert.Equal(0u, info.Width);
        Assert.Equal(0u, info.Height);
        Assert.Equal(TextureFormat.Unknown, info.Format);
        Assert.Equal(TextureFormat.Unknown, info.SemanticFormat);
        Assert.Equal(0.0, info.EstimatedFps);
        Assert.Equal(0ul, info.FrameCounter);
        Assert.Equal(0ul, info.LastUpdateTimeNs);
        Assert.Equal(0ul, info.NativeFormatModifier);
    }

    [Fact]
    public void Init_properties_work()
    {
        var info = new ConnectedSenderInfo
        {
            Name = "test",
            ApplicationName = "App",
            Id = "id",
            Backend = BackendType.D3D11,
            Width = 640,
            Height = 480,
            Format = TextureFormat.Rgba8Unorm,
            SemanticFormat = TextureFormat.Rgba16Float,
            EstimatedFps = 60.0,
            FrameCounter = 100,
            LastUpdateTimeNs = 99999,
            NativeFormatModifier = 0xdeadbeef,
        };

        Assert.Equal("test", info.Name);
        Assert.Equal(640u, info.Width);
        Assert.Equal(480u, info.Height);
        Assert.Equal(TextureFormat.Rgba8Unorm, info.Format);
        Assert.Equal(TextureFormat.Rgba16Float, info.SemanticFormat);
        Assert.Equal(60.0, info.EstimatedFps);
        Assert.Equal(100ul, info.FrameCounter);
        Assert.Equal(0xdeadbeeful, info.NativeFormatModifier);
    }
}

public class MappedPixelsTests
{
    [Fact]
    public void Default_values_are_zero()
    {
        var pixels = new MappedPixels();
        Assert.Equal(nint.Zero, pixels.Data);
        Assert.Equal(0L, pixels.RowStrideBytes);
        Assert.Equal(0u, pixels.Width);
        Assert.Equal(0u, pixels.Height);
        Assert.Equal(TextureFormat.Unknown, pixels.Format);
        Assert.Equal(TextureOrigin.TopLeft, pixels.Origin);
    }

    [Fact]
    public void Init_properties_work()
    {
        var pixels = new MappedPixels
        {
            Data = (nint)0x1234,
            RowStrideBytes = 7680,
            Width = 1920,
            Height = 1080,
            Format = TextureFormat.Rgba8Unorm,
            Origin = TextureOrigin.BottomLeft,
        };
        Assert.Equal((nint)0x1234, pixels.Data);
        Assert.Equal(7680L, pixels.RowStrideBytes);
        Assert.Equal(1920u, pixels.Width);
        Assert.Equal(1080u, pixels.Height);
        Assert.Equal(TextureFormat.Rgba8Unorm, pixels.Format);
        Assert.Equal(TextureOrigin.BottomLeft, pixels.Origin);
    }
}

public class TextureOriginTests
{
    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)TextureOrigin.TopLeft);
        Assert.Equal(1, (int)TextureOrigin.BottomLeft);
    }
}

public class NativeSenderTests
{
    [Fact(Skip = "Requires native nozzle library")]
    public void Create_throws_without_native_lib()
    {
        Sender.Create("test", "test");
    }
}

public class NativeReceiverTests
{
    [Fact(Skip = "Requires native nozzle library")]
    public void Create_throws_without_native_lib()
    {
        Receiver.Create("test", "test");
    }
}

public class NativeFrameInfoLayoutTests
{
    [Fact]
    public void Size_matches_c_abi()
    {
        Assert.Equal(48, Marshal.SizeOf<NativeMethods.FrameInfo>());
    }

    [Fact]
    public void Field_offsets_match_c_abi()
    {
        Assert.Equal(0, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("FrameIndex"));
        Assert.Equal(8, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("TimestampNs"));
        Assert.Equal(16, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("Width"));
        Assert.Equal(20, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("Height"));
        Assert.Equal(24, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("Format"));
        Assert.Equal(28, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("SemanticFormat"));
        Assert.Equal(32, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("TransferMode"));
        Assert.Equal(36, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("SyncMode"));
        Assert.Equal(40, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("DroppedFrameCount"));
    }

    [Fact]
    public void SemanticFormat_is_at_offset_28()
    {
        Assert.Equal(28, (int)Marshal.OffsetOf<NativeMethods.FrameInfo>("SemanticFormat"));
    }

    [Fact]
    public void FromNative_maps_semantic_format()
    {
        var native = new NativeMethods.FrameInfo
        {
            SemanticFormat = NativeMethods.TextureFormat.Rgba8Srgb,
        };
        var managed = FrameInfo.FromNative(native);
        Assert.Equal(TextureFormat.Rgba8Srgb, managed.SemanticFormat);
    }

    [Fact]
    public void FromNative_maps_transfer_mode()
    {
        var native = new NativeMethods.FrameInfo
        {
            TransferMode = NativeMethods.TransferMode.GpuCopy,
        };
        var managed = FrameInfo.FromNative(native);
        Assert.Equal(TransferMode.GpuCopy, managed.TransferMode);
    }

    [Fact]
    public void FromNative_maps_sync_mode()
    {
        var native = new NativeMethods.FrameInfo
        {
            SyncMode = NativeMethods.SyncMode.AccessGuarded,
        };
        var managed = FrameInfo.FromNative(native);
        Assert.Equal(SyncMode.AccessGuarded, managed.SyncMode);
    }

    [Fact]
    public void FromNative_maps_all_fields()
    {
        var native = new NativeMethods.FrameInfo
        {
            FrameIndex = 1,
            TimestampNs = 2,
            Width = 3,
            Height = 4,
            Format = NativeMethods.TextureFormat.Rgba8Unorm,
            SemanticFormat = NativeMethods.TextureFormat.Rgba16Float,
            TransferMode = NativeMethods.TransferMode.ZeroCopySharedTexture,
            SyncMode = NativeMethods.SyncMode.GpuFenceBestEffort,
            DroppedFrameCount = 7,
        };
        var managed = FrameInfo.FromNative(native);

        Assert.Equal(1ul, managed.FrameIndex);
        Assert.Equal(2ul, managed.TimestampNs);
        Assert.Equal(3u, managed.Width);
        Assert.Equal(4u, managed.Height);
        Assert.Equal(TextureFormat.Rgba8Unorm, managed.Format);
        Assert.Equal(TextureFormat.Rgba16Float, managed.SemanticFormat);
        Assert.Equal(TransferMode.ZeroCopySharedTexture, managed.TransferMode);
        Assert.Equal(SyncMode.GpuFenceBestEffort, managed.SyncMode);
        Assert.Equal(7u, managed.DroppedFrameCount);
    }
}

public class NativeConnectedSenderInfoLayoutTests
{
    [Fact]
    public void Size_matches_c_abi()
    {
        Assert.Equal(80, Marshal.SizeOf<NativeMethods.ConnectedSenderInfo>());
    }

    [Fact]
    public void Field_offsets_match_c_abi()
    {
        Assert.Equal(0, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("Name"));
        Assert.Equal(8, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("ApplicationName"));
        Assert.Equal(16, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("Id"));
        Assert.Equal(24, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("Backend"));
        Assert.Equal(28, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("Width"));
        Assert.Equal(32, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("Height"));
        Assert.Equal(36, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("Format"));
        Assert.Equal(40, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("SemanticFormat"));
        Assert.Equal(48, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("EstimatedFps"));
        Assert.Equal(56, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("FrameCounter"));
        Assert.Equal(64, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("LastUpdateTimeNs"));
        Assert.Equal(72, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("NativeFormatModifier"));
    }

    [Fact]
    public void SemanticFormat_is_at_offset_40()
    {
        Assert.Equal(40, (int)Marshal.OffsetOf<NativeMethods.ConnectedSenderInfo>("SemanticFormat"));
    }

    [Fact]
    public void FromNative_maps_semantic_format()
    {
        unsafe
        {
            var native = new NativeMethods.ConnectedSenderInfo
            {
                SemanticFormat = NativeMethods.TextureFormat.Rgba32Float,
            };
            var managed = ConnectedSenderInfo.FromNative(native);
            Assert.Equal(TextureFormat.Rgba32Float, managed.SemanticFormat);
        }
    }

    [Fact]
    public void FromNative_maps_native_format_modifier()
    {
        unsafe
        {
            var native = new NativeMethods.ConnectedSenderInfo
            {
                NativeFormatModifier = 0x123456789ABCDEF0,
            };
            var managed = ConnectedSenderInfo.FromNative(native);
            Assert.Equal(0x123456789ABCDEF0ul, managed.NativeFormatModifier);
        }
    }
}

public class NativeMappedPixelsLayoutTests
{
    [Fact]
    public void Size_matches_c_abi()
    {
        Assert.Equal(32, Marshal.SizeOf<NativeMethods.MappedPixels>());
    }

    [Fact]
    public void Field_offsets_match_c_abi()
    {
        Assert.Equal(0, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("Data"));
        Assert.Equal(8, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("RowStrideBytes"));
        Assert.Equal(16, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("Width"));
        Assert.Equal(20, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("Height"));
        Assert.Equal(24, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("Format"));
        Assert.Equal(28, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("Origin"));
    }

    [Fact]
    public void RowStrideBytes_is_8_bytes()
    {
        Assert.Equal(8, (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("Width") -
                        (int)Marshal.OffsetOf<NativeMethods.MappedPixels>("RowStrideBytes"));
    }

    [Fact]
    public void FromNative_maps_all_fields()
    {
        unsafe
        {
            var native = new NativeMethods.MappedPixels
            {
                Data = (void*)0xDEAD,
                RowStrideBytes = 7680,
                Width = 1920,
                Height = 1080,
                Format = NativeMethods.TextureFormat.Rgba8Unorm,
                Origin = NativeMethods.TextureOrigin.BottomLeft,
            };
            var managed = MappedPixels.FromNative(native);
            Assert.Equal((nint)0xDEAD, managed.Data);
            Assert.Equal(7680L, managed.RowStrideBytes);
            Assert.Equal(1920u, managed.Width);
            Assert.Equal(1080u, managed.Height);
            Assert.Equal(TextureFormat.Rgba8Unorm, managed.Format);
            Assert.Equal(TextureOrigin.BottomLeft, managed.Origin);
        }
    }
}

public class FormatSourceTests
{
    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)FormatSource.Unknown);
        Assert.Equal(1, (int)FormatSource.Requested);
        Assert.Equal(2, (int)FormatSource.CallerHint);
        Assert.Equal(3, (int)FormatSource.NativeObserved);
    }
}

public class NativeFormatKindTests
{
    [Fact]
    public void Values_match_c_abi()
    {
        Assert.Equal(0, (int)NativeFormatKind.Unknown);
        Assert.Equal(1, (int)NativeFormatKind.MtlPixelFormat);
        Assert.Equal(2, (int)NativeFormatKind.DxgiFormat);
        Assert.Equal(3, (int)NativeFormatKind.DrmFourcc);
        Assert.Equal(4, (int)NativeFormatKind.GlInternalFormat);
    }
}

public class NativeResolvedTextureFormatLayoutTests
{
    [Fact]
    public void Size_matches_c_abi()
    {
        Assert.Equal(36, Marshal.SizeOf<NativeMethods.ResolvedTextureFormat>());
    }

    [Fact]
    public void Field_offsets_match_c_abi()
    {
        Assert.Equal(0, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("StorageFormat"));
        Assert.Equal(4, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("SemanticFormat"));
        Assert.Equal(8, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("FormatSource"));
        Assert.Equal(12, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("NativeBackend"));
        Assert.Equal(16, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("NativeKind"));
        Assert.Equal(20, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("NativeValue"));
        Assert.Equal(24, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("ChannelOrder"));
        Assert.Equal(28, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("ComponentType"));
        Assert.Equal(32, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("ComponentBits"));
        Assert.Equal(33, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("ChannelCount"));
        Assert.Equal(34, (int)Marshal.OffsetOf<NativeMethods.ResolvedTextureFormat>("BytesPerPixel"));
    }

    [Fact]
    public void FromNative_maps_all_fields()
    {
        var native = new NativeMethods.ResolvedTextureFormat
        {
            StorageFormat = NativeMethods.TextureFormat.Rgba8Unorm,
            SemanticFormat = NativeMethods.TextureFormat.Rgba8Srgb,
            FormatSource = NativeMethods.FormatSource.NativeObserved,
            NativeBackend = NativeMethods.BackendType.Metal,
            NativeKind = NativeMethods.NativeFormatKind.MtlPixelFormat,
            NativeValue = 80,
            ChannelOrder = 1,
            ComponentType = 2,
            ComponentBits = 8,
            ChannelCount = 4,
            BytesPerPixel = 4,
        };
        var managed = ResolvedTextureFormat.FromNative(native);

        Assert.Equal(TextureFormat.Rgba8Unorm, managed.StorageFormat);
        Assert.Equal(TextureFormat.Rgba8Srgb, managed.SemanticFormat);
        Assert.Equal(FormatSource.NativeObserved, managed.FormatSource);
        Assert.Equal(BackendType.Metal, managed.NativeBackend);
        Assert.Equal(NativeFormatKind.MtlPixelFormat, managed.NativeKind);
        Assert.Equal(80u, managed.NativeValue);
        Assert.Equal(1u, managed.ChannelOrder);
        Assert.Equal(2u, managed.ComponentType);
        Assert.Equal(8, managed.ComponentBits);
        Assert.Equal(4, managed.ChannelCount);
        Assert.Equal(4, managed.BytesPerPixel);
    }
}
