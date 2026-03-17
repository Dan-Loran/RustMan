using RustMan.Core.Modules.ConsoleStream.Enums;

namespace RustMan.Core.Modules.ConsoleStream.Models;

public sealed record ConsoleEntry
{
    public required string Text { get; init; }

    public ConsoleEntryKind Kind { get; init; }

    public DateTimeOffset TimestampUtc { get; init; }
}
