using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Core.Modules.WebRcon.Contracts;

public interface IWebRconConsumer
{
    Task OnConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default);

    Task OnMessageReceivedAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default);

    Task OnErrorOccurredAsync(WebRconError error, CancellationToken cancellationToken = default);
}
