using System.Runtime.InteropServices;

namespace Nozzle;

internal static unsafe partial class NativeMethods
{
    private const string LibraryName = "nozzle";

    // ========== Opaque Handles ==========

    [StructLayout(LayoutKind.Sequential)]
    public struct NozzleSender;
    [StructLayout(LayoutKind.Sequential)]
    public struct NozzleReceiver;
    [StructLayout(LayoutKind.Sequential)]
    public struct NozzleFrame;
    [StructLayout(LayoutKind.Sequential)]
    public struct NozzleTexture;
    [StructLayout(LayoutKind.Sequential)]
    public struct NozzleDevice;

    // ========== Enums ==========

    public enum ErrorCode : int
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

    public enum BackendType : int
    {
        Unknown = 0,
        D3D11 = 1,
        Metal = 2,
        OpenGL = 3,
    }

    public enum TextureFormat : int
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

    public enum ReceiveMode : int
    {
        LatestOnly = 0,
        SequentialBestEffort = 1,
    }

    public enum FrameStatus : int
    {
        New = 0,
        NoNew = 1,
        Dropped = 2,
        SenderClosed = 3,
        Error = 4,
    }

    // ========== Descriptor Structs ==========

    [StructLayout(LayoutKind.Sequential)]
    public struct SenderDesc
    {
        public byte* Name;
        public byte* ApplicationName;
        public uint RingBufferSize;
        public int AllowFormatFallback;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ReceiverDesc
    {
        public byte* Name;
        public byte* ApplicationName;
        public ReceiveMode ReceiveMode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AcquireDesc
    {
        public ulong TimeoutMs;
    }

    // ========== Info Structs ==========

    [StructLayout(LayoutKind.Sequential)]
    public struct SenderInfo
    {
        public byte* Name;
        public byte* ApplicationName;
        public byte* Id;
        public BackendType Backend;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ConnectedSenderInfo
    {
        public byte* Name;
        public byte* ApplicationName;
        public byte* Id;
        public BackendType Backend;
        public uint Width;
        public uint Height;
        public TextureFormat Format;
        public TextureFormat SemanticFormat;
        public double EstimatedFps;
        public ulong FrameCounter;
        public ulong LastUpdateTimeNs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FrameInfo
    {
        public ulong FrameIndex;
        public ulong TimestampNs;
        public uint Width;
        public uint Height;
        public TextureFormat Format;
        public TextureFormat SemanticFormat;
        public uint DroppedFrameCount;
    }

    // ========== Discovery ==========

    [StructLayout(LayoutKind.Sequential)]
    public struct SenderInfoArray
    {
        public SenderInfo* Items;
        public uint Count;
    }

    // ========== Pixel Access ==========

    [StructLayout(LayoutKind.Sequential)]
    public struct MappedPixels
    {
        public void* Data;
        public uint RowBytes;
        public uint Width;
        public uint Height;
        public TextureFormat Format;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureWrapDesc
    {
        public void* NativeTexture;
        public uint Width;
        public uint Height;
        public TextureFormat Format;
        public BackendType Backend;
    }

    // ========== Sender API ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_create(SenderDesc* desc, NozzleSender** out_sender);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_sender_destroy(NozzleSender* sender);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_publish_texture(NozzleSender* sender, NozzleTexture* texture);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_acquire_writable_frame(
        NozzleSender* sender, uint width, uint height, TextureFormat format, NozzleFrame** out_frame);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_commit_frame(NozzleSender* sender, NozzleFrame* frame);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_get_info(NozzleSender* sender, SenderInfo* out_info);

    // ========== Receiver API ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_receiver_create(ReceiverDesc* desc, NozzleReceiver** out_receiver);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_receiver_destroy(NozzleReceiver* receiver);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_receiver_acquire_frame(
        NozzleReceiver* receiver, AcquireDesc* desc, NozzleFrame** out_frame);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_receiver_get_connected_info(
        NozzleReceiver* receiver, ConnectedSenderInfo* out_info);

    // ========== Frame API ==========

    [LibraryImport(LibraryName)]
    public static partial void nozzle_frame_release(NozzleFrame* frame);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_frame_get_info(NozzleFrame* frame, FrameInfo* out_info);

    // ========== Pixel Access ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_frame_lock_pixels(NozzleFrame* frame, MappedPixels* out_pixels);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_frame_unlock_pixels(NozzleFrame* frame);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_frame_lock_writable_pixels(NozzleFrame* frame, MappedPixels* out_pixels);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_frame_unlock_writable_pixels(NozzleFrame* frame);

    // ========== Discovery ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_enumerate_senders(SenderInfoArray* out_array);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_free_sender_info_array(SenderInfoArray* array);

    // ========== Device API ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_device_get_default(NozzleDevice** out_device);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_device_destroy(NozzleDevice* device);

    // ========== Native Texture Interop (GPU) ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_publish_native_texture(
        NozzleSender* sender, void* native_texture,
        uint width, uint height, TextureFormat format);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_publish_native_texture_ex(
        NozzleSender* sender, void* native_texture,
        uint width, uint height, TextureFormat storage_format, TextureFormat semantic_format);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_frame_copy_to_native_texture(
        NozzleFrame* frame, void* native_texture,
        uint width, uint height, TextureFormat format);

    // ========== Texture Wrap ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_texture_wrap(
        TextureWrapDesc* desc, NozzleTexture** out_texture);

    [LibraryImport(LibraryName)]
    public static partial void nozzle_texture_destroy(NozzleTexture* texture);

    // ========== GL Interop ==========

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_sender_publish_gl_texture(
        NozzleSender* sender, uint gl_texture_name, uint gl_target,
        uint width, uint height, TextureFormat format);

    [LibraryImport(LibraryName)]
    public static partial ErrorCode nozzle_frame_copy_to_gl_texture(
        NozzleFrame* frame, uint gl_texture_name, uint gl_target,
        uint width, uint height, TextureFormat format);
}
