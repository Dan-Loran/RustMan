namespace RustMan.Core.Modules.ConsoleStream;

public interface IConsoleStreamConsumer
{
    Task OnConsoleStreamStateChangedAsync(ConsoleStreamStateChanged state, CancellationToken cancellationToken = default);
}
