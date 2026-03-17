namespace RustMan.Core.Modules.Routing.Models;

public sealed record RoutedConsoleCandidate
{
    public required string Content { get; init; }

    public DateTimeOffset ReceivedAtUtc { get; init; }
}
