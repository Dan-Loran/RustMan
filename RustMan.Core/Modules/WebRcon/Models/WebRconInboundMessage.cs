namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconInboundMessage
{
    public int Identifier { get; init; }

    public required string Type { get; init; }

    public string? Stacktrace { get; init; }

    public required IWebRconPayload Payload { get; init; }
}
