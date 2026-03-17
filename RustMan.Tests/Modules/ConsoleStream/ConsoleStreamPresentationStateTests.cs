using RustMan.Presentation.Modules.ConsoleStream.Models;
using RustMan.Presentation.Modules.ConsoleStream.State;

namespace RustMan.Tests.Modules.ConsoleStream;

public sealed class ConsoleStreamPresentationStateTests
{
    [Fact]
    public void SetViewModel_ReplacesCurrentViewModel()
    {
        var state = new ConsoleStreamPresentationState();
        var viewModel = new ConsoleStreamViewModel
        {
            Lines = new[] { "line 1" }
        };

        state.SetViewModel(viewModel);

        Assert.Same(viewModel, state.ViewModel);
    }
}
