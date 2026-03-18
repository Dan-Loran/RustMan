using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Core.Modules.WebRcon.Models;
using RustMan.Core.Modules.WebRcon.Enums;
using RustMan.Infrastructure.Modules.WebRcon.Connection;
using RustMan.Infrastructure.Modules.WebRcon.Protocol;
using RustMan.Infrastructure.Modules.WebRcon.Runtime;

namespace RustMan.Tests.Modules.WebRcon;

public sealed class WebRconModuleTests
{
    [Fact]
    public async Task ConnectAsync_NotifiesConnectingThenConnected()
    {
        var consumer = new RecordingConsumer(expectedStateCount: 2);
        var module = new WebRconModule(
            new StubConnectionClient(receivedMessages: Array.Empty<string?>(), returnNullWhenEmpty: false),
            new StubProtocolTranslator());
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        await consumer.StateCountReached.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal(WebRconConnectionState.Connecting, consumer.States[0]);
        Assert.Equal(WebRconConnectionState.Connected, consumer.States[1]);
    }

    [Fact]
    public async Task ConnectAsync_RetriesBeforeTerminalFault()
    {
        var consumer = new RecordingConsumer(expectedErrorCount: 1);
        var connectionClient = new StubConnectionClient(connectExceptions: new[]
        {
            new InvalidOperationException("connect-1"),
            new InvalidOperationException("connect-2"),
            new InvalidOperationException("connect-3"),
            new InvalidOperationException("connect-4")
        });
        var module = new WebRconModule(
            connectionClient,
            new StubProtocolTranslator());
        module.SetConsumer(consumer);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        }));

        var error = await consumer.ErrorReceived.Task.WaitAsync(TimeSpan.FromSeconds(8));

        Assert.Equal(4, connectionClient.ConnectCallCount);
        Assert.Equal("connect-4", exception.Message);
        Assert.Equal("Failed to connect WebRcon after retry attempts.", error.Message);
        Assert.Contains(WebRconConnectionState.Faulted, consumer.States);
    }

    [Fact]
    public async Task ConnectAsync_SucceedsOnLaterRetry()
    {
        var consumer = new RecordingConsumer(expectedStateCount: 2);
        var connectionClient = new StubConnectionClient(
            receivedMessages: Array.Empty<string?>(),
            connectExceptions: new[]
            {
                new InvalidOperationException("connect-1"),
                new InvalidOperationException("connect-2")
            },
            returnNullWhenEmpty: false);
        var module = new WebRconModule(connectionClient, new StubProtocolTranslator());
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        await consumer.StateCountReached.Task.WaitAsync(TimeSpan.FromSeconds(6));

        Assert.Equal(3, connectionClient.ConnectCallCount);
        Assert.Equal(new[] { WebRconConnectionState.Connecting, WebRconConnectionState.Connected }, consumer.States.Take(2));
    }

    [Fact]
    public async Task ConnectAsync_StartsReceiveFlowAndForwardsTranslatedMessages()
    {
        var consumer = new RecordingConsumer(expectedMessageCount: 1);
        var translatedMessage = new WebRconInboundMessage
        {
            Identifier = 7,
            Type = "Generic",
            Payload = new WebRconTextPayload
            {
                Text = "hostname: Test Server"
            }
        };
        var module = new WebRconModule(
            new StubConnectionClient("raw-message", null),
            new StubProtocolTranslator(translatedMessage));
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        var message = await consumer.MessageReceived.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Same(translatedMessage, message);
    }

    [Fact]
    public async Task SendCommandAsync_SerializesThenSends()
    {
        var consumer = new RecordingConsumer();
        var connectionClient = new StubConnectionClient(receivedMessages: Array.Empty<string?>(), returnNullWhenEmpty: false);
        var protocolTranslator = new StubProtocolTranslator
        {
            SerializedCommand = "{\"Message\":\"status\"}"
        };
        var module = new WebRconModule(connectionClient, protocolTranslator);
        module.SetConsumer(consumer);

        var command = new WebRconCommandRequest
        {
            Identifier = 1,
            CommandText = "status",
            Parameters = new[] { "players", "active" },
            Name = "WebRconCommand"
        };

        await module.SendCommandAsync(command);

        Assert.Same(command, protocolTranslator.LastSerializedCommand);
        Assert.Equal("{\"Message\":\"status\"}", connectionClient.LastSentMessage);
    }

    [Fact]
    public async Task ReceiveLoop_ReconnectsWhenConnectionCloses()
    {
        var consumer = new RecordingConsumer(expectedStateCount: 4, expectedMessageCount: 1);
        var module = new WebRconModule(new StubConnectionClient(null, "raw-after-reconnect"), new StubProtocolTranslator());
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        var message = await consumer.MessageReceived.Task.WaitAsync(TimeSpan.FromSeconds(3));
        await consumer.StateCountReached.Task.WaitAsync(TimeSpan.FromSeconds(3));

        Assert.Equal("raw-after-reconnect", ((WebRconTextPayload)message.Payload).Text);
        Assert.Equal(new[]
        {
            WebRconConnectionState.Connecting,
            WebRconConnectionState.Connected,
            WebRconConnectionState.Reconnecting,
            WebRconConnectionState.Connected
        }, consumer.States.Take(4));
    }

    [Fact]
    public async Task ReceiveLoop_ReportsTranslationErrorWithoutReconnect()
    {
        var consumer = new RecordingConsumer(expectedErrorCount: 1);
        var module = new WebRconModule(
            new StubConnectionClient(receivedMessages: new[] { "raw-message" }, returnNullWhenEmpty: false),
            new StubProtocolTranslator(translationException: new InvalidOperationException("bad payload")));
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        var error = await consumer.ErrorReceived.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal("Failed to translate inbound WebRcon message.", error.Message);
        Assert.Equal("bad payload", error.Detail);
        Assert.DoesNotContain(WebRconConnectionState.Reconnecting, consumer.States);
    }

    [Fact]
    public async Task ReceiveLoop_ReportsFaultedWhenReconnectIsExhausted()
    {
        var consumer = new RecordingConsumer(expectedErrorCount: 1);
        var module = new WebRconModule(
            new StubConnectionClient(
                receivedMessages: new string?[] { null },
                connectExceptions: new[]
                {
                    null,
                    new InvalidOperationException("reconnect-1"),
                    new InvalidOperationException("reconnect-2"),
                    new InvalidOperationException("reconnect-3"),
                    new InvalidOperationException("reconnect-4")
                }),
            new StubProtocolTranslator());
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        var error = await consumer.ErrorReceived.Task.WaitAsync(TimeSpan.FromSeconds(8));

        Assert.Contains(WebRconConnectionState.Faulted, consumer.States);
        Assert.Equal("WebRcon connection lost and reconnect attempts were exhausted.", error.Message);
    }

    [Fact]
    public async Task SendCommandAsync_TriggersReconnectAttemptAndRethrowsWhenTransportFails()
    {
        var consumer = new RecordingConsumer(expectedErrorCount: 1);
        var connectionClient = new StubConnectionClient(
            receivedMessages: Array.Empty<string?>(),
            connectExceptions: Array.Empty<Exception?>(),
            sendException: new InvalidOperationException("send failed"),
            returnNullWhenEmpty: false);
        var module = new WebRconModule(
            connectionClient,
            new StubProtocolTranslator
            {
                SerializedCommand = "{\"Message\":\"status\"}"
            });
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => module.SendCommandAsync(new WebRconCommandRequest
        {
            Identifier = 1,
            CommandText = "status",
            Name = "WebRconCommand"
        }));

        var error = await consumer.ErrorReceived.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal("send failed", exception.Message);
        Assert.Equal("Failed to send WebRcon command.", error.Message);
        Assert.Contains(WebRconConnectionState.Reconnecting, consumer.States);
        Assert.Equal(2, connectionClient.ConnectCallCount);
    }

    [Fact]
    public async Task OverlappingSendFailures_DoNotStartDuplicateReconnectAttempts()
    {
        var consumer = new RecordingConsumer(expectedErrorCount: 2);
        var connectionClient = new StubConnectionClient(
            receivedMessages: Array.Empty<string?>(),
            connectExceptions: Array.Empty<Exception?>(),
            connectDelay: TimeSpan.FromMilliseconds(300),
            sendException: new InvalidOperationException("send failed"),
            returnNullWhenEmpty: false);
        var module = new WebRconModule(
            connectionClient,
            new StubProtocolTranslator
            {
                SerializedCommand = "{\"Message\":\"status\"}"
            });
        module.SetConsumer(consumer);

        await module.ConnectAsync(new WebRconConnectionRequest
        {
            ServerUri = new Uri("wss://localhost:28016"),
            Password = "password"
        });

        var firstSend = Assert.ThrowsAsync<InvalidOperationException>(() => module.SendCommandAsync(new WebRconCommandRequest
        {
            Identifier = 1,
            CommandText = "status",
            Name = "WebRconCommand"
        }));
        var secondSend = Assert.ThrowsAsync<InvalidOperationException>(() => module.SendCommandAsync(new WebRconCommandRequest
        {
            Identifier = 2,
            CommandText = "status",
            Name = "WebRconCommand"
        }));

        await Task.WhenAll(firstSend, secondSend);

        Assert.Equal(2, connectionClient.ConnectCallCount);
    }

    private sealed class StubConnectionClient : IWebRconConnectionClient
    {
        private readonly Queue<string?> _receivedMessages;
        private readonly Queue<Exception?> _connectExceptions;
        private readonly Exception? _sendException;
        private readonly TimeSpan _connectDelay;
        private readonly bool _returnNullWhenEmpty;
        private readonly TaskCompletionSource<string?> _pendingReceive = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public StubConnectionClient(
            params string?[] receivedMessages)
            : this(receivedMessages, Array.Empty<Exception?>(), null, TimeSpan.Zero)
        {
        }

        public StubConnectionClient(
            string?[]? receivedMessages = null,
            Exception?[]? connectExceptions = null,
            Exception? sendException = null,
            TimeSpan? connectDelay = null,
            bool returnNullWhenEmpty = true)
        {
            _receivedMessages = new Queue<string?>(receivedMessages ?? Array.Empty<string?>());
            _connectExceptions = new Queue<Exception?>(connectExceptions ?? Array.Empty<Exception?>());
            _sendException = sendException;
            _connectDelay = connectDelay ?? TimeSpan.Zero;
            _returnNullWhenEmpty = returnNullWhenEmpty;
        }

        public string? LastSentMessage { get; private set; }

        public int ConnectCallCount { get; private set; }

        public async Task ConnectAsync(WebRconConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ConnectCallCount++;

            if (_connectDelay > TimeSpan.Zero)
            {
                await Task.Delay(_connectDelay, cancellationToken);
            }

            if (_connectExceptions.Count > 0)
            {
                var exception = _connectExceptions.Dequeue();
                if (exception is not null)
                {
                    throw exception;
                }
            }
        }

        public Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            if (_sendException is not null)
            {
                throw _sendException;
            }

            LastSentMessage = message;
            return Task.CompletedTask;
        }

        public Task<string?> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            if (_receivedMessages.Count == 0)
            {
                if (_returnNullWhenEmpty)
                {
                    return Task.FromResult<string?>(null);
                }

                return _pendingReceive.Task.WaitAsync(cancellationToken);
            }

            return Task.FromResult(_receivedMessages.Dequeue());
        }
    }

    private sealed class StubProtocolTranslator : IWebRconProtocolTranslator
    {
        private readonly WebRconInboundMessage? _deserializedMessage;
        private readonly Exception? _translationException;

        public StubProtocolTranslator(
            WebRconInboundMessage? deserializedMessage = null,
            Exception? translationException = null)
        {
            _deserializedMessage = deserializedMessage;
            _translationException = translationException;
        }

        public WebRconCommandRequest? LastSerializedCommand { get; private set; }

        public string SerializedCommand { get; init; } = "{}";

        public WebRconInboundMessage DeserializeInboundMessage(string rawMessage)
        {
            if (_translationException is not null)
            {
                throw _translationException;
            }

            return _deserializedMessage ?? new WebRconInboundMessage
            {
                Identifier = 0,
                Type = "Generic",
                Payload = new WebRconTextPayload
                {
                    Text = rawMessage
                }
            };
        }

        public string SerializeCommand(WebRconCommandRequest command)
        {
            LastSerializedCommand = command;
            return SerializedCommand;
        }
    }

    private sealed class RecordingConsumer : IWebRconConsumer
    {
        private readonly int _expectedStateCount;
        private readonly int _expectedMessageCount;
        private readonly int _expectedErrorCount;
        private int _messageCount;
        private int _errorCount;

        public RecordingConsumer(
            int expectedStateCount = 0,
            int expectedMessageCount = 0,
            int expectedErrorCount = 0)
        {
            _expectedStateCount = expectedStateCount;
            _expectedMessageCount = expectedMessageCount;
            _expectedErrorCount = expectedErrorCount;
        }

        public List<WebRconConnectionState> States { get; } = new();

        public TaskCompletionSource<bool> StateCountReached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<WebRconInboundMessage> MessageReceived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<WebRconError> ErrorReceived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task OnConnectionStateChangedAsync(WebRconConnectionState state, CancellationToken cancellationToken = default)
        {
            States.Add(state);

            if (_expectedStateCount > 0 && States.Count >= _expectedStateCount)
            {
                StateCountReached.TrySetResult(true);
            }

            return Task.CompletedTask;
        }

        public Task OnMessageReceivedAsync(WebRconInboundMessage message, CancellationToken cancellationToken = default)
        {
            _messageCount++;
            if (_expectedMessageCount == 0 || _messageCount >= _expectedMessageCount)
            {
                MessageReceived.TrySetResult(message);
            }

            return Task.CompletedTask;
        }

        public Task OnErrorOccurredAsync(WebRconError error, CancellationToken cancellationToken = default)
        {
            _errorCount++;
            if (_expectedErrorCount == 0 || _errorCount >= _expectedErrorCount)
            {
                ErrorReceived.TrySetResult(error);
            }

            return Task.CompletedTask;
        }
    }
}
