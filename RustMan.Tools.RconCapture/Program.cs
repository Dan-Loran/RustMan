using RustMan.Tools.RconCapture.Capture;

var exitCode = await RunAsync(args);
return exitCode;

static async Task<int> RunAsync(string[] args)
{
    try
    {
        var options = ParseArguments(args);

        Console.WriteLine("RustMan.Tools.RconCapture");
        Console.WriteLine($"Host: {options.Host}");
        Console.WriteLine($"Port: {options.Port}");
        Console.WriteLine($"Command: {options.Command}");
        Console.WriteLine($"DurationSeconds: {options.DurationSeconds}");
        Console.WriteLine($"Output: {options.OutputPath}");

        var session = new RconCaptureSession(
            options.Host,
            options.Port,
            options.Password,
            options.Command,
            options.OutputPath,
            TimeSpan.FromSeconds(options.DurationSeconds));

        await session.RunAsync();

        Console.WriteLine("Capture complete.");
        return 0;
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine($"Argument error: {ex.Message}");
        PrintUsage();
        return 1;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Capture failed: {ex.Message}");
        return 1;
    }
}

static CaptureOptions ParseArguments(string[] args)
{
    string? host = null;
    int? port = null;
    string? password = null;
    var command = "status";
    string? output = null;
    var durationSeconds = 5;

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (!arg.StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Unexpected argument '{arg}'.");
        }

        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for '{arg}'.");
        }

        var value = args[++i];

        switch (arg)
        {
            case "--host":
                host = value;
                break;
            case "--port":
                if (!int.TryParse(value, out var parsedPort) || parsedPort <= 0 || parsedPort > 65535)
                {
                    throw new ArgumentException("Port must be an integer between 1 and 65535.");
                }

                port = parsedPort;
                break;
            case "--password":
                password = value;
                break;
            case "--command":
                command = value;
                break;
            case "--output":
                output = value;
                break;
            case "--duration-seconds":
                if (!int.TryParse(value, out var parsedDuration) || parsedDuration <= 0)
                {
                    throw new ArgumentException("DurationSeconds must be a positive integer.");
                }

                durationSeconds = parsedDuration;
                break;
            default:
                throw new ArgumentException($"Unknown argument '{arg}'.");
        }
    }

    if (string.IsNullOrWhiteSpace(host))
    {
        throw new ArgumentException("Host is required.");
    }

    if (port is null)
    {
        throw new ArgumentException("Port is required.");
    }

    if (string.IsNullOrWhiteSpace(password))
    {
        throw new ArgumentException("Password is required.");
    }

    if (string.IsNullOrWhiteSpace(command))
    {
        throw new ArgumentException("Command cannot be empty.");
    }

    var outputPath = string.IsNullOrWhiteSpace(output)
        ? Path.Combine(
            AppContext.BaseDirectory,
            $"rcon-capture-{DateTime.UtcNow:yyyyMMdd-HHmmss}.jsonl")
        : Path.GetFullPath(output);

    return new CaptureOptions(
        host.Trim(),
        port.Value,
        password,
        command,
        outputPath,
        durationSeconds);
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  --host <host> --port <port> --password <password> [--command <command>] [--output <path>] [--duration-seconds <seconds>]");
}

internal sealed record CaptureOptions(
    string Host,
    int Port,
    string Password,
    string Command,
    string OutputPath,
    int DurationSeconds);
