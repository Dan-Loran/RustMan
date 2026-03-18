using System.Net.WebSockets;
using System.Text;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.WebRcon.Connection;

public sealed class WebRconConnectionClient : IWebRconConnectionClient, IDisposable
{
    private ClientWebSocket? _socket;

    public async Task ConnectAsync(WebRconConnectionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var socket = new ClientWebSocket();
        var connectionUri = BuildConnectionUri(request.ServerUri, request.Password);
        ConfigureSocket(socket);

        DisposeSocket();

        try
        {
            await socket.ConnectAsync(connectionUri, cancellationToken);
            _socket = socket;
        }
        catch (Exception exception)
        {
            socket.Dispose();
            throw CreateConnectionException(connectionUri, socket.Options, exception);
        }
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var socket = GetConnectedSocket();
        var payload = Encoding.UTF8.GetBytes(message);

        await socket.SendAsync(payload, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
    }

    public async Task<string?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var socket = GetReadableSocket();
        var buffer = new byte[4096];
        using var stream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                throw new InvalidOperationException("WebRcon connection only supports text messages.");
            }

            if (result.Count > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken);
            }

            if (result.EndOfMessage)
            {
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

    public void Dispose()
    {
        DisposeSocket();
        GC.SuppressFinalize(this);
    }

    private ClientWebSocket GetConnectedSocket()
    {
        if (_socket is null || _socket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebRcon connection is not open.");
        }

        return _socket;
    }

    private ClientWebSocket GetReadableSocket()
    {
        if (_socket is null)
        {
            throw new InvalidOperationException("WebRcon connection is not open.");
        }

        return _socket.State switch
        {
            WebSocketState.Open => _socket,
            WebSocketState.CloseReceived => _socket,
            WebSocketState.Closed => _socket,
            _ => throw new InvalidOperationException("WebRcon connection is not open.")
        };
    }

    private static Uri BuildConnectionUri(Uri serverUri, string password)
    {
        ArgumentNullException.ThrowIfNull(serverUri);
        ArgumentNullException.ThrowIfNull(password);

        return new Uri($"{serverUri.Scheme}://{serverUri.Host}:{serverUri.Port}/{password}");
    }

    private static void ConfigureSocket(ClientWebSocket socket)
    {
        ArgumentNullException.ThrowIfNull(socket);

        socket.Options.Proxy = null;
    }

    private static Exception CreateConnectionException(
        Uri connectionUri,
        ClientWebSocketOptions options,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(connectionUri);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(exception);

        var diagnostics = new StringBuilder()
            .Append("WebRcon WebSocket connect failed. ")
            .Append("Uri=").Append(connectionUri).Append("; ")
            .Append("Scheme=").Append(connectionUri.Scheme).Append("; ")
            .Append("KeepAliveInterval=").Append(options.KeepAliveInterval).Append("; ")
            .Append("CollectHttpResponseDetails=").Append(options.CollectHttpResponseDetails).Append("; ")
            .Append("DangerousDeflateOptionsConfigured=").Append(options.DangerousDeflateOptions is not null).Append("; ")
            .Append("ProxyConfigured=").Append(options.Proxy is not null).Append("; ")
            .Append("CredentialsConfigured=").Append(options.Credentials is not null).Append("; ")
            .Append("ClientCertificatesConfigured=").Append(options.ClientCertificates?.Count ?? 0).Append("; ")
            .Append("CookiesConfigured=").Append(options.Cookies is not null).Append("; ")
            .Append("ExceptionChain=").Append(BuildExceptionChain(exception));

        return new InvalidOperationException(diagnostics.ToString(), exception);
    }

    private static string BuildExceptionChain(Exception exception)
    {
        var builder = new StringBuilder();
        var current = exception;
        var depth = 0;

        while (current is not null)
        {
            if (depth > 0)
            {
                builder.Append(" --> ");
            }

            builder.Append(current.GetType().FullName)
                .Append(": ")
                .Append(current.Message);

            current = current.InnerException;
            depth++;
        }

        return builder.ToString();
    }

    private void DisposeSocket()
    {
        _socket?.Dispose();
        _socket = null;
    }
}
