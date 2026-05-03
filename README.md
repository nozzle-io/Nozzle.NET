# Nozzle.NET

> This codebase is currently in its AI-slob prototyping phase: the code runs on momentum, vibes, and plausible intent.
> Proper debugging will be introduced once demand graduates from hypothetical to measurable.

.NET 8 P/Invoke bindings for [nozzle](https://github.com/nozzle-io/nozzle) — cross-platform inter-process GPU texture sharing.

Uses `[LibraryImport]` with source-generated marshalling (requires .NET 7+).

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

## API Coverage

All functions from `nozzle_c.h` are bound:

- Sender: create, destroy, acquire_writable_frame, commit_frame, get_info, publish_texture, publish_gl_texture, publish_native_texture
- Receiver: create, destroy, acquire_frame, get_connected_info
- Frame: release, get_info, lock_pixels, unlock_pixels, lock_writable_pixels, unlock_writable_pixels, copy_to_gl_texture, copy_to_native_texture
- Texture: wrap, destroy
- Discovery: enumerate_senders, free_sender_info_array
- Device: get_default, destroy

## CI Status

CI is **green but does not validate P/Invoke correctness**. The test suite contains 23 unit tests (enum values, default constructors, etc.) that pass without the native library. The 2 tests that actually call native functions are marked `[Skip]` because CI has no `libnozzle` available. P/Invoke marshalling correctness is only verifiable in an environment with the native library present.

## License

MIT
