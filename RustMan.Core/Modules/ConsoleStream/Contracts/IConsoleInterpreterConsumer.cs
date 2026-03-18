using RustMan.Core.Modules.ConsoleStream.Models;
using ConsoleEntryModel = RustMan.Core.Modules.ConsoleStream.Models.ConsoleEntry;

namespace RustMan.Core.Modules.ConsoleStream.Contracts;

public interface IConsoleInterpreterConsumer
{
    Task ReceiveConsoleEntryAsync(ConsoleEntryModel entry, CancellationToken cancellationToken = default);

    Task ReceiveErrorAsync(ConsoleStreamError error, CancellationToken cancellationToken = default);
}
