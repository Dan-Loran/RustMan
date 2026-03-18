namespace RustMan.Core.Modules.ConsoleStream;

public sealed record ConsoleStreamSnapshot
{
    public IReadOnlyList<ConsoleEntry> Entries { get; init; } = Array.Empty<ConsoleEntry>();
}
