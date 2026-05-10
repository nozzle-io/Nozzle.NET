using System;

namespace Nozzle;

internal enum MappedPixelAccess
{
    ReadOnly,
    Writable
}

public sealed class MappedPixelHandle : IDisposable
{
    private enum HandleState
    {
        Active,
        DisposedByUser,
        InvalidatedByFrame
    }

    private Frame? _frame;
    private readonly MappedPixels _pixels;
    private readonly MappedPixelAccess _access;
    private HandleState _state;

    internal MappedPixelHandle(Frame frame, MappedPixels pixels, MappedPixelAccess access)
    {
        _frame = frame;
        _pixels = pixels;
        _access = access;
        _state = HandleState.Active;
    }

    internal void Invalidate()
    {
        _state = HandleState.InvalidatedByFrame;
        _frame = null;
    }

    internal bool HasFrameReferenceForTests => _frame != null;

    public nint Data
    {
        get
        {
            ThrowIfDisposed();
            return _pixels.Data;
        }
    }

    public long RowStrideBytes
    {
        get
        {
            ThrowIfDisposed();
            return _pixels.RowStrideBytes;
        }
    }

    public uint Width
    {
        get
        {
            ThrowIfDisposed();
            return _pixels.Width;
        }
    }

    public uint Height
    {
        get
        {
            ThrowIfDisposed();
            return _pixels.Height;
        }
    }

    public TextureFormat Format
    {
        get
        {
            ThrowIfDisposed();
            return _pixels.Format;
        }
    }

    public TextureOrigin Origin
    {
        get
        {
            ThrowIfDisposed();
            return _pixels.Origin;
        }
    }

    public void Dispose()
    {
        if (_state == HandleState.Active)
        {
            _state = HandleState.DisposedByUser;
            _frame?.OnHandleDisposed(this);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_state != HandleState.Active)
            throw new ObjectDisposedException(nameof(MappedPixelHandle));
    }
}
