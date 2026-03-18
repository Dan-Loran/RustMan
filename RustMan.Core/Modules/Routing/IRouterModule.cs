namespace RustMan.Core.Modules.Routing;

public interface IRouterModule
{
    void SetCommandDispatchOutput(Func<RouterCommandDispatchRequested, CancellationToken, Task> output);

    void SetCommandResponseOutput(Func<RoutedCommandResponse, CancellationToken, Task> output);

    void SetErrorOutput(Func<RouterErrorOccurred, CancellationToken, Task> output);

    Task RequestCommandAsync(RouterCommandRequested input, CancellationToken cancellationToken = default);

    Task ReceiveInboundMessageAsync(RouterInboundMessageReceived input, CancellationToken cancellationToken = default);

    Task NotifyConnectionStateChangedAsync(RouterConnectionStateChanged input, CancellationToken cancellationToken = default);
}
