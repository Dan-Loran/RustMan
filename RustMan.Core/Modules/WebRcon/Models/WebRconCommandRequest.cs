namespace RustMan.Core.Modules.WebRcon.Models;

public sealed record WebRconCommandRequest
{
    public int Identifier { get; init; }

    public required string CommandText { get; init; }

    public IReadOnlyList<string> Parameters { get; init; } = Array.Empty<string>();

    public required string Name { get; init; }
}
