using RustMan.Presentation.Modules.Connection.Models;

namespace RustMan.Presentation.Modules.Connection.State;

public sealed class ConnectionPresentationState
{
    public ConnectionViewModel ViewModel { get; private set; } = new();

    public void SetViewModel(ConnectionViewModel viewModel)
    {
        ViewModel = viewModel;
    }
}
