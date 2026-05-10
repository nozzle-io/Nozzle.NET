using System;

namespace Nozzle;

public static class PixelConvert
{
    private static int BytesPerPixel(TextureFormat format) => format switch
    {
        TextureFormat.R8Unorm => 1,
        TextureFormat.Rg8Unorm => 2,
        TextureFormat.Rgb8Unorm => 3,
        TextureFormat.Rgba8Unorm => 4,
        TextureFormat.Bgra8Unorm => 4,
        TextureFormat.Rgba8Srgb => 4,
        TextureFormat.Bgra8Srgb => 4,
        TextureFormat.R16Unorm => 2,
        TextureFormat.Rg16Unorm => 4,
        TextureFormat.Rgb16Unorm => 6,
        TextureFormat.Rgba16Unorm => 8,
        TextureFormat.R16Float => 2,
        TextureFormat.Rg16Float => 4,
        TextureFormat.Rgb16Float => 6,
        TextureFormat.Rgba16Float => 8,
        TextureFormat.R32Float => 4,
        TextureFormat.Rg32Float => 8,
        TextureFormat.Rgb32Float => 12,
        TextureFormat.Rgba32Float => 16,
        TextureFormat.R32Uint => 4,
        TextureFormat.Rgba32Uint => 16,
        TextureFormat.Rgb32Uint => 12,
        TextureFormat.Depth32Float => 4,
        _ => -1,
    };

    private static void ValidateBufferSizes(
        ReadOnlySpan<byte> src, Span<byte> dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint minSrcRowBytes, uint minDstRowBytes)
    {
        if (height == 0)
            return;

        if (srcRowBytes < minSrcRowBytes)
            throw new NozzleException(ErrorCode.ErrorInvalidArgument,
                $"srcRowBytes ({srcRowBytes}) is less than minimum ({minSrcRowBytes}) for width {width}");
        if (dstRowBytes < minDstRowBytes)
            throw new NozzleException(ErrorCode.ErrorInvalidArgument,
                $"dstRowBytes ({dstRowBytes}) is less than minimum ({minDstRowBytes}) for width {width}");

        long srcRequired;
        long dstRequired;
        try
        {
            srcRequired = checked((long)(height - 1) * srcRowBytes + minSrcRowBytes);
            dstRequired = checked((long)(height - 1) * dstRowBytes + minDstRowBytes);
        }
        catch (OverflowException)
        {
            throw new NozzleException(ErrorCode.ErrorInvalidArgument, "Buffer size calculation overflow");
        }

        if (src.Length < srcRequired)
            throw new NozzleException(ErrorCode.ErrorInvalidArgument,
                $"Source span length ({src.Length}) is less than required ({srcRequired})");
        if (dst.Length < dstRequired)
            throw new NozzleException(ErrorCode.ErrorInvalidArgument,
                $"Destination span length ({dst.Length}) is less than required ({dstRequired})");
    }

    public static void SwizzleChannels(
        ReadOnlySpan<byte> src, Span<byte> dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        TextureFormat format, (byte R, byte G, byte B, byte A) permuteMap)
    {
        var bpp = BytesPerPixel(format);
        if (bpp < 0)
            throw new NozzleException(ErrorCode.ErrorInvalidArgument, $"Unknown format: {format}");

        uint minRowBytes;
        try
        {
            minRowBytes = checked(width * (uint)bpp);
        }
        catch (OverflowException)
        {
            throw new NozzleException(ErrorCode.ErrorInvalidArgument, "Row size calculation overflow");
        }

        ValidateBufferSizes(src, dst, width, height, srcRowBytes, dstRowBytes, minRowBytes, minRowBytes);

        unsafe
        {
            byte* map = stackalloc byte[4];
            map[0] = permuteMap.R;
            map[1] = permuteMap.G;
            map[2] = permuteMap.B;
            map[3] = permuteMap.A;
            fixed (byte* pSrc = src)
            fixed (byte* pDst = dst)
            {
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_swizzle_channels(
                    pSrc, pDst, width, height, srcRowBytes, dstRowBytes,
                    (NativeMethods.TextureFormat)format, map));
            }
        }
    }

    public static void WidenUInt16ToUInt32(
        ReadOnlySpan<byte> src, Span<byte> dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        ValidateChannelCount(channels);

        uint minSrcRow;
        uint minDstRow;
        try
        {
            minSrcRow = checked(width * channels * 2u);
            minDstRow = checked(width * channels * 4u);
        }
        catch (OverflowException)
        {
            throw new NozzleException(ErrorCode.ErrorInvalidArgument, "Row size calculation overflow");
        }

        ValidateBufferSizes(src, dst, width, height, srcRowBytes, dstRowBytes, minSrcRow, minDstRow);

        unsafe
        {
            fixed (byte* pSrc = src)
            fixed (byte* pDst = dst)
            {
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_widen_uint16_to_uint32(
                    pSrc, pDst, width, height, srcRowBytes, dstRowBytes, channels));
            }
        }
    }

    public static void ConvertUInt32ToFloat32(
        ReadOnlySpan<byte> src, Span<byte> dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        ValidateChannelCount(channels);

        uint minRow;
        try
        {
            minRow = checked(width * channels * 4u);
        }
        catch (OverflowException)
        {
            throw new NozzleException(ErrorCode.ErrorInvalidArgument, "Row size calculation overflow");
        }

        ValidateBufferSizes(src, dst, width, height, srcRowBytes, dstRowBytes, minRow, minRow);

        unsafe
        {
            fixed (byte* pSrc = src)
            fixed (byte* pDst = dst)
            {
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_convert_uint32_to_float32(
                    pSrc, pDst, width, height, srcRowBytes, dstRowBytes, channels));
            }
        }
    }

    public static void WidenHalfToFloat(
        ReadOnlySpan<byte> src, Span<byte> dst,
        uint width, uint height,
        uint srcRowBytes, uint dstRowBytes,
        uint channels)
    {
        ValidateChannelCount(channels);

        uint minSrcRow;
        uint minDstRow;
        try
        {
            minSrcRow = checked(width * channels * 2u);
            minDstRow = checked(width * channels * 4u);
        }
        catch (OverflowException)
        {
            throw new NozzleException(ErrorCode.ErrorInvalidArgument, "Row size calculation overflow");
        }

        ValidateBufferSizes(src, dst, width, height, srcRowBytes, dstRowBytes, minSrcRow, minDstRow);

        unsafe
        {
            fixed (byte* pSrc = src)
            fixed (byte* pDst = dst)
            {
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_widen_half_to_float(
                    pSrc, pDst, width, height, srcRowBytes, dstRowBytes, channels));
            }
        }
    }

    private static void ValidateChannelCount(uint channels)
    {
        if (channels is < 1 or > 4)
            throw new NozzleException(ErrorCode.ErrorInvalidArgument,
                $"Channel count must be 1-4, got {channels}");
    }
}
