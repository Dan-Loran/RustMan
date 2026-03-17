using RustMan.Core.Modules.ConsoleStream.Contracts;
using RustMan.Core.Modules.ConsoleStream.Models;

namespace RustMan.Infrastructure.Modules.ConsoleStream.Runtime;

public sealed class ConsoleStreamStateModule : IConsoleStreamStateModule
{
    private IConsoleStreamStateConsumer? _consumer;
    private ConsoleStreamSnapshot _snapshot = new();

    public void SetConsumer(IConsoleStreamStateConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task AppendEntryAsync(ConsoleEntry entry, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _snapshot = new ConsoleStreamSnapshot();
        return Task.CompletedTask;
    }

    public ConsoleStreamSnapshot GetSnapshot()
    {
        return _snapshot;
    }
}
