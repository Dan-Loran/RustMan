using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Core.Modules.WebRcon.Models;
using RustMan.Infrastructure.Modules.Routing;
using RustMan.Infrastructure.Modules.Wiring;

namespace RustMan.Tests.Infrastructure.Modules.Wiring;

public sealed class RuntimeModuleWiringTests
{
    [Fact]
    public async Task RouterCommandRequest_IsForwardedToWebRcon()
    {
        var routerModule = new RouterModule();
        var webRconModule = new FakeWebRconModule();
        _ = new RuntimeModuleWiring(routerModule, webRconModule);

        await routerModule.RequestCommandAsync(new RouterCommandRequested
        {
            CommandText = "status",
            Parameters = Array.Empty<string>()
        });

        var command = Assert.Single(webRconModule.SentCommands);
        Assert.Equal(1, command.Identifier);
        Assert.Equal("status", command.Message);
        Assert.Equal("RustMan.Router", command.Name);
    }

    [Fact]
    public async Task MatchingInboundResponse_IsRoutedBackThroughRouter()
    {
        var routerModule = new RouterModule();
        var webRconModule = new FakeWebRconModule();
        _ = new RuntimeModuleWiring(routerModule, webRconModule);

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
    public async Task DisconnectedState_ClearsPendingCommandCorrelation()
    {
        var routerModule = new RouterModule();
        var webRconModule = new FakeWebRconModule();
        _ = new RuntimeModuleWiring(routerModule, webRconModule);

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

        await webRconModule.EmitConnectionStateChangedAsync(WebRconConnectionState.Disconnected);
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
        private IWebRconConsumer? _consumer;

        public List<WebRconCommandRequest> SentCommands { get; } = new();

        public void SetConsumer(IWebRconConsumer consumer)
        {
            _consumer = consumer;
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
            if (_consumer is null)
            {
                throw new InvalidOperationException("WebRcon consumer has not been set.");
            }

            return _consumer.OnMessageReceivedAsync(message);
        }

        public Task EmitConnectionStateChangedAsync(WebRconConnectionState state)
        {
            if (_consumer is null)
            {
                throw new InvalidOperationException("WebRcon consumer has not been set.");
            }

            return _consumer.OnConnectionStateChangedAsync(state);
        }
    }
}
