using RustMan.Core.Modules.WebRcon.Enums;

namespace RustMan.Core.Modules.Routing;

public sealed record RouterConnectionStateChanged
{
    public WebRconConnectionState ConnectionState { get; init; }
}
