namespace RustMan.Core.Modules.ConsoleStream.Models;

public sealed record ConsoleStreamError
{
    public required string Message { get; init; }
}
