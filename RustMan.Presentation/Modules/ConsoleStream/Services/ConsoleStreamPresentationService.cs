using RustMan.Presentation.Modules.ConsoleStream.Actions;
using RustMan.Presentation.Modules.ConsoleStream.State;

namespace RustMan.Presentation.Modules.ConsoleStream.Services;

public sealed class ConsoleStreamPresentationService
{
    public ConsoleStreamPresentationService(ConsoleStreamPresentationState state, ConsoleStreamInterfaceActions actions)
    {
        State = state;
        Actions = actions;
    }

    public ConsoleStreamPresentationState State { get; }

    public ConsoleStreamInterfaceActions Actions { get; }
}
