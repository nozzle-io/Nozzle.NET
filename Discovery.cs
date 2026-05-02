using System.Runtime.InteropServices;

namespace Nozzle;

public static class Discovery
{
    public static SenderInfo[] EnumerateSenders()
    {
        unsafe
        {
            var array = new NativeMethods.SenderInfoArray();
            ErrorHelper.ThrowIfFailed(NativeMethods.nozzle_enumerate_senders(&array));

            try
            {
                var results = new SenderInfo[array.Count];
                for (uint i = 0; i < array.Count; i++)
                {
                    results[i] = SenderInfo.FromNative(array.Items[i]);
                }
                return results;
            }
            finally
            {
                NativeMethods.nozzle_free_sender_info_array(&array);
            }
        }
    }
}
