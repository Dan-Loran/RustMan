using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Core.Modules.Routing.Contracts;

public interface IMessageRouterModule
{
    void SetConsumer(IMessageRouterConsumer consumer);

    Task ReceiveInboundMessageAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default);

    Task NotifyCommandSentAsync(WebRconCommandRequest request, CancellationToken cancellationToken = default);

    Task NotifyConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default);
}
