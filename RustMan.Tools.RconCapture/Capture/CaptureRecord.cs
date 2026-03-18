namespace RustMan.Tools.RconCapture.Capture;

internal sealed record CaptureRecord(
    DateTime CapturedAtUtc,
    string Direction,
    string Raw);
