namespace RustMan.Core.Modules.Routing;

public sealed record RouterCommandRequested
{
    public string CommandText { get; init; } = string.Empty;

    // String parameters are intentionally simple for now and may be revisited if richer typing is needed later.
    public IReadOnlyList<string> Parameters { get; init; } = Array.Empty<string>();
}
