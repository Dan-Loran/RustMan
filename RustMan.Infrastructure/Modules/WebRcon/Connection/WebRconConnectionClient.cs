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

        DisposeSocket();

        try
        {
            await socket.ConnectAsync(connectionUri, cancellationToken);
            _socket = socket;
        }
        catch
        {
            socket.Dispose();
            throw;
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

        var builder = new UriBuilder(serverUri);
        var existingPath = builder.Path.TrimEnd('/');
        var escapedPassword = Uri.EscapeDataString(password);

        builder.Path = string.IsNullOrEmpty(existingPath) || existingPath == "/"
            ? $"/{escapedPassword}"
            : $"{existingPath}/{escapedPassword}";

        return builder.Uri;
    }

    private void DisposeSocket()
    {
        _socket?.Dispose();
        _socket = null;
    }
}
