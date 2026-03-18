namespace RustMan.Core.Modules.ConsoleStream;

public sealed record ConsoleStreamStateChanged
{
    public required ConsoleStreamSnapshot Snapshot { get; init; }
}
