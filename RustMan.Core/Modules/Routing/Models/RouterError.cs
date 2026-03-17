namespace RustMan.Core.Modules.Routing.Models;

public sealed record RouterError
{
    public required string Message { get; init; }
}
