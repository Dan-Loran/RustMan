using RustMan.Core.Modules.ConsoleStream.Models;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleInterpreterConsumer
{
    Task ReceiveConsoleEntryAsync(ConsoleEntry entry, CancellationToken cancellationToken = default);

    Task ReceiveErrorAsync(ConsoleStreamError error, CancellationToken cancellationToken = default);
}
