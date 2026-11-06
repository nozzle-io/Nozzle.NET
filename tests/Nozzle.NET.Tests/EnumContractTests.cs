using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Nozzle.Tests;

public class EnumContractTests
{
    private static string Normalized(string value) => value.Replace("_", "").ToLowerInvariant();

    [Fact]
    public void PublicAndNativeEnumLayersMatchEmbeddedCManifest()
    {
        var manifest = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "enum_manifest.json")))!;
        Assert.NotEmpty(manifest);
        var families = new (Type Public, Type Native, string Family, string Prefix)[]
        {
            (typeof(ErrorCode), typeof(NativeMethods.ErrorCode), "NozzleErrorCode", "NOZZLE_"),
            (typeof(BackendType), typeof(NativeMethods.BackendType), "NozzleBackendType", "NOZZLE_BACKEND_"),
            (typeof(TextureFormat), typeof(NativeMethods.TextureFormat), "NozzleTextureFormat", "NOZZLE_FORMAT_"),
            (typeof(ReceiveMode), typeof(NativeMethods.ReceiveMode), "NozzleReceiveMode", "NOZZLE_RECEIVE_"),
            (typeof(FrameStatus), typeof(NativeMethods.FrameStatus), "NozzleFrameStatus", "NOZZLE_FRAME_"),
            (typeof(TextureOrigin), typeof(NativeMethods.TextureOrigin), "NozzleTextureOrigin", "NOZZLE_ORIGIN_"),
            (typeof(TransferMode), typeof(NativeMethods.TransferMode), "NozzleTransferMode", "NOZZLE_TRANSFER_"),
            (typeof(SyncMode), typeof(NativeMethods.SyncMode), "NozzleSyncMode", "NOZZLE_SYNC_"),
            (typeof(FormatSource), typeof(NativeMethods.FormatSource), "NozzleFormatSource", "NOZZLE_FORMAT_SOURCE_"),
            (typeof(NativeFormatKind), typeof(NativeMethods.NativeFormatKind), "NozzleNativeFormatKind", "NOZZLE_NATIVE_KIND_"),
        };
        var covered = new HashSet<string>();
        foreach (var family in families)
        {
            Assert.True(covered.Add(family.Family));
            var expected = manifest[family.Family];
            Assert.NotEmpty(expected);
            foreach (var type in new[] { family.Public, family.Native })
            {
                var seen = new HashSet<string>();
                foreach (var name in Enum.GetNames(type))
                {
                    var key = Assert.Single(expected.Keys.Where(k =>
                        Normalized(k) == Normalized(family.Prefix + name)));
                    Assert.True(seen.Add(key), $"{type.FullName}.{name}: duplicate C mapping");
                    var value = Enum.Parse(type, name);
                    Assert.True(Convert.ToInt32(value) == expected[key],
                        $"{type.FullName}.{name}: C {key}={expected[key]}, binding={value:D}");
                    // Production uses numeric enum casts, not a custom decoder.
                    Assert.Equal(value, Enum.ToObject(type, expected[key]));
                }
                Assert.True(seen.SetEquals(expected.Keys), $"{type.FullName}: missing C member");
            }
        }

        // Capability queries and fallback-info categories/swizzles have no .NET enums.
        // FallbackFlags is a macro domain, deliberately outside this enum-only guard.
        var unexposed = new Dictionary<string, string[]>
        {
            ["NozzleBackendSharingMechanism"] = new[] {
                "NOZZLE_SHARING_NONE", "NOZZLE_SHARING_IOSURFACE", "NOZZLE_SHARING_D3D11_NT_HANDLE",
                "NOZZLE_SHARING_DMA_BUF", "NOZZLE_SHARING_OPENGL_TEXTURE" },
            ["NozzleBackendCapabilityFlags"] = new[] {
                "NOZZLE_BACKEND_CAP_NONE", "NOZZLE_BACKEND_CAP_SENDER", "NOZZLE_BACKEND_CAP_RECEIVER",
                "NOZZLE_BACKEND_CAP_WRITABLE_FRAMES", "NOZZLE_BACKEND_CAP_NATIVE_TEXTURE_PUBLISH",
                "NOZZLE_BACKEND_CAP_DIRECT_EXTERNAL_PUBLISH", "NOZZLE_BACKEND_CAP_CPU_READ",
                "NOZZLE_BACKEND_CAP_CPU_WRITE", "NOZZLE_BACKEND_CAP_ZERO_COPY_RECEIVE",
                "NOZZLE_BACKEND_CAP_ZERO_COPY_PUBLISH", "NOZZLE_BACKEND_CAP_REQUIRES_MATCHING_BACKEND",
                "NOZZLE_BACKEND_CAP_SINGLE_SENDER_PER_PROCESS", "NOZZLE_BACKEND_CAP_MAY_REQUIRE_RUNTIME_PROBE" },
            ["NozzleFallbackCategory"] = new[] {
                "NOZZLE_FALLBACK_CATEGORY_NONE", "NOZZLE_FALLBACK_CATEGORY_STORAGE_COMPATIBLE",
                "NOZZLE_FALLBACK_CATEGORY_CHANNEL_EXPANSION", "NOZZLE_FALLBACK_CATEGORY_QUALITY_LOSS" },
            ["NozzleChannelSwizzle"] = new[] {
                "NOZZLE_CHANNEL_SWIZZLE_IDENTITY", "NOZZLE_CHANNEL_SWIZZLE_SWAP_RB" },
        };
        foreach (var entry in unexposed)
        {
            Assert.True(covered.Add(entry.Key));
            Assert.True(new HashSet<string>(entry.Value).SetEquals(manifest[entry.Key].Keys),
                $"{entry.Key}: review changed unexposed members");
        }
        Assert.True(covered.SetEquals(manifest.Keys), "Unclassified C enum family");
    }
}
