using RustMan.Core.Modules.Routing.Models;

namespace RustMan.Core.Modules.Routing.Contracts;

public interface IMessageRouterConsumer
{
    Task ReceiveConsoleCandidateAsync(RoutedConsoleCandidate candidate, CancellationToken cancellationToken = default);

    Task ReceiveCommandResponseAsync(RoutedCommandResponse response, CancellationToken cancellationToken = default);

    Task ReceiveUnhandledMessageAsync(RoutedUnhandledMessage message, CancellationToken cancellationToken = default);

    Task ReceiveErrorAsync(RouterError error, CancellationToken cancellationToken = default);
}
