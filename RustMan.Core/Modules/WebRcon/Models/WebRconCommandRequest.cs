namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconCommandRequest
{
    public int Identifier { get; init; }

    public required string Message { get; init; }

    public required string Name { get; init; }
}
