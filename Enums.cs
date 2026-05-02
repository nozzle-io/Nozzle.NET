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
}

public enum TextureFormat
{
    Unknown = 0,
    R8Unorm = 1,
    Rg8Unorm = 2,
    Rgba8Unorm = 3,
    Bgra8Unorm = 4,
    Rgba8Srgb = 5,
    Bgra8Srgb = 6,
    R16Unorm = 7,
    Rg16Unorm = 8,
    Rgba16Unorm = 9,
    R16Float = 10,
    Rg16Float = 11,
    Rgba16Float = 12,
    R32Float = 13,
    Rg32Float = 14,
    Rgba32Float = 15,
    R32Uint = 16,
    Rgba32Uint = 17,
    Depth32Float = 18,
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
