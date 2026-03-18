using RustMan.Core.Modules.WebRcon.Enums;

namespace RustMan.Presentation.Modules.Connection.Models;

public sealed record ConnectionViewModel
{
    public WebRconConnectionState Status { get; init; } = WebRconConnectionState.Disconnected;

    public string StatusText { get; init; } = "Disconnected";
}
