namespace RustMan.Presentation.Modules.ConsoleStream;

public sealed class ConsoleStreamState
{
    public event Action? Changed;

    public ConsoleStreamViewModel ViewModel { get; private set; } = ConsoleStreamViewModel.Empty;

    public void SetViewModel(ConsoleStreamViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        Changed?.Invoke();
    }
}
