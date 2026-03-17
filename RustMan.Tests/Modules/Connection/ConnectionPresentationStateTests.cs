using RustMan.Presentation.Modules.Connection.Models;
using RustMan.Presentation.Modules.Connection.State;

namespace RustMan.Tests.Modules.Connection;

public sealed class ConnectionPresentationStateTests
{
    [Fact]
    public void SetViewModel_ReplacesCurrentViewModel()
    {
        var state = new ConnectionPresentationState();
        var viewModel = new ConnectionViewModel
        {
            StatusText = "Connected"
        };

        state.SetViewModel(viewModel);

        Assert.Same(viewModel, state.ViewModel);
    }
}
