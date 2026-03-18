using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.WebRcon.Enums;

namespace RustMan.Infrastructure.Modules.Routing;

public sealed class RouterModule : IRouterModule
{
    private static readonly TimeSpan CommandTtl = TimeSpan.FromSeconds(5);

    private readonly Dictionary<int, DateTime> _pendingCommands = new();
    private int _nextCommandIdentifier = 1;
    private Action<RouterCommandDispatchRequested>? _commandDispatchHandler;
    private Action<RoutedCommandResponse>? _commandResponseHandler;
    private Action<RouterErrorOccurred>? _errorHandler;

    public void SetCommandDispatchOutput(Func<RouterCommandDispatchRequested, CancellationToken, Task> output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _commandDispatchHandler = dispatchRequest =>
        {
            output(dispatchRequest, CancellationToken.None).GetAwaiter().GetResult();
        };
    }

    public void SetCommandResponseOutput(Func<RoutedCommandResponse, CancellationToken, Task> output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _commandResponseHandler = routedCommandResponse =>
        {
            output(routedCommandResponse, CancellationToken.None).GetAwaiter().GetResult();
        };
    }

    public void SetErrorOutput(Func<RouterErrorOccurred, CancellationToken, Task> output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _errorHandler = routerError =>
        {
            output(routerError, CancellationToken.None).GetAwaiter().GetResult();
        };
    }

    public Task RequestCommandAsync(RouterCommandRequested input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        CleanupExpired();

        var commandIdentifier = _nextCommandIdentifier;
        _nextCommandIdentifier++;

        _pendingCommands[commandIdentifier] = DateTime.UtcNow;

        _commandDispatchHandler?.Invoke(new RouterCommandDispatchRequested
        {
            CommandIdentifier = commandIdentifier,
            CommandText = input.CommandText,
            Parameters = input.Parameters
        });

        return Task.CompletedTask;
    }

    public Task ReceiveInboundMessageAsync(RouterInboundMessageReceived input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        try
        {
            CleanupExpired();

            var inboundMessage = input.Message;
            var commandIdentifier = inboundMessage.Identifier;

            if (_pendingCommands.Remove(commandIdentifier))
            {
                _commandResponseHandler?.Invoke(new RoutedCommandResponse
                {
                    CommandIdentifier = commandIdentifier,
                    Message = inboundMessage
                });

                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            _errorHandler?.Invoke(new RouterErrorOccurred
            {
                Message = "Router failed to process inbound message.",
                RelatedMessage = input.Message,
                CommandIdentifier = input.Message.Identifier
            });
        }

        return Task.CompletedTask;
    }

    public Task NotifyConnectionStateChangedAsync(RouterConnectionStateChanged input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.ConnectionState == WebRconConnectionState.Disconnected ||
            input.ConnectionState == WebRconConnectionState.Faulted)
        {
            _pendingCommands.Clear();
        }

        return Task.CompletedTask;
    }

    private void CleanupExpired()
    {
        var expiredCommandIdentifiers = new List<int>();
        var utcNow = DateTime.UtcNow;

        foreach (var pendingCommand in _pendingCommands)
        {
            var age = utcNow - pendingCommand.Value;
            if (age > CommandTtl)
            {
                expiredCommandIdentifiers.Add(pendingCommand.Key);
            }
        }

        foreach (var commandIdentifier in expiredCommandIdentifiers)
        {
            _pendingCommands.Remove(commandIdentifier);
        }
    }
}
