namespace RustMan.Presentation.Modules.Connection.Actions;

public sealed class ConnectionInterfaceActions
{
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
