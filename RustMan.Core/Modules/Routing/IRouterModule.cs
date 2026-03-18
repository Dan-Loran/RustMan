namespace RustMan.Core.Modules.Routing;

using RustMan.Core.Modules.ConsoleStream;

public interface IRouterModule
{
    void SetConsoleConsumer(IConsoleStreamModule consoleStreamModule);

    void SetCommandResponseOutput(Func<RoutedCommandResponse, CancellationToken, Task> output);

    void SetUnhandledMessageOutput(Func<RoutedUnhandledMessage, CancellationToken, Task> output);

    void SetErrorOutput(Func<RouterErrorOccurred, CancellationToken, Task> output);

    Task RequestCommandAsync(RouterCommandRequested input, CancellationToken cancellationToken = default);
}
