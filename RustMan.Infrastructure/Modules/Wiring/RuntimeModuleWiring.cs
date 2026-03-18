using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.Wiring;

public sealed class RuntimeModuleWiring : IWebRconConsumer
{
    private const string CommandSourceName = "RustMan.Router";

    private readonly IRouterModule _routerModule;
    private readonly IWebRconModule _webRconModule;

    public RuntimeModuleWiring(IRouterModule routerModule, IWebRconModule webRconModule)
    {
        _routerModule = routerModule;
        _webRconModule = webRconModule;

        _routerModule.SetCommandDispatchOutput((dispatchRequest, cancellationToken) =>
        {
            var commandMessage = BuildCommandMessage(dispatchRequest.CommandText, dispatchRequest.Parameters);

            return _webRconModule.SendCommandAsync(new WebRconCommandRequest
            {
                Identifier = dispatchRequest.CommandIdentifier,
                Message = commandMessage,
                Name = CommandSourceName
            }, cancellationToken);
        });

        _webRconModule.SetConsumer(this);
    }

    public Task OnConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default)
    {
        return _routerModule.NotifyConnectionStateChangedAsync(new RouterConnectionStateChanged
        {
            ConnectionState = state
        }, cancellationToken);
    }

    public Task OnMessageReceivedAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default)
    {
        return _routerModule.ReceiveInboundMessageAsync(new RouterInboundMessageReceived
        {
            Message = message
        }, cancellationToken);
    }

    public Task OnErrorOccurredAsync(WebRconError error, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private static string BuildCommandMessage(string commandText, IReadOnlyList<string> parameters)
    {
        if (parameters.Count == 0)
        {
            return commandText;
        }

        return commandText + " " + string.Join(" ", parameters);
    }
}
