using RustMan.Core.Modules.ConsoleStream.Models;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleStreamStateConsumer
{
    Task ReceiveSnapshotChangedAsync(ConsoleStreamSnapshot snapshot, CancellationToken cancellationToken = default);

    Task ReceiveClearedAsync(CancellationToken cancellationToken = default);

    Task ReceiveErrorAsync(ConsoleStreamError error, CancellationToken cancellationToken = default);
}
