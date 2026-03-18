using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.ConsoleStream;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.Routing;

public sealed class RouterModule : IRouterModule, IWebRconConsumer
{
    private static readonly TimeSpan CommandTtl = TimeSpan.FromSeconds(5);
    private const string CommandSourceName = "RustMan.Router";

    private readonly IWebRconModule _webRconModule;
    private readonly Dictionary<int, DateTime> _pendingCommands = new();
    private int _nextCommandIdentifier = 1;
    private IConsoleStreamModule? _consoleStreamModule;
    private Action<RoutedCommandResponse>? _commandResponseHandler;
    private Action<RoutedUnhandledMessage>? _unhandledMessageHandler;
    private Action<RouterErrorOccurred>? _errorHandler;

    public RouterModule(IWebRconModule webRconModule)
    {
        _webRconModule = webRconModule;
        _webRconModule.SetConsumer(this);
    }

    public void SetConsoleConsumer(IConsoleStreamModule consoleStreamModule)
    {
        ArgumentNullException.ThrowIfNull(consoleStreamModule);
        _consoleStreamModule = consoleStreamModule;
    }

    public void SetCommandResponseOutput(Func<RoutedCommandResponse, CancellationToken, Task> output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _commandResponseHandler = routedCommandResponse =>
        {
            output(routedCommandResponse, CancellationToken.None).GetAwaiter().GetResult();
        };
    }

    public void SetUnhandledMessageOutput(Func<RoutedUnhandledMessage, CancellationToken, Task> output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _unhandledMessageHandler = routedMessage =>
        {
            output(routedMessage, CancellationToken.None).GetAwaiter().GetResult();
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

    public async Task RequestCommandAsync(RouterCommandRequested input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        try
        {
            CleanupExpired();

            var commandIdentifier = _nextCommandIdentifier;
            _nextCommandIdentifier++;

            _pendingCommands[commandIdentifier] = DateTime.UtcNow;

            await _webRconModule.SendCommandAsync(new WebRconCommandRequest
            {
                Identifier = commandIdentifier,
                CommandText = input.CommandText,
                Parameters = input.Parameters,
                Name = CommandSourceName
            }, cancellationToken);
        }
        catch (Exception)
        {
            _errorHandler?.Invoke(new RouterErrorOccurred
            {
                Message = "Router failed to dispatch command.",
                CommandIdentifier = _nextCommandIdentifier - 1
            });

            throw;
        }
    }

    public Task OnConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default)
    {
        if (state == WebRconConnectionState.Disconnected ||
            state == WebRconConnectionState.Faulted)
        {
            _pendingCommands.Clear();
        }

        return Task.CompletedTask;
    }

    public Task OnMessageReceivedAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            _consoleStreamModule?.HandleMessageAsync(new RoutedConsoleMessage
            {
                Message = GetConsoleMessageText(message.Payload),
                Type = message.Type,
                TimestampUtc = DateTime.UtcNow
            }, cancellationToken).GetAwaiter().GetResult();

            CleanupExpired();

            if (_pendingCommands.Remove(message.Identifier))
            {
                _commandResponseHandler?.Invoke(new RoutedCommandResponse
                {
                    CommandIdentifier = message.Identifier,
                    Message = message
                });
            }
            else
            {
                _unhandledMessageHandler?.Invoke(new RoutedUnhandledMessage
                {
                    Message = message
                });
            }
        }
        catch (Exception)
        {
            _errorHandler?.Invoke(new RouterErrorOccurred
            {
                Message = "Router failed to process inbound message.",
                RelatedMessage = message,
                CommandIdentifier = message.Identifier
            });
        }

        return Task.CompletedTask;
    }

    private static string GetConsoleMessageText(IWebRconPayload payload)
    {
        return payload switch
        {
            WebRconTextPayload textPayload => textPayload.Text,
            WebRconChatPayload chatPayload => chatPayload.Message,
            _ => string.Empty
        };
    }

    public Task OnErrorOccurredAsync(WebRconError error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);

        _errorHandler?.Invoke(new RouterErrorOccurred
        {
            Message = error.Message
        });

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
