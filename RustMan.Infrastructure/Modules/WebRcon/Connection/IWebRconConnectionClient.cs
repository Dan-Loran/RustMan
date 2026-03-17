using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.WebRcon.Connection;

public interface IWebRconConnectionClient
{
    Task ConnectAsync(WebRconConnectionRequest request, CancellationToken cancellationToken = default);

    Task SendAsync(string message, CancellationToken cancellationToken = default);

    Task<string?> ReceiveAsync(CancellationToken cancellationToken = default);
}
