using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace Nozzle.Tests;

[Trait("Category", "NativeMetadata")]
public class NativeInfoSnapshotTests
{
    // One resolver per assembly. Keep the dylib loaded for cached P/Invoke addresses.
    private static readonly Lazy<nint> Library = new(() =>
    {
        Assert.True(OperatingSystem.IsMacOS(), "NativeMetadata requires the macOS CI setup.");
        var path = Environment.GetEnvironmentVariable("NOZZLE_NATIVE_LIBRARY");
        Assert.False(string.IsNullOrWhiteSpace(path), "NOZZLE_NATIVE_LIBRARY is required.");
        Assert.True(Path.IsPathFullyQualified(path!), "Use the exact built dylib's absolute path.");
        Assert.True(File.Exists(path), $"Native library missing: {path}");
        var handle = NativeLibrary.Load(path!);
        NativeLibrary.SetDllImportResolver(typeof(Sender).Assembly,
            (name, assembly, searchPath) => name == "nozzle" ? handle : IntPtr.Zero);
        return handle;
    });

    private readonly ITestOutputHelper _output;

    public NativeInfoSnapshotTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Real_getters_return_owned_metadata_and_observed_disconnect()
    {
        _ = Library.Value;
        _output.WriteLine($"Native library: {Environment.GetEnvironmentVariable("NOZZLE_NATIVE_LIBRARY")}");
        var name = "net193-" + Guid.NewGuid().ToString("N") + "-\u65e5\U0001f680";
        const string application = "snapshot-\u65e5\u672c\U0001f680";
        using var sender = Sender.Create(name, application);
        using var receiver = Receiver.Create(name, "snapshot-reader");

        var senderInfo = sender.GetInfo();
        var connectedInfo = receiver.GetConnectedInfo();
        var connected = receiver.GetConnectionSnapshot();
        Assert.Equal(name, senderInfo.Name);
        Assert.Equal(application, senderInfo.ApplicationName);
        Assert.False(string.IsNullOrEmpty(senderInfo.Id));
        Assert.Equal(BackendType.Metal, senderInfo.Backend);
        Assert.True(connected.Connected);
        Assert.Equal(senderInfo.Name, connected.Info.Name);
        Assert.Equal(senderInfo.ApplicationName, connected.Info.ApplicationName);
        Assert.Equal(senderInfo.Id, connected.Info.Id);
        Assert.Equal(senderInfo.Backend, connected.Info.Backend);
        Assert.Equal(connectedInfo, connected.Info);
        Assert.Equal(senderInfo, sender.GetInfo());
        _output.WriteLine($"Initial: id={senderInfo.Id}, connected={connected.Connected}, backend={senderInfo.Backend}");

        // No frames or mappings retain the sender. Null acquisition alone does not
        // distinguish timeout, sender-closed, or sender-not-found in this wrapper.
        sender.Dispose();
        using var frame = receiver.AcquireFrame(timeoutMs: 0);
        Assert.Null(frame);
        var disconnected = receiver.GetConnectionSnapshot();
        Assert.False(disconnected.Connected);
        Assert.Equal(connected.Info, disconnected.Info);
        Assert.Equal(disconnected.Info, receiver.GetConnectedInfo());
        _output.WriteLine($"After disposal/acquire: id={disconnected.Info.Id}, connected={disconnected.Connected}");
        receiver.Dispose();

        Assert.Equal(ErrorCode.ErrorInvalidArgument, Assert.Throws<NozzleException>(() => sender.GetInfo()).ErrorCode);
        Assert.Equal(ErrorCode.ErrorInvalidArgument, Assert.Throws<NozzleException>(() => receiver.GetConnectedInfo()).ErrorCode);
        Assert.Equal(ErrorCode.ErrorInvalidArgument, Assert.Throws<NozzleException>(() => receiver.GetConnectionSnapshot()).ErrorCode);
        Assert.Equal(name, senderInfo.Name);
        Assert.Equal(application, senderInfo.ApplicationName);
        Assert.Equal(name, connectedInfo.Name);
        Assert.Equal(application, connectedInfo.ApplicationName);
        Assert.Equal(senderInfo.Id, connectedInfo.Id);
        Assert.True(connected.Connected);
        Assert.Equal(connectedInfo, connected.Info);
        Assert.Equal(connectedInfo, disconnected.Info);
    }
}
