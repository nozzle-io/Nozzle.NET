namespace Nozzle;

public enum ErrorCode
{
    Ok = 0,
    ErrorUnknown = 1,
    ErrorInvalidArgument = 2,
    ErrorUnsupportedBackend = 3,
    ErrorUnsupportedFormat = 4,
    ErrorDeviceMismatch = 5,
    ErrorResourceCreationFailed = 6,
    ErrorSharedHandleFailed = 7,
    ErrorSenderNotFound = 8,
    ErrorSenderClosed = 9,
    ErrorTimeout = 10,
    ErrorBackendError = 11,
}

public enum BackendType
{
    Unknown = 0,
    D3D11 = 1,
    Metal = 2,
    OpenGL = 3,
    DmaBuf = 4,
}

public enum TextureFormat
{
    Unknown = 0,
    R8Unorm = 1,
    Rg8Unorm = 2,
    Rgb8Unorm = 3,
    Rgba8Unorm = 4,
    Bgra8Unorm = 5,
    Rgba8Srgb = 6,
    Bgra8Srgb = 7,
    R16Unorm = 8,
    Rg16Unorm = 9,
    Rgb16Unorm = 10,
    Rgba16Unorm = 11,
    R16Float = 12,
    Rg16Float = 13,
    Rgb16Float = 14,
    Rgba16Float = 15,
    R32Float = 16,
    Rg32Float = 17,
    Rgb32Float = 18,
    Rgba32Float = 19,
    R32Uint = 20,
    Rgba32Uint = 21,
    Rgb32Uint = 22,
    Depth32Float = 23,
}

public enum ReceiveMode
{
    LatestOnly = 0,
    SequentialBestEffort = 1,
}

public enum FrameStatus
{
    New = 0,
    NoNew = 1,
    Dropped = 2,
    SenderClosed = 3,
    Error = 4,
}

public enum TransferMode
{
    Unknown = 0,
    ZeroCopySharedTexture = 1,
    GpuCopy = 2,
    CpuCopy = 3,
}

public enum SyncMode
{
    None = 0,
    AccessGuarded = 1,
    GpuFenceBestEffort = 2,
}

public enum TextureOrigin
{
    TopLeft = 0,
    BottomLeft = 1,
}

public enum FormatSource
{
    Unknown = 0,
    Requested = 1,
    CallerHint = 2,
    NativeObserved = 3,
}

public enum NativeFormatKind
{
    Unknown = 0,
    MtlPixelFormat = 1,
    DxgiFormat = 2,
    DrmFourcc = 3,
    GlInternalFormat = 4,
}
