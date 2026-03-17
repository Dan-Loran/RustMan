using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Core.Modules.WebRcon.Contracts;

public interface IWebRconModule
{
    void SetConsumer(IWebRconConsumer consumer);

    Task ConnectAsync(WebRconConnectionRequest request, CancellationToken cancellationToken = default);

    Task SendCommandAsync(WebRconCommandRequest command, CancellationToken cancellationToken = default);
}
