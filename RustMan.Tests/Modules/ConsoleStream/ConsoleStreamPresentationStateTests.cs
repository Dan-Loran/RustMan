using RustMan.Core.Modules.ConsoleStream;
using RustMan.Presentation.Modules.ConsoleStream;

namespace RustMan.Tests.Modules.ConsoleStream;

public sealed class ConsoleStreamPresentationStateTests
{
    [Fact]
    public async Task OnConsoleStreamStateChangedAsync_UpdatesStateWithProjectedLines()
    {
        var state = new ConsoleStreamState();
        var service = new ConsoleStreamPresentationService(state);
        var timestampUtc = new DateTime(2026, 03, 18, 12, 34, 56, DateTimeKind.Utc);

        await service.OnConsoleStreamStateChangedAsync(new ConsoleStreamStateChanged
        {
            Snapshot = new ConsoleStreamSnapshot
            {
                Entries =
                [
                    new ConsoleEntry
                    {
                        Text = "server ready",
                        Type = "System",
                        TimestampUtc = timestampUtc
                    }
                ]
            }
        });

        var viewModel = service.ViewModel;
        var line = Assert.Single(viewModel.Lines);
        Assert.Equal("server ready", line.Text);
        Assert.Equal("System", line.Type);
        Assert.Equal(timestampUtc, line.TimestampUtc);
        Assert.False(viewModel.IsEmpty);
        Assert.True(viewModel.CanClear);
        Assert.Same(viewModel, state.ViewModel);
    }

    [Fact]
    public async Task OnConsoleStreamStateChangedAsync_WhenSnapshotEmpty_SetsEmptyState()
    {
        var service = new ConsoleStreamPresentationService(new ConsoleStreamState());

        await service.OnConsoleStreamStateChangedAsync(new ConsoleStreamStateChanged
        {
            Snapshot = new ConsoleStreamSnapshot()
        });

        Assert.Empty(service.ViewModel.Lines);
        Assert.True(service.ViewModel.IsEmpty);
        Assert.False(service.ViewModel.CanClear);
    }

    [Fact]
    public async Task OnConsoleStreamStateChangedAsync_ExposesReadOnlyLines()
    {
        var service = new ConsoleStreamPresentationService(new ConsoleStreamState());

        await service.OnConsoleStreamStateChangedAsync(new ConsoleStreamStateChanged
        {
            Snapshot = new ConsoleStreamSnapshot
            {
                Entries =
                [
                    new ConsoleEntry
                    {
                        Text = "line 1",
                        Type = "Generic",
                        TimestampUtc = new DateTime(2026, 03, 18, 13, 00, 00, DateTimeKind.Utc)
                    }
                ]
            }
        });

        Assert.False(service.ViewModel.Lines is List<ConsoleLineViewModel>);
        Assert.True(service.ViewModel.Lines is System.Collections.ObjectModel.ReadOnlyCollection<ConsoleLineViewModel>);
    }

    [Fact]
    public void PresentationModels_ExposeOnlyNeutralFields()
    {
        Assert.Equal(
            ["Text", "Type", "TimestampUtc"],
            typeof(ConsoleLineViewModel)
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Select(property => property.Name));

        Assert.Equal(
            ["Lines", "IsEmpty", "CanClear"],
            typeof(ConsoleStreamViewModel)
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Select(property => property.Name));
    }
}
