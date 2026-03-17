namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconError
{
    public required string Message { get; init; }

    public string? Detail { get; init; }
}
