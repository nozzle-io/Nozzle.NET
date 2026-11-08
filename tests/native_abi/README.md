# Connected-Info ABI Probe

This header-only C executable measures the legacy `NozzleConnectedSenderInfo`
layout from the checked-out `deps/nozzle/include/nozzle/nozzle_c.h`. It does not
link core, call the getter, create a receiver, or exercise a GPU backend. Its
JSON reports the native size, pointer width, every field offset, and tail width.
The managed tests compare the actual P/Invoke struct with that compiler output.
No numeric C layout is copied into the test oracle.

With the core submodule initialized, CI configures and builds only this probe:

```sh
cmake -S tests/native_abi -B build/native-abi
cmake --build build/native-abi --config Release
```

On macOS/Linux, generate the fixture and run the focused managed tests:

```sh
build/native-abi/nozzle_info_abi > build/native-abi/connected-info.json
export NOZZLE_INFO_ABI_JSON="$PWD/build/native-abi/connected-info.json"
dotnet test Nozzle.NET.sln --filter FullyQualifiedName~NativeConnectedSenderInfoLayoutTests
```

For the default Windows Visual Studio generator, use PowerShell:

```powershell
build/native-abi/Release/nozzle_info_abi.exe > build/native-abi/connected-info.json
$env:NOZZLE_INFO_ABI_JSON = "$PWD/build/native-abi/connected-info.json"
dotnet test Nozzle.NET.sln --filter FullyQualifiedName~NativeConnectedSenderInfoLayoutTests
```

CI generates and prints fresh probe output on all three existing OS jobs before
running the full managed suite. Missing output, malformed JSON, or a native/managed
pointer-width mismatch fails the tests; none is treated as a skip. The probe and
managed process must target the same architecture. This is not a claim of 32-bit
coverage or runtime getter overwrite reproduction.

The fail-first test compiles against the existing managed struct: it compares
all currently declared field offsets without referring to the two missing
managed fields. The native probe still reports both missing fields. Size and
modifier offset expose the stale declaration before its production correction.
