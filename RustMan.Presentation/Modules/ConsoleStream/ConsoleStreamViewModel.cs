using System.Collections.ObjectModel;

namespace RustMan.Presentation.Modules.ConsoleStream;

public sealed record ConsoleStreamViewModel
{
    public static ConsoleStreamViewModel Empty { get; } = new()
    {
        Lines = ReadOnlyCollection<ConsoleLineViewModel>.Empty,
        IsEmpty = true,
        CanClear = false
    };

    public IReadOnlyList<ConsoleLineViewModel> Lines { get; init; } = ReadOnlyCollection<ConsoleLineViewModel>.Empty;

    public bool IsEmpty { get; init; }

    public bool CanClear { get; init; }
}
