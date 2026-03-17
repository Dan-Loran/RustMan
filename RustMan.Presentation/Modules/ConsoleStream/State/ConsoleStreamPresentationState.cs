using RustMan.Presentation.Modules.ConsoleStream.Models;

namespace RustMan.Presentation.Modules.ConsoleStream.State;

public sealed class ConsoleStreamPresentationState
{
    public ConsoleStreamViewModel ViewModel { get; private set; } = new();

    public void SetViewModel(ConsoleStreamViewModel viewModel)
    {
        ViewModel = viewModel;
    }
}
