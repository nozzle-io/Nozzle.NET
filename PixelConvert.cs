using System;

namespace Nozzle;

public static class PixelConvert
{
    public static void SwizzleChannels(
        nint src, nint dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        TextureFormat format, nint permuteMap)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_swizzle_channels(
                (void*)src, (void*)dst, width, height, srcRowBytes, dstRowBytes,
                (NativeMethods.TextureFormat)format, (byte*)permuteMap));
        }
    }

    public static void WidenUInt16ToUInt32(
        nint src, nint dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_widen_uint16_to_uint32(
                (void*)src, (void*)dst, width, height, srcRowBytes, dstRowBytes, channels));
        }
    }

    public static void ConvertUInt32ToFloat32(
        nint src, nint dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_convert_uint32_to_float32(
                (void*)src, (void*)dst, width, height, srcRowBytes, dstRowBytes, channels));
        }
    }

    public static void WidenHalfToFloat(
        nint src, nint dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        unsafe
        {
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_widen_half_to_float(
                (void*)src, (void*)dst, width, height, srcRowBytes, dstRowBytes, channels));
        }
    }
}
