using RustMan.Presentation.Modules.Connection.Actions;
using RustMan.Presentation.Modules.Connection.State;

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
}
