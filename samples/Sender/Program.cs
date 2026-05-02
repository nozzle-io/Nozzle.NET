using System;
using System.Runtime.InteropServices;
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
const int BytesPerPixel = sizeof(float) * 4;
var pixelBuffer = new byte[Width * Height * BytesPerPixel];

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

        FillGradient(pixelBuffer, Width, Height, frameIndex);

        using var frame = sender.AcquireWritableFrame(Width, Height, TextureFormat.Rgba32Float);
        var pixels = frame.LockWritablePixels();
        Marshal.Copy(pixelBuffer, 0, pixels.Data, pixelBuffer.Length);
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

void FillGradient(byte[] buffer, uint width, uint height, uint frameIndex)
{
    float t = frameIndex / (Fps * 10.0f);
    int stride = (int)width * 16;

    for (uint y = 0; y < height; y++)
    {
        for (uint x = 0; x < width; x++)
        {
            float r = (float)Math.Sin(x / width * Math.PI * 2.0 + t) * 0.5f + 0.5f;
            float g = (float)Math.Sin(y / height * Math.PI * 2.0 + t * 0.7f) * 0.5f + 0.5f;
            float b = (float)Math.Sin((x + y) / (width + height) * Math.PI * 2.0 + t * 1.3f) * 0.5f + 0.5f;
            float a = 1.0f;

            int offset = (int)(y * stride + x * 16);
            WriteFloat(buffer, offset, r);
            WriteFloat(buffer, offset + 4, g);
            WriteFloat(buffer, offset + 8, b);
            WriteFloat(buffer, offset + 12, a);
        }
    }
}

void WriteFloat(byte[] buffer, int offset, float value)
{
    var bytes = BitConverter.GetBytes(value);
    buffer[offset] = bytes[0];
    buffer[offset + 1] = bytes[1];
    buffer[offset + 2] = bytes[2];
    buffer[offset + 3] = bytes[3];
}
