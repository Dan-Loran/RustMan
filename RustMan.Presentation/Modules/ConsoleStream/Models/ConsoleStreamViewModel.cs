namespace RustMan.Presentation.Modules.ConsoleStream.Models;

public sealed record ConsoleStreamViewModel
{
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}
