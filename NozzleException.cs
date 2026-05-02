using System;

namespace Nozzle;

public sealed class NozzleException : Exception
{
    public ErrorCode ErrorCode { get; }

    public NozzleException(ErrorCode errorCode, string? message = null)
        : base(message ?? errorCode.ToString())
    {
        ErrorCode = errorCode;
    }
}

internal static class ErrorHelper
{
    public static void ThrowIfFailed(NativeMethods.ErrorCode code)
    {
        if (code != NativeMethods.ErrorCode.Ok)
        {
            throw new NozzleException((ErrorCode)code);
        }
    }
}
