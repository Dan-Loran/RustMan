using RustMan.Core.Modules.ConsoleStream.Models;
using ConsoleStreamSnapshotModel = RustMan.Core.Modules.ConsoleStream.Models.ConsoleStreamSnapshot;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleStreamStateConsumer
{
    Task ReceiveSnapshotChangedAsync(ConsoleStreamSnapshotModel snapshot, CancellationToken cancellationToken = default);

    Task ReceiveClearedAsync(CancellationToken cancellationToken = default);

    Task ReceiveErrorAsync(ConsoleStreamError error, CancellationToken cancellationToken = default);
}
