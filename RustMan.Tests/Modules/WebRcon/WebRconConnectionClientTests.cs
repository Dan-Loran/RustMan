using RustMan.Infrastructure.Modules.WebRcon.Connection;

namespace RustMan.Tests.Modules.WebRcon;

public sealed class WebRconConnectionClientTests
{
    [Fact]
    public async Task SendAsync_ThrowsWhenConnectionIsNotOpen()
    {
        using var client = new WebRconConnectionClient();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync("message"));

        Assert.Equal("WebRcon connection is not open.", exception.Message);
    }

    [Fact]
    public async Task ReceiveAsync_ThrowsWhenConnectionHasNotBeenOpened()
    {
        using var client = new WebRconConnectionClient();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReceiveAsync());

        Assert.Equal("WebRcon connection is not open.", exception.Message);
    }
}
