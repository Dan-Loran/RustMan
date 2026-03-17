namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconConnectionRequest
{
    public required Uri ServerUri { get; init; }

    public required string Password { get; init; }
}
