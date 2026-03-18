namespace RustMan.Core.Modules.ConsoleStream;

public sealed record ConsoleEntry
{
    public required string Text { get; init; }

    public required string Type { get; init; }

    public DateTime TimestampUtc { get; init; }
}
