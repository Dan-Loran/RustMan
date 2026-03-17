using RustMan.Core.Modules.ConsoleStream.Enums;
using RustMan.Core.Modules.ConsoleStream.Models;
using RustMan.Infrastructure.Modules.ConsoleStream.Runtime;

namespace RustMan.Tests.Modules.ConsoleStream;

public sealed class ConsoleStreamStateModuleTests
{
    [Fact]
    public void GetSnapshot_StartsEmpty()
    {
        var module = new ConsoleStreamStateModule();

        var snapshot = module.GetSnapshot();

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public async Task ClearAsync_RestoresEmptySnapshot()
    {
        var module = new ConsoleStreamStateModule();

        await module.AppendEntryAsync(new ConsoleEntry
        {
            Text = "status",
            Kind = ConsoleEntryKind.Output,
            TimestampUtc = DateTimeOffset.UtcNow
        });

        await module.ClearAsync();

        var snapshot = module.GetSnapshot();
        Assert.Empty(snapshot.Entries);
    }
}
