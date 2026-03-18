namespace RustMan.Core.Modules.Routing;

public interface IRouterModule
{
    void SetCommandResponseOutput(Func<RoutedCommandResponse, CancellationToken, Task> output);

    void SetUnhandledMessageOutput(Func<RoutedUnhandledMessage, CancellationToken, Task> output);

    void SetErrorOutput(Func<RouterErrorOccurred, CancellationToken, Task> output);

    Task RequestCommandAsync(RouterCommandRequested input, CancellationToken cancellationToken = default);
}
