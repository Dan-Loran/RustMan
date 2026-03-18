namespace RustMan.Core.Modules.ConsoleStream;

public interface IConsoleStreamModule
{
    Task HandleMessageAsync(RoutedConsoleMessage message, CancellationToken cancellationToken = default);

    void SetConsumer(IConsoleStreamConsumer consumer);
}
