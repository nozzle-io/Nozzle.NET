# Metadata ABI Probe

This header-only C executable measures the legacy `NozzleConnectedSenderInfo`
and the two owned snapshot layouts from the checked-out
`deps/nozzle/include/nozzle/nozzle_c.h`. It does not
link core, call the getter, create a receiver, or exercise a GPU backend. Its
JSON reports the native sizes, pointer width, every field offset, and tail widths.
For snapshots it also reports inline string capacities and alignment via the
offset of a value following a single char, retaining C99 compatibility.
The managed tests compare the actual P/Invoke struct with that compiler output.
No numeric C layout is copied into the test oracle.

With the core submodule initialized, configure and build this probe:

```sh
cmake -S tests/native_abi -B build/native-abi
cmake --build build/native-abi --config Release
```

On macOS/Linux, generate the fixture and run the focused managed tests:

```sh
build/native-abi/nozzle_info_abi > build/native-abi/connected-info.json
export NOZZLE_INFO_ABI_JSON="$PWD/build/native-abi/connected-info.json"
dotnet test Nozzle.NET.sln --filter "FullyQualifiedName~NativeConnectedSenderInfoLayoutTests|FullyQualifiedName~InfoSnapshotTests&Category!=NativeMetadata"
```

For the default Windows Visual Studio generator, use PowerShell:

```powershell
build/native-abi/Release/nozzle_info_abi.exe > build/native-abi/connected-info.json
$env:NOZZLE_INFO_ABI_JSON = "$PWD/build/native-abi/connected-info.json"
dotnet test Nozzle.NET.sln --filter "FullyQualifiedName~NativeConnectedSenderInfoLayoutTests|FullyQualifiedName~InfoSnapshotTests&Category!=NativeMetadata"
```

CI generates and prints fresh probe output on all three existing OS jobs before
running the portable managed suite (`Category!=NativeMetadata`). Missing output, malformed JSON, or a native/managed
pointer-width mismatch fails the tests; none is treated as a skip. The probe and
managed process must target the same architecture. This is not a claim of 32-bit
coverage or runtime getter overwrite reproduction.

The legacy checks retain the regression for the formerly missing native-format
kind/value fields: size, all field offsets, and the modifier tail must match.
Snapshot checks are additive, not a replacement for that legacy ABI coverage.

## macOS Real Getter Smoke

This separate CI step builds the pinned core shared library, not core tests or
examples, and loads only its absolute path through a once-per-assembly test
resolver. With recursive submodules initialized:

```sh
cmake -S deps/nozzle -B build/native-core -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=ON -DNOZZLE_BUILD_TESTS=OFF -DNOZZLE_BUILD_EXAMPLES=OFF
cmake --build build/native-core --target nozzle --parallel 3
export NOZZLE_NATIVE_LIBRARY="$PWD/build/native-core/libnozzle.dylib"
dotnet test tests/Nozzle.NET.Tests/Nozzle.NET.Tests.csproj --filter "Category=NativeMetadata" --logger "trx;LogFileName=native-metadata.trx" --results-directory build/native-results
```

CI also verifies the checkout matches its recorded core gitlink and checks the
TRX for the exact required test name, one execution, and a Passed outcome. Zero
matches or a skipped case cannot satisfy this gate. A missing library, wrong OS,
or unavailable Metal backend fails rather than skips. The smoke checks real
metadata P/Invokes and owned copies in one process; it publishes no frames and
makes no pixel, cross-process, or Windows/Linux runtime claim. Acquisition's
null result is not treated as proof of a specific native error. The disconnected
snapshot flag is asserted separately after sender disposal and acquisition.
