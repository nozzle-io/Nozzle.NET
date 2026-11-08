using System;
using System.Runtime.InteropServices;

namespace Nozzle;

public sealed class Receiver : IDisposable
{
    private unsafe NativeMethods.NozzleReceiver* _handle;

    internal unsafe Receiver(NativeMethods.NozzleReceiver* handle)
    {
        _handle = handle;
    }

    public static Receiver Create(string name, string applicationName, ReceiveMode receiveMode = ReceiveMode.LatestOnly)
    {
        unsafe
        {
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(name + '\0');
            var appBytes = System.Text.Encoding.UTF8.GetBytes(applicationName + '\0');

            fixed (byte* pName = nameBytes)
            fixed (byte* pApp = appBytes)
            {
                var desc = new NativeMethods.ReceiverDesc
                {
                    Name = pName,
                    ApplicationName = pApp,
                    ReceiveMode = (NativeMethods.ReceiveMode)receiveMode,
                };

                NativeMethods.NozzleReceiver* receiver;
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_receiver_create(&desc, &receiver));
                return new Receiver(receiver);
            }
        }
    }

    public Frame? AcquireFrame(ulong timeoutMs = 0)
    {
        unsafe
        {
            var desc = new NativeMethods.AcquireDesc { TimeoutMs = timeoutMs };
            NativeMethods.NozzleFrame* frame;
            var result = NativeMethods.nozzle_receiver_acquire_frame(_handle, &desc, &frame);

            if (result == NativeMethods.ErrorCode.ErrorTimeout ||
                result == NativeMethods.ErrorCode.ErrorSenderClosed ||
                result == NativeMethods.ErrorCode.ErrorSenderNotFound)
            {
                return null;
            }

            ErrorHelper.ThrowIfFailed(result);
            return new Frame(frame);
        }
    }

    public ConnectedSenderInfo GetConnectedInfo()
    {
        return GetConnectionSnapshot().Info;
    }

    /// <summary>Copies last-observed state and metadata together; disconnected identity is retained.</summary>
    public ConnectionSnapshot GetConnectionSnapshot()
    {
        unsafe
        {
            try
            {
                var info = new NativeMethods.ConnectedSenderSnapshot();
                ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_receiver_get_connected_info_snapshot(_handle, &info));
                return ConnectionSnapshot.FromNative(info);
            }
            finally
            {
                GC.KeepAlive(this);
            }
        }
    }

    private unsafe void Dispose(bool disposing)
    {
        if (_handle != null)
        {
            NativeMethods.nozzle_receiver_destroy(_handle);
            _handle = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Receiver()
    {
        Dispose(false);
    }
}
