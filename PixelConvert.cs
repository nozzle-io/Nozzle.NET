namespace Nozzle;

public static class PixelConvert
{
    public static void SwizzleChannels(
        nint src, nint dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        TextureFormat format, (byte R, byte G, byte B, byte A) permuteMap)
    {
        unsafe
        {
            byte* map = stackalloc byte[4];
            map[0] = permuteMap.R;
            map[1] = permuteMap.G;
            map[2] = permuteMap.B;
            map[3] = permuteMap.A;
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_swizzle_channels(
                (void*)src, (void*)dst, width, height, srcRowBytes, dstRowBytes,
                (NativeMethods.TextureFormat)format, map));
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
