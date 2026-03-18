using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;
using RustMan.Infrastructure.Modules.Routing;

namespace RustMan.Tests.Infrastructure.Modules.Routing;

public sealed class RouterModuleTests
{
    [Fact]
    public async Task RequestCommandAsync_ForwardsStructuredCommandDirectlyToWebRcon()
    {
        var webRconModule = new FakeWebRconModule();
        var routerModule = new RouterModule(webRconModule);

        await routerModule.RequestCommandAsync(new RouterCommandRequested
        {
            CommandText = "status",
            Parameters = new[] { "players", "active" }
        });

        Assert.Same(routerModule, webRconModule.Consumer);

        var command = Assert.Single(webRconModule.SentCommands);
        Assert.Equal(1, command.Identifier);
        Assert.Equal("status", command.CommandText);
        Assert.Equal(new[] { "players", "active" }, command.Parameters);
        Assert.Equal("RustMan.Router", command.Name);
    }

    [Fact]
    public async Task MatchingInboundResponse_IsCorrelatedCorrectly()
    {
        var webRconModule = new FakeWebRconModule();
        var routerModule = new RouterModule(webRconModule);

        RoutedCommandResponse? routedResponse = null;
        routerModule.SetCommandResponseOutput((response, cancellationToken) =>
        {
            routedResponse = response;
            return Task.CompletedTask;
        });

        await routerModule.RequestCommandAsync(new RouterCommandRequested
        {
            CommandText = "status",
            Parameters = Array.Empty<string>()
        });

        var dispatchedCommand = Assert.Single(webRconModule.SentCommands);
        var inboundMessage = new WebRconInboundMessage
        {
            Identifier = dispatchedCommand.Identifier,
            Type = "Generic",
            Payload = new WebRconTextPayload
            {
                Text = "hostname: Test Server"
            }
        };

        await webRconModule.EmitInboundMessageAsync(inboundMessage);

        Assert.NotNull(routedResponse);
        Assert.Equal(dispatchedCommand.Identifier, routedResponse!.CommandIdentifier);
        Assert.Same(inboundMessage, routedResponse.Message);
    }

    [Fact]
    public async Task UnmatchedInboundMessage_IsForwardedInsteadOfDropped()
    {
        var webRconModule = new FakeWebRconModule();
        var routerModule = new RouterModule(webRconModule);

        RoutedUnhandledMessage? unhandledMessage = null;
        routerModule.SetUnhandledMessageOutput((message, cancellationToken) =>
        {
            unhandledMessage = message;
            return Task.CompletedTask;
        });

        var inboundMessage = new WebRconInboundMessage
        {
            Identifier = 99,
            Type = "Generic",
            Payload = new WebRconTextPayload
            {
                Text = "server event"
            }
        };

        await webRconModule.EmitInboundMessageAsync(inboundMessage);

        Assert.NotNull(unhandledMessage);
        Assert.Same(inboundMessage, unhandledMessage!.Message);
    }

    [Theory]
    [InlineData(WebRconConnectionState.Disconnected)]
    [InlineData(WebRconConnectionState.Faulted)]
    public async Task DisconnectOrFault_ClearsPendingCommandCorrelation(WebRconConnectionState state)
    {
        var webRconModule = new FakeWebRconModule();
        var routerModule = new RouterModule(webRconModule);

        var routedResponses = new List<RoutedCommandResponse>();
        routerModule.SetCommandResponseOutput((response, cancellationToken) =>
        {
            routedResponses.Add(response);
            return Task.CompletedTask;
        });

        await routerModule.RequestCommandAsync(new RouterCommandRequested
        {
            CommandText = "status",
            Parameters = Array.Empty<string>()
        });

        var dispatchedCommand = Assert.Single(webRconModule.SentCommands);

        await webRconModule.EmitConnectionStateChangedAsync(state);
        await webRconModule.EmitInboundMessageAsync(new WebRconInboundMessage
        {
            Identifier = dispatchedCommand.Identifier,
            Type = "Generic",
            Payload = new WebRconTextPayload
            {
                Text = "hostname: Test Server"
            }
        });

        Assert.Empty(routedResponses);
    }

    private sealed class FakeWebRconModule : IWebRconModule
    {
        public IWebRconConsumer? Consumer { get; private set; }

        public List<WebRconCommandRequest> SentCommands { get; } = new();

        public void SetConsumer(IWebRconConsumer consumer)
        {
            Consumer = consumer;
        }

        public Task ConnectAsync(WebRconConnectionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SendCommandAsync(WebRconCommandRequest command, CancellationToken cancellationToken = default)
        {
            SentCommands.Add(command);
            return Task.CompletedTask;
        }

        public Task EmitInboundMessageAsync(WebRconInboundMessage message)
        {
            if (Consumer is null)
            {
                throw new InvalidOperationException("WebRcon consumer has not been set.");
            }

            return Consumer.OnMessageReceivedAsync(message);
        }

        public Task EmitConnectionStateChangedAsync(WebRconConnectionState state)
        {
            if (Consumer is null)
            {
                throw new InvalidOperationException("WebRcon consumer has not been set.");
            }

            return Consumer.OnConnectionStateChangedAsync(state);
        }
    }
}
