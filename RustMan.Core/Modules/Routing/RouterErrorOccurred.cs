using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Core.Modules.Routing;

public sealed record RouterErrorOccurred
{
    public required string Message { get; init; }

    public int? CommandIdentifier { get; init; }

    public WebRconInboundMessage? RelatedMessage { get; init; }
}
