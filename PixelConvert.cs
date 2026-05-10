using System;

namespace Nozzle;

public static unsafe class PixelConvert
{
    public static void SwizzleChannels(
        void* src, void* dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        TextureFormat format, byte* permuteMap)
    {
        ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_swizzle_channels(
            src, dst, width, height, srcRowBytes, dstRowBytes,
            (NativeMethods.TextureFormat)format, permuteMap));
    }

    public static void WidenUInt16ToUInt32(
        void* src, void* dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_widen_uint16_to_uint32(
            src, dst, width, height, srcRowBytes, dstRowBytes, channels));
    }

    public static void ConvertUInt32ToFloat32(
        void* src, void* dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_convert_uint32_to_float32(
            src, dst, width, height, srcRowBytes, dstRowBytes, channels));
    }

    public static void WidenHalfToFloat(
        void* src, void* dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_widen_half_to_float(
            src, dst, width, height, srcRowBytes, dstRowBytes, channels));
    }
}
