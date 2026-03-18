namespace RustMan.Presentation.Modules.ConsoleStream;

public sealed record ConsoleLineViewModel
{
    public required string Text { get; init; }

    public required string Type { get; init; }

    public DateTime TimestampUtc { get; init; }
}
