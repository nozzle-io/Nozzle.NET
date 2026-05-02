using System;
using Nozzle;

const string SenderName = "nozzle_sample_sender";
const string AppName = "Nozzle.Receiver";
var _running = true;

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    _running = false;
};

var receiver = Receiver.Create(SenderName, AppName);
Console.WriteLine($"Receiver created, waiting for sender: {SenderName}");

var lastFrameIndex = 0ul;

while (_running)
{
    var frame = receiver.AcquireFrame(timeoutMs: 1000);
    if (frame == null)
    {
        continue;
    }

    using (frame)
    {
        var info = frame.GetInfo();
        if (info.FrameIndex != lastFrameIndex)
        {
            var dropped = info.FrameIndex > 0 && lastFrameIndex > 0
                ? info.FrameIndex - lastFrameIndex - 1
                : 0;
            Console.WriteLine(
                $"Frame {info.FrameIndex}: {info.Width}x{info.Height} " +
                $"format={info.Format} dropped={dropped}");
            lastFrameIndex = info.FrameIndex;
        }
    }
}

receiver.Dispose();
Console.WriteLine("Receiver shut down.");
