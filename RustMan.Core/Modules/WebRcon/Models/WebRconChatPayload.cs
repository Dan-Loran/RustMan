namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconChatPayload : IWebRconPayload
{
    public int Channel { get; init; }

    public required string Message { get; init; }

    public required string UserId { get; init; }

    public required string Username { get; init; }

    public string? Color { get; init; }

    public long Time { get; init; }
}
