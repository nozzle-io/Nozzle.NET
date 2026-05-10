using System;
using System.Runtime.InteropServices;
using System.Threading;
using Nozzle;

const string SenderName = "nozzle_sample_sender";
const string AppName = "Nozzle.Sender";
const uint Width = 640;
const uint Height = 480;
const float Fps = 30.0f;
var _running = true;

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    _running = false;
};

var sender = Sender.Create(SenderName, AppName);
Console.WriteLine($"Sender created: {SenderName} ({Width}x{Height} {TextureFormat.Rgba32Float})");

var frameIndex = 0u;
var interval = TimeSpan.FromSeconds(1.0 / Fps);
var nextTime = DateTime.UtcNow;

try
{
    while (_running)
    {
        var now = DateTime.UtcNow;
        if (now < nextTime)
        {
            Thread.Sleep((int)(nextTime - now).TotalMilliseconds);
            continue;
        }
        nextTime = now + interval;

        using var frame = sender.AcquireWritableFrame(Width, Height, TextureFormat.Rgba32Float);
        var pixels = frame.LockWritablePixels();
        FillGradient(pixels, frameIndex);
        frame.UnlockWritablePixels();
        sender.CommitFrame(frame);

        frameIndex++;
        if (frameIndex % (uint)Fps == 0)
        {
            Console.WriteLine($"Sent {frameIndex} frames");
        }
    }
}
finally
{
    sender.Dispose();
    Console.WriteLine("Sender shut down.");
}

unsafe void FillGradient(MappedPixels pixels, uint frameIndex)
{
    float t = frameIndex / (Fps * 10.0f);
    int stride = (int)pixels.RowStrideBytes;
    byte* ptr = (byte*)pixels.Data;

    for (uint y = 0; y < pixels.Height; y++)
    {
        for (uint x = 0; x < pixels.Width; x++)
        {
            float r = (float)Math.Sin(x / (double)pixels.Width * Math.PI * 2.0 + t) * 0.5f + 0.5f;
            float g = (float)Math.Sin(y / (double)pixels.Height * Math.PI * 2.0 + t * 0.7f) * 0.5f + 0.5f;
            float b = (float)Math.Sin((x + y) / (double)(pixels.Width + pixels.Height) * Math.PI * 2.0 + t * 1.3f) * 0.5f + 0.5f;
            float a = 1.0f;

            int offset = (int)(y * stride + x * 16);
            WriteFloat(ptr, offset, r);
            WriteFloat(ptr, offset + 4, g);
            WriteFloat(ptr, offset + 8, b);
            WriteFloat(ptr, offset + 12, a);
        }
    }
}

static void WriteFloat(byte* buffer, int offset, float value)
{
    var bytes = BitConverter.GetBytes(value);
    buffer[offset] = bytes[0];
    buffer[offset + 1] = bytes[1];
    buffer[offset + 2] = bytes[2];
    buffer[offset + 3] = bytes[3];
}
