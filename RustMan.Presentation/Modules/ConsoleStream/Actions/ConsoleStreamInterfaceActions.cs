namespace RustMan.Presentation.Modules.ConsoleStream.Actions;

public sealed class ConsoleStreamInterfaceActions
{
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
