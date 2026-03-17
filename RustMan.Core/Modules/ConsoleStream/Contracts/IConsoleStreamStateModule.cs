using RustMan.Core.Modules.ConsoleStream.Models;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleStreamStateModule
{
    void SetConsumer(IConsoleStreamStateConsumer consumer);

    Task AppendEntryAsync(ConsoleEntry entry, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);

    ConsoleStreamSnapshot GetSnapshot();
}
