using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Core.Modules.Routing;

public sealed record RoutedCommandResponse
{
    public int CommandIdentifier { get; init; }

    public required WebRconInboundMessage Message { get; init; }
}
