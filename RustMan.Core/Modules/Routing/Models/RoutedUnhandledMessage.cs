namespace RustMan.Core.Modules.Routing.Models;

public sealed record RoutedUnhandledMessage
{
    public required string Content { get; init; }
}
