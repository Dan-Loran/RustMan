using RustMan.Core.Modules.ConsoleStream.Contracts;
using RustMan.Core.Modules.Routing.Models;

namespace RustMan.Infrastructure.Modules.ConsoleStream.Interpretation;

public sealed class ConsoleInterpreterModule : IConsoleInterpreterModule
{
    private IConsoleInterpreterConsumer? _consumer;

    public void SetConsumer(IConsoleInterpreterConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task ReceiveConsoleCandidateAsync(RoutedConsoleCandidate candidate, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
