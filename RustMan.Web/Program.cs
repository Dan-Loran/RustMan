using Microsoft.Data.Sqlite;
using RustMan.Core.Modules.ConsoleStream;
using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;
using RustMan.Infrastructure.Modules.ConsoleStream;
using RustMan.Infrastructure.Modules.Routing;
using RustMan.Infrastructure.Modules.WebRcon.Runtime;
using RustMan.Presentation.Modules.Connection.Services;
using RustMan.Presentation.Modules.ConsoleStream;
using RustMan.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IWebRconModule, WebRconModule>();
builder.Services.AddSingleton<IConsoleStreamModule, ConsoleStreamModule>();
builder.Services.AddSingleton<ConnectionPresentationService>();
builder.Services.AddSingleton<RustMan.Presentation.Modules.Connection.State.ConnectionPresentationState>();
builder.Services.AddSingleton<RustMan.Presentation.Modules.Connection.Actions.ConnectionInterfaceActions>();
builder.Services.AddSingleton<ConsoleStreamState>();
builder.Services.AddSingleton<ConsoleStreamPresentationService>();
builder.Services.AddSingleton<IRouterModule>(serviceProvider =>
    new RouterModule(serviceProvider.GetRequiredService<IWebRconModule>()));

var app = builder.Build();
var router = app.Services.GetRequiredService<IRouterModule>();
var consoleStream = app.Services.GetRequiredService<IConsoleStreamModule>();
var presentationService = app.Services.GetRequiredService<ConsoleStreamPresentationService>();
var connectionPresentationService = app.Services.GetRequiredService<ConnectionPresentationService>();
var webRcon = app.Services.GetRequiredService<IWebRconModule>();
var webRconConsumer = new AppWebRconConsumer(router, connectionPresentationService);

router.SetConsoleConsumer(consoleStream);
consoleStream.SetConsumer(presentationService);
webRcon.SetConsumer(webRconConsumer);
connectionPresentationService.SetConnectionState(WebRconConnectionState.Disconnected);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await ConnectEnabledInstanceAsync(app, webRcon);
        }
        catch (Exception exception)
        {
            app.Logger.LogError(exception, "Initial WebRcon bootstrap connection failed.");
        }
    });
});

app.Run();

static async Task ConnectEnabledInstanceAsync(WebApplication app, IWebRconModule webRconModule)
{
    var connectionString = app.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
    var rconHost = app.Configuration["RuntimeBootstrap:RconHost"];
    if (string.IsNullOrWhiteSpace(rconHost))
    {
        throw new InvalidOperationException("RuntimeBootstrap:RconHost is not configured.");
    }

    var connectionBuilder = new SqliteConnectionStringBuilder(connectionString);
    if (string.IsNullOrWhiteSpace(connectionBuilder.DataSource))
    {
        throw new InvalidOperationException("ConnectionStrings:Default does not specify a database path.");
    }

    if (!Path.IsPathRooted(connectionBuilder.DataSource))
    {
        connectionBuilder.DataSource = Path.GetFullPath(
            Path.Combine(app.Environment.ContentRootPath, connectionBuilder.DataSource));
    }

    await using var connection = new SqliteConnection(connectionBuilder.ToString());
    await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText =
        """
        SELECT name, rcon_port, rcon_password
        FROM instances
        WHERE is_enabled = 1
        LIMIT 2;
        """;

    await using var reader = await command.ExecuteReaderAsync();
    var instances = new List<EnabledInstance>();

    while (await reader.ReadAsync())
    {
        instances.Add(new EnabledInstance(
            reader.GetString(0),
            reader.GetInt32(1),
            reader.GetString(2)));
    }

    if (instances.Count == 0)
    {
        throw new InvalidOperationException("No enabled instance row was found.");
    }

    if (instances.Count > 1)
    {
        throw new InvalidOperationException("Multiple enabled instance rows were found. This bootstrap path expects exactly one.");
    }

    var instance = instances[0];
    app.Logger.LogInformation(
        "Connecting WebRcon for instance {InstanceName} on {RconHost}:{RconPort}.",
        instance.Name,
        rconHost,
        instance.RconPort);

    await webRconModule.ConnectAsync(new WebRconConnectionRequest
    {
        ServerUri = new Uri($"ws://{rconHost}:{instance.RconPort}"),
        Password = instance.RconPassword
    });
}

internal sealed record EnabledInstance(string Name, int RconPort, string RconPassword);

internal sealed class AppWebRconConsumer : IWebRconConsumer
{
    private readonly RouterModule _router;
    private readonly ConnectionPresentationService _connectionPresentationService;

    public AppWebRconConsumer(IRouterModule router, ConnectionPresentationService connectionPresentationService)
    {
        _router = router as RouterModule ?? throw new InvalidOperationException("RouterModule is required for WebRcon bootstrap wiring.");
        _connectionPresentationService = connectionPresentationService;
    }

    public async Task OnConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default)
    {
        _connectionPresentationService.SetConnectionState(state switch
        {
            WebRconConnectionState.Connected => WebRconConnectionState.Connected,
            WebRconConnectionState.Connecting => WebRconConnectionState.Connecting,
            WebRconConnectionState.Faulted => WebRconConnectionState.Faulted,
            _ => WebRconConnectionState.Disconnected
        });

        await _router.OnConnectionStateChangedAsync(state, cancellationToken);
    }

    public Task OnMessageReceivedAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default)
    {
        return _router.OnMessageReceivedAsync(message, cancellationToken);
    }

    public Task OnErrorOccurredAsync(WebRconError error, CancellationToken cancellationToken = default)
    {
        return _router.OnErrorOccurredAsync(error, cancellationToken);
    }
}
