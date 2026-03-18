using System.Net.WebSockets;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;
using RustMan.Infrastructure.Modules.WebRcon.Connection;
using RustMan.Infrastructure.Modules.WebRcon.Protocol;

namespace RustMan.Infrastructure.Modules.WebRcon.Runtime;

public sealed class WebRconModule : IWebRconModule
{
    private const int RetryCount = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    private readonly IWebRconConnectionClient _connectionClient;
    private readonly IWebRconProtocolTranslator _protocolTranslator;
    private readonly object _syncLock = new();
    private IWebRconConsumer? _consumer;
    private WebRconConnectionRequest? _connectionRequest;
    private Task? _receiveLoopTask;
    private Task<bool>? _reconnectTask;

    public WebRconModule()
        : this(new WebRconConnectionClient(), new WebRconProtocolTranslator())
    {
    }

    public WebRconModule(
        IWebRconConnectionClient connectionClient,
        IWebRconProtocolTranslator protocolTranslator)
    {
        _connectionClient = connectionClient;
        _protocolTranslator = protocolTranslator;
    }

    public void SetConsumer(IWebRconConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        _consumer = consumer;
    }

    public async Task ConnectAsync(WebRconConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var consumer = _consumer ?? throw new InvalidOperationException("WebRcon consumer has not been set.");
        var connectionRequest = PrepareConnectionRequest(request);
        _connectionRequest = connectionRequest;

        try
        {
            await consumer.OnConnectionStateChangedAsync(WebRconConnectionState.Connecting, cancellationToken);
            await ConnectWithRetriesAsync(connectionRequest, cancellationToken);
            await consumer.OnConnectionStateChangedAsync(WebRconConnectionState.Connected, cancellationToken);
            StartReceiveLoop(consumer);
        }
        catch (Exception exception)
        {
            await ReportFaultAsync(
                consumer,
                "Failed to connect WebRcon after retry attempts.",
                exception.Message,
                cancellationToken);
            throw;
        }
    }

    public async Task SendCommandAsync(WebRconCommandRequest command, CancellationToken cancellationToken = default)
    {
        var consumer = _consumer ?? throw new InvalidOperationException("WebRcon consumer has not been set.");
        string rawMessage;

        try
        {
            rawMessage = _protocolTranslator.SerializeCommand(command);
        }
        catch (Exception exception)
        {
            await consumer.OnErrorOccurredAsync(new WebRconError
            {
                Message = "Failed to send WebRcon command.",
                Detail = exception.Message
            }, cancellationToken);
            throw;
        }

        try
        {
            await _connectionClient.SendAsync(rawMessage, cancellationToken);
        }
        catch (Exception exception)
        {
            await consumer.OnErrorOccurredAsync(new WebRconError
            {
                Message = "Failed to send WebRcon command.",
                Detail = exception.Message
            }, cancellationToken);

            if (IsTransportFailure(exception))
            {
                await EnsureReconnectAsync(consumer, cancellationToken);
            }

            throw;
        }
    }

    private async Task ReceiveLoopAsync(IWebRconConsumer consumer)
    {
        while (true)
        {
            string? rawMessage;

            try
            {
                rawMessage = await _connectionClient.ReceiveAsync();
            }
            catch (Exception exception)
            {
                if (IsTransportFailure(exception))
                {
                    if (!await EnsureReconnectAsync(consumer))
                    {
                        return;
                    }

                    continue;
                }

                await ReportFaultAsync(consumer, "Failed to receive WebRcon message.", exception.Message);
                return;
            }

            if (rawMessage is null)
            {
                if (!await EnsureReconnectAsync(consumer))
                {
                    return;
                }

                continue;
            }

            try
            {
                var inboundMessage = _protocolTranslator.DeserializeInboundMessage(rawMessage);
                await consumer.OnMessageReceivedAsync(inboundMessage);
            }
            catch (Exception exception)
            {
                await consumer.OnErrorOccurredAsync(new WebRconError
                {
                    Message = "Failed to translate inbound WebRcon message.",
                    Detail = exception.Message
                });
            }
        }
    }

    private async Task ConnectWithRetriesAsync(WebRconConnectionRequest request, CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 0; attempt <= RetryCount; attempt++)
        {
            try
            {
                await _connectionClient.ConnectAsync(request, cancellationToken);
                return;
            }
            catch (Exception exception)
            {
                lastException = exception;

                if (attempt == RetryCount)
                {
                    break;
                }

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("WebRcon connection attempt failed.");
    }

    private static WebRconConnectionRequest PrepareConnectionRequest(WebRconConnectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var serverUriBuilder = new UriBuilder(request.ServerUri)
        {
            Path = "/"
        };

        return new WebRconConnectionRequest
        {
            ServerUri = serverUriBuilder.Uri,
            Password = request.Password
        };
    }

    private Task<bool> EnsureReconnectAsync(IWebRconConsumer consumer, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            if (_reconnectTask is { IsCompleted: false })
            {
                return _reconnectTask;
            }

            _reconnectTask = ReconnectAsync(consumer, cancellationToken);
            return _reconnectTask;
        }
    }

    private async Task<bool> ReconnectAsync(IWebRconConsumer consumer, CancellationToken cancellationToken)
    {
        var request = _connectionRequest ?? throw new InvalidOperationException("WebRcon connection request has not been set.");
        await consumer.OnConnectionStateChangedAsync(WebRconConnectionState.Reconnecting, cancellationToken);

        Exception? lastException = null;

        for (var attempt = 0; attempt <= RetryCount; attempt++)
        {
            try
            {
                await _connectionClient.ConnectAsync(request, cancellationToken);
                await consumer.OnConnectionStateChangedAsync(WebRconConnectionState.Connected, cancellationToken);
                return true;
            }
            catch (Exception exception)
            {
                lastException = exception;

                if (attempt == RetryCount)
                {
                    break;
                }

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        await ReportFaultAsync(
            consumer,
            "WebRcon connection lost and reconnect attempts were exhausted.",
            lastException?.Message,
            cancellationToken);

        return false;
    }

    private void StartReceiveLoop(IWebRconConsumer consumer)
    {
        lock (_syncLock)
        {
            if (_receiveLoopTask is { IsCompleted: false })
            {
                return;
            }

            _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(consumer));
        }
    }

    private static bool IsTransportFailure(Exception exception)
    {
        return exception is InvalidOperationException
            or ObjectDisposedException
            or WebSocketException;
    }

    private static async Task ReportFaultAsync(
        IWebRconConsumer consumer,
        string message,
        string? detail = null,
        CancellationToken cancellationToken = default)
    {
        await consumer.OnConnectionStateChangedAsync(WebRconConnectionState.Faulted, cancellationToken);
        await consumer.OnErrorOccurredAsync(new WebRconError
        {
            Message = message,
            Detail = detail
        }, cancellationToken);
    }
}
