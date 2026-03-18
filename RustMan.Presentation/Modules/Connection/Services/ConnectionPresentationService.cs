using RustMan.Presentation.Modules.Connection.Actions;
using RustMan.Presentation.Modules.Connection.Models;
using RustMan.Presentation.Modules.Connection.State;
using RustMan.Core.Modules.WebRcon.Enums;

namespace RustMan.Presentation.Modules.Connection.Services;

public sealed class ConnectionPresentationService
{
    public ConnectionPresentationService(ConnectionPresentationState state, ConnectionInterfaceActions actions)
    {
        State = state;
        Actions = actions;
    }

    public ConnectionPresentationState State { get; }

    public ConnectionInterfaceActions Actions { get; }

    public ConnectionViewModel ViewModel => State.ViewModel;

    public void SetConnectionState(WebRconConnectionState state)
    {
        State.SetViewModel(new ConnectionViewModel
        {
            Status = state,
            StatusText = state switch
            {
                WebRconConnectionState.Connecting => "Connecting...",
                WebRconConnectionState.Connected => "Connected",
                WebRconConnectionState.Faulted => "Connection Failed",
                _ => "Disconnected"
            }
        });
    }
}
