namespace RustMan.Core.Modules.Routing.Models;

public sealed record RoutedCommandResponse
{
    public required string Content { get; init; }
}
