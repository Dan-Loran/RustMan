using RustMan.Core.Modules.Routing.Models;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleInterpreterModule
{
    void SetConsumer(IConsoleInterpreterConsumer consumer);

    Task ReceiveConsoleCandidateAsync(RoutedConsoleCandidate candidate, CancellationToken cancellationToken = default);
}
