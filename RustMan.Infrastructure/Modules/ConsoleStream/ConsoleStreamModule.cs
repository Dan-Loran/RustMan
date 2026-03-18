using System.Collections.ObjectModel;
using RustMan.Core.Modules.ConsoleStream;

namespace RustMan.Infrastructure.Modules.ConsoleStream;

public sealed class ConsoleStreamModule : IConsoleStreamModule
{
    private const int MaxEntries = 500;

    private readonly List<ConsoleEntry> _entries = new();
    private IConsoleStreamConsumer? _consumer;

    public void SetConsumer(IConsoleStreamConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        _consumer = consumer;
    }

    public Task HandleMessageAsync(RoutedConsoleMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _entries.Add(new ConsoleEntry
        {
            Text = message.Message,
            Type = message.Type,
            TimestampUtc = message.TimestampUtc
        });

        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveRange(0, _entries.Count - MaxEntries);
        }

        if (_consumer is null)
        {
            return Task.CompletedTask;
        }

        return _consumer.OnConsoleStreamStateChangedAsync(new ConsoleStreamStateChanged
        {
            Snapshot = CreateSnapshot()
        }, cancellationToken);
    }

    private ConsoleStreamSnapshot CreateSnapshot()
    {
        return new ConsoleStreamSnapshot
        {
            Entries = new ReadOnlyCollection<ConsoleEntry>(_entries.ToList())
        };
    }
}
