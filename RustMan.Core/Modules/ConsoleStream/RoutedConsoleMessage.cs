namespace RustMan.Core.Modules.ConsoleStream;

public sealed record RoutedConsoleMessage
{
    public required string Message { get; init; }

    public required string Type { get; init; }

    public DateTime TimestampUtc { get; init; }
}
