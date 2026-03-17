namespace RustMan.Presentation.Modules.Connection.Models;

public sealed record ConnectionViewModel
{
    public string StatusText { get; init; } = string.Empty;
}
