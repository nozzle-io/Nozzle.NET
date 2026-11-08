# Nozzle.NET

> This codebase is currently in its AI-slob prototyping phase: the code runs on momentum, vibes, and plausible intent.
> Proper debugging will be introduced once demand graduates from hypothetical to measurable.

.NET 8 P/Invoke bindings for [nozzle](https://github.com/nozzle-io/nozzle) — cross-platform inter-process GPU texture sharing.

Uses `[LibraryImport]` with source-generated marshalling (requires .NET 7+).

## Disclaimer / Notice

This library is currently a work in progress and contains many incomplete features and unverified implementations.
Although it may appear usable at first glance, it may not function correctly.

Please use it with the understanding that no guarantees are made regarding its behavior, and perform debugging, validation, and review as needed.
If you encounter problems, please do not become angry; instead, contributions in the form of Issues or Pull Requests would be greatly appreciated.

## Build

```bash
dotnet build
```

Requires .NET 8 SDK.

## Usage

### Sender

```csharp
using Nozzle;

using var sender = Sender.Create("my_output", "MyApp");
using var frame = sender.AcquireWritableFrame(1920, 1080, TextureFormat.Rgba8Unorm);
var pixels = frame.LockWritablePixels();
// write pixel data via pixels.Data pointer...
frame.UnlockWritablePixels();
sender.CommitFrame(frame);
```

### Receiver

```csharp
using Nozzle;

using var receiver = Receiver.Create("my_output", "MyViewer");
var frame = receiver.AcquireFrame(timeoutMs: 1000);
if (frame != null)
{
    using (frame)
    {
        var info = frame.GetInfo();
        Console.WriteLine($"{info.Width}x{info.Height} format={info.Format}");
    }
}
```

### Discovery

```csharp
using Nozzle;

var senders = Discovery.EnumerateSenders();
foreach (var s in senders)
{
    Console.WriteLine($"{s.Name} ({s.ApplicationName}) — {s.Backend}");
}
```

### Owned Metadata Snapshots

`Sender.GetInfo()` and `Receiver.GetConnectedInfo()` copy native snapshots into
owned managed strings and values. Their signatures, metadata fields, defaults,
and native-error exception behavior are unchanged. Discovery still copies its
separately allocated enumeration before freeing it.

`Receiver.GetConnectionSnapshot()` additionally returns a `ConnectionSnapshot`
with `Connected` and `Info` from one native snapshot call. This is the receiver's
last-observed state, not a fresh liveness check or a reconnect operation. After
an acquisition observes disconnection, `Connected` is false and `Info` retains
the last-known sender metadata. `GetConnectedInfo()` projects that metadata;
successful metadata retrieval does not itself imply a connection.

Returned values survive later calls and disposal of either handle. The snapshot
getters keep their managed owner alive for the current call; this prevents
finalization during that call, not concurrent explicit disposal. Disposal
must not race with calls on the same handle; this change does not establish
general wrapper thread safety. `AcquireFrame()` still returns null for timeout,
sender-closed, and sender-not-found, so null alone does not identify the cause.
Getter calls on disposed handles still throw `NozzleException` with
`ErrorCode.ErrorInvalidArgument`.

## API Coverage

All functions from `nozzle_c.h` are bound:

- Sender: create, destroy, acquire_writable_frame, commit_frame, get_info, get_info_snapshot, publish_texture, publish_gl_texture, publish_native_texture
- Receiver: create, destroy, acquire_frame, get_connected_info, get_connected_info_snapshot
- Frame: release, get_info, lock_pixels, unlock_pixels, lock_writable_pixels, unlock_writable_pixels, copy_to_gl_texture, copy_to_native_texture
- Texture: wrap, destroy
- Discovery: enumerate_senders, free_sender_info_array
- Device: get_default, destroy

## CI Status

CI on macOS, Windows, and Linux compiles a [header-only ABI probe](tests/native_abi/README.md) and compares the legacy connected-info and new snapshot declarations with compiler-derived sizes and offsets, including native-format kind/value/modifier. Snapshot alignment and inline string capacities are also checked. Portable tests cover bounded UTF-8 decoding, owned conversions, synthetic scalar preservation, enum contracts, and managed lifetime logic. This portable coverage is not real getter or GPU evidence.

macOS additionally builds the exact pinned core dylib and runs one required same-process metadata smoke through the public getters with real Metal-backed sender/receiver handles. A test-only resolver loads the explicit built path; missing setup/library/backend fails, and CI checks that the named case executed and passed exactly once. It checks UTF-8 identity, retained owned values, observed disconnection, and disposed-getter errors, without publishing frames. It does not establish pixel correctness, cross-process behavior, concurrent disposal safety, or Windows/Linux runtime getter coverage. Older native creation placeholders remain skipped. The snapshot migration has no claimed deterministic preexisting public-API behavioral red; passing CI is not complete P/Invoke or runtime interoperability validation.

## License

MIT
