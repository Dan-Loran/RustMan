using RustMan.Core.Modules.ConsoleStream.Models;
using ConsoleEntryModel = RustMan.Core.Modules.ConsoleStream.Models.ConsoleEntry;
using ConsoleStreamSnapshotModel = RustMan.Core.Modules.ConsoleStream.Models.ConsoleStreamSnapshot;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleStreamStateModule
{
    void SetConsumer(IConsoleStreamStateConsumer consumer);

    Task AppendEntryAsync(ConsoleEntryModel entry, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);

    ConsoleStreamSnapshotModel GetSnapshot();
}
