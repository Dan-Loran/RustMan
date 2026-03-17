using RustMan.Core.Modules.Routing.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.Routing.Runtime;

public sealed class MessageRouterModule : IMessageRouterModule
{
    private IMessageRouterConsumer? _consumer;

    public void SetConsumer(IMessageRouterConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task ReceiveInboundMessageAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyCommandSentAsync(WebRconCommandRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
