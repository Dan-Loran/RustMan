namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconTextPayload : IWebRconPayload
{
    public required string Text { get; init; }
}
