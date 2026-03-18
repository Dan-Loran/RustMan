using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Core.Modules.Routing;

public sealed record RouterInboundMessageReceived
{
    public required WebRconInboundMessage Message { get; init; }
}
