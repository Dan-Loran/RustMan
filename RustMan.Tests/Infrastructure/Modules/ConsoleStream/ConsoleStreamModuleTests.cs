using RustMan.Core.Modules.ConsoleStream;
using RustMan.Infrastructure.Modules.ConsoleStream;

namespace RustMan.Tests.Infrastructure.Modules.ConsoleStream;

public sealed class ConsoleStreamModuleTests
{
    [Fact]
    public async Task HandleMessageAsync_OneMessageCreatesOneEntry()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);

        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "hostname: test",
            Type = "Generic",
            TimestampUtc = new DateTime(2026, 03, 17, 12, 00, 00, DateTimeKind.Utc)
        });

        var state = Assert.Single(consumer.States);
        var entry = Assert.Single(state.Snapshot.Entries);
        Assert.Equal("hostname: test", entry.Text);
    }

    [Fact]
    public async Task HandleMessageAsync_PreservesTextTypeAndTimestamp()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);
        var timestampUtc = new DateTime(2026, 03, 17, 12, 34, 56, DateTimeKind.Utc);

        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "server ready",
            Type = "System",
            TimestampUtc = timestampUtc
        });

        var entry = Assert.Single(Assert.Single(consumer.States).Snapshot.Entries);
        Assert.Equal("server ready", entry.Text);
        Assert.Equal("System", entry.Type);
        Assert.Equal(timestampUtc, entry.TimestampUtc);
    }

    [Fact]
    public async Task HandleMessageAsync_NotifiesConsumerAfterMessageHandled()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);

        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "first line",
            Type = "Generic",
            TimestampUtc = new DateTime(2026, 03, 17, 13, 00, 00, DateTimeKind.Utc)
        });

        Assert.Single(consumer.States);
    }

    [Fact]
    public async Task HandleMessageAsync_SnapshotPreservesArrivalOrder()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);

        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "line 1",
            Type = "Generic",
            TimestampUtc = new DateTime(2026, 03, 17, 13, 00, 00, DateTimeKind.Utc)
        });
        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "line 2",
            Type = "Warning",
            TimestampUtc = new DateTime(2026, 03, 17, 13, 00, 01, DateTimeKind.Utc)
        });
        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "line 3",
            Type = "System",
            TimestampUtc = new DateTime(2026, 03, 17, 13, 00, 02, DateTimeKind.Utc)
        });

        var entries = consumer.States.Last().Snapshot.Entries;
        Assert.Equal(new[] { "line 1", "line 2", "line 3" }, entries.Select(entry => entry.Text));
    }

    [Fact]
    public async Task HandleMessageAsync_When501MessagesHandled_OnlyNewest500Remain()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);

        for (var index = 1; index <= 501; index++)
        {
            await module.HandleMessageAsync(new RoutedConsoleMessage
            {
                Message = $"line {index}",
                Type = "Generic",
                TimestampUtc = new DateTime(2026, 03, 17, 14, 00, 00, DateTimeKind.Utc).AddSeconds(index)
            });
        }

        var entries = consumer.States.Last().Snapshot.Entries;
        Assert.Equal(500, entries.Count);
    }

    [Fact]
    public async Task HandleMessageAsync_PurgesOldestEntriesWhenLimitExceeded()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);

        for (var index = 1; index <= 501; index++)
        {
            await module.HandleMessageAsync(new RoutedConsoleMessage
            {
                Message = $"line {index}",
                Type = "Generic",
                TimestampUtc = new DateTime(2026, 03, 17, 15, 00, 00, DateTimeKind.Utc).AddSeconds(index)
            });
        }

        var entries = consumer.States.Last().Snapshot.Entries;
        Assert.Equal("line 2", entries.First().Text);
        Assert.Equal("line 501", entries.Last().Text);
    }

    [Fact]
    public async Task HandleMessageAsync_SnapshotDoesNotExposeMutableInternalCollection()
    {
        var consumer = new RecordingConsumer();
        var module = new ConsoleStreamModule();
        module.SetConsumer(consumer);

        await module.HandleMessageAsync(new RoutedConsoleMessage
        {
            Message = "line 1",
            Type = "Generic",
            TimestampUtc = new DateTime(2026, 03, 17, 16, 00, 00, DateTimeKind.Utc)
        });

        var entries = consumer.States.Last().Snapshot.Entries;

        Assert.False(entries is List<ConsoleEntry>);
        Assert.True(entries is System.Collections.ObjectModel.ReadOnlyCollection<ConsoleEntry>);
    }

    private sealed class RecordingConsumer : IConsoleStreamConsumer
    {
        public List<ConsoleStreamStateChanged> States { get; } = new();

        public Task OnConsoleStreamStateChangedAsync(ConsoleStreamStateChanged state, CancellationToken cancellationToken = default)
        {
            States.Add(state);
            return Task.CompletedTask;
        }
    }
}
