using System.Collections.ObjectModel;
using RustMan.Core.Modules.ConsoleStream;

namespace RustMan.Presentation.Modules.ConsoleStream;

public sealed class ConsoleStreamPresentationService : IConsoleStreamConsumer
{
    public ConsoleStreamPresentationService(ConsoleStreamState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        State = state;
    }

    public ConsoleStreamState State { get; }

    public ConsoleStreamViewModel ViewModel => State.ViewModel;

    public Task OnConsoleStreamStateChangedAsync(ConsoleStreamStateChanged state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var lines = state.Snapshot.Entries
            .Select(entry => new ConsoleLineViewModel
            {
                Text = entry.Text,
                Type = entry.Type,
                TimestampUtc = entry.TimestampUtc
            })
            .ToList();

        State.SetViewModel(new ConsoleStreamViewModel
        {
            Lines = new ReadOnlyCollection<ConsoleLineViewModel>(lines),
            IsEmpty = lines.Count == 0,
            CanClear = lines.Count > 0
        });

        return Task.CompletedTask;
    }
}
