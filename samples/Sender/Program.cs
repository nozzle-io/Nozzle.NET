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
        {
            using var map = frame.LockWritablePixels();
            FillGradient(map, frameIndex);
        }
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

unsafe void FillGradient(MappedPixelHandle map, uint frameIndex)
{
    float t = frameIndex / (Fps * 10.0f);
    long stride = map.RowStrideBytes;
    byte* ptr = (byte*)map.Data;

    for (uint y = 0; y < map.Height; y++)
    {
        for (uint x = 0; x < map.Width; x++)
        {
            float r = (float)Math.Sin(x / (double)map.Width * Math.PI * 2.0 + t) * 0.5f + 0.5f;
            float g = (float)Math.Sin(y / (double)map.Height * Math.PI * 2.0 + t * 0.7f) * 0.5f + 0.5f;
            float b = (float)Math.Sin((x + y) / (double)(map.Width + map.Height) * Math.PI * 2.0 + t * 1.3f) * 0.5f + 0.5f;
            float a = 1.0f;

            byte* pixel = ptr + y * stride + x * 16;
            WriteFloat(pixel, 0, r);
            WriteFloat(pixel, 4, g);
            WriteFloat(pixel, 8, b);
            WriteFloat(pixel, 12, a);
        }
    }
}

static unsafe void WriteFloat(byte* pixel, int offset, float value)
{
    var bytes = BitConverter.GetBytes(value);
    pixel[offset] = bytes[0];
    pixel[offset + 1] = bytes[1];
    pixel[offset + 2] = bytes[2];
    pixel[offset + 3] = bytes[3];
}
