using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Presentation.Modules.Connection.Actions;
using RustMan.Presentation.Modules.Connection.Models;
using RustMan.Presentation.Modules.Connection.Services;
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

    [Theory]
    [InlineData(WebRconConnectionState.Disconnected, "Disconnected")]
    [InlineData(WebRconConnectionState.Connecting, "Connecting...")]
    [InlineData(WebRconConnectionState.Connected, "Connected")]
    [InlineData(WebRconConnectionState.Faulted, "Connection Failed")]
    [InlineData(WebRconConnectionState.Reconnecting, "Disconnected")]
    public void SetConnectionState_ProjectsUiFriendlyStatus(WebRconConnectionState status, string statusText)
    {
        var service = new ConnectionPresentationService(
            new ConnectionPresentationState(),
            new ConnectionInterfaceActions());

        service.SetConnectionState(status);

        Assert.Equal(status, service.ViewModel.Status);
        Assert.Equal(statusText, service.ViewModel.StatusText);
    }
}
