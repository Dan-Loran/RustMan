using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace RustMan.Tools.RconCapture.Capture;

internal sealed class RconCaptureSession
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly string _host;
    private readonly int _port;
    private readonly string _password;
    private readonly string _command;
    private readonly string _outputPath;
    private readonly TimeSpan _duration;

    public RconCaptureSession(
        string host,
        int port,
        string password,
        string command,
        string outputPath,
        TimeSpan duration)
    {
        _host = host;
        _port = port;
        _password = password;
        _command = command;
        _outputPath = outputPath;
        _duration = duration;
    }

    public async Task RunAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_outputPath)!);

        using var socket = new ClientWebSocket();
        await using var stream = new FileStream(_outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));

        var uri = new Uri($"ws://{_host}:{_port}/{_password}");

        Console.WriteLine($"Connecting to {uri}...");
        await socket.ConnectAsync(uri, CancellationToken.None);
        Console.WriteLine("Connected.");

        var outboundPayload = JsonSerializer.Serialize(
            new
            {
                Identifier = 1,
                Message = _command,
                Name = "RustMan.RconCapture"
            },
            JsonOptions);

        Console.WriteLine($"Sending command '{_command}'...");
        await socket.SendAsync(
            Encoding.UTF8.GetBytes(outboundPayload),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None);

        await WriteRecordAsync(writer, "outbound", outboundPayload);

        Console.WriteLine($"Capturing inbound text frames for {_duration.TotalSeconds:0} seconds...");

        using var captureCts = new CancellationTokenSource(_duration);
        var buffer = new byte[4096];

        try
        {
            while (!captureCts.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                using var frameStream = new MemoryStream();

                do
                {
                    result = await socket.ReceiveAsync(buffer, captureCts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Server closed the connection.");
                        return;
                    }

                    if (result.Count > 0)
                    {
                        await frameStream.WriteAsync(buffer.AsMemory(0, result.Count), captureCts.Token);
                    }
                }
                while (!result.EndOfMessage);

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    continue;
                }

                var raw = Encoding.UTF8.GetString(frameStream.ToArray());
                await WriteRecordAsync(writer, "inbound", raw);
            }
        }
        catch (OperationCanceledException) when (captureCts.IsCancellationRequested)
        {
            Console.WriteLine("Capture window elapsed.");
        }
    }

    private static async Task WriteRecordAsync(StreamWriter writer, string direction, string raw)
    {
        var record = new CaptureRecord(
            DateTime.UtcNow,
            direction,
            raw);

        var line = JsonSerializer.Serialize(record, JsonOptions);
        await writer.WriteLineAsync(line);
        await writer.FlushAsync();
    }
}
