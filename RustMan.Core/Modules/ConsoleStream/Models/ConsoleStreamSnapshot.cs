namespace RustMan.Core.Modules.ConsoleStream.Models;

public sealed record ConsoleStreamSnapshot
{
    public IReadOnlyList<ConsoleEntry> Entries { get; init; } = Array.Empty<ConsoleEntry>();
}
