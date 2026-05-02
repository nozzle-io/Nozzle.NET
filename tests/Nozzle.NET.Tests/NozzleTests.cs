using System;
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
    public void Rgba32Float_has_expected_value()
    {
        Assert.Equal(15, (int)TextureFormat.Rgba32Float);
    }

    [Fact]
    public void All_formats_have_distinct_values()
    {
        var values = Enum.GetValues<TextureFormat>();
        var distinct = new HashSet<int>(values.Select(v => (int)v));
        Assert.Equal(values.Length, distinct.Count);
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
            DroppedFrameCount = 3,
        };

        Assert.Equal(42ul, info.FrameIndex);
        Assert.Equal(12345ul, info.TimestampNs);
        Assert.Equal(1920u, info.Width);
        Assert.Equal(1080u, info.Height);
        Assert.Equal(TextureFormat.Rgba32Float, info.Format);
        Assert.Equal(3u, info.DroppedFrameCount);
    }
}

public class SenderInfoTests
{
    [Fact]
    public void Default_constructor_exists()
    {
        var info = new SenderInfo();
        Assert.Equal("", info.Name);
        Assert.Equal("", info.ApplicationName);
        Assert.Equal("", info.Id);
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
        Assert.Equal("", info.Name);
        Assert.Equal("", info.ApplicationName);
        Assert.Equal("", info.Id);
        Assert.Equal(BackendType.Unknown, info.Backend);
        Assert.Equal(0u, info.Width);
        Assert.Equal(0u, info.Height);
        Assert.Equal(TextureFormat.Unknown, info.Format);
        Assert.Equal(0.0, info.EstimatedFps);
        Assert.Equal(0ul, info.FrameCounter);
        Assert.Equal(0ul, info.LastUpdateTimeNs);
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
            EstimatedFps = 60.0,
            FrameCounter = 100,
            LastUpdateTimeNs = 99999,
        };

        Assert.Equal("test", info.Name);
        Assert.Equal(640u, info.Width);
        Assert.Equal(480u, info.Height);
        Assert.Equal(TextureFormat.Rgba8Unorm, info.Format);
        Assert.Equal(60.0, info.EstimatedFps);
        Assert.Equal(100ul, info.FrameCounter);
    }
}

public class MappedPixelsTests
{
    [Fact]
    public void Default_values_are_zero()
    {
        var pixels = new MappedPixels();
        Assert.Equal(nint.Zero, pixels.Data);
        Assert.Equal(0u, pixels.RowBytes);
        Assert.Equal(0u, pixels.Width);
        Assert.Equal(0u, pixels.Height);
        Assert.Equal(TextureFormat.Unknown, pixels.Format);
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
