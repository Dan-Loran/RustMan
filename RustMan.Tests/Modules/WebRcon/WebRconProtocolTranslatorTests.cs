using System.Text.Json;
using RustMan.Core.Modules.WebRcon.Models;
using RustMan.Infrastructure.Modules.WebRcon.Protocol;

namespace RustMan.Tests.Modules.WebRcon;

public sealed class WebRconProtocolTranslatorTests
{
    private readonly WebRconProtocolTranslator _translator = new();

    [Fact]
    public void SerializeCommand_ProducesExpectedJsonShape()
    {
        var command = new WebRconCommandRequest
        {
            Identifier = 1,
            Message = "status",
            Name = "RustMan.RconCapture"
        };

        var json = _translator.SerializeCommand(command);
        using var document = JsonDocument.Parse(json);

        Assert.Equal(1, document.RootElement.GetProperty("Identifier").GetInt32());
        Assert.Equal("status", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal("RustMan.RconCapture", document.RootElement.GetProperty("Name").GetString());
        Assert.Equal(3, document.RootElement.EnumerateObject().Count());
    }

    [Fact]
    public void DeserializeInboundMessage_MapsGenericPacketToTextPayload()
    {
        const string rawMessage = """
            {
              "Message": "hostname: Test Server",
              "Identifier": 1,
              "Type": "Generic",
              "Stacktrace": ""
            }
            """;

        var message = _translator.DeserializeInboundMessage(rawMessage);

        var payload = Assert.IsType<WebRconTextPayload>(message.Payload);
        Assert.Equal(1, message.Identifier);
        Assert.Equal("Generic", message.Type);
        Assert.Equal(string.Empty, message.Stacktrace);
        Assert.Equal("hostname: Test Server", payload.Text);
    }

    [Fact]
    public void DeserializeInboundMessage_MapsValidChatPacketToChatPayload()
    {
        const string rawMessage = """
            {
              "Message": "{ \"Channel\": 0, \"Message\": \"Hello\", \"UserId\": \"1\", \"Username\": \"Dan\", \"Color\": \"#5af\", \"Time\": 1773745049 }",
              "Identifier": -1,
              "Type": "Chat",
              "Stacktrace": null
            }
            """;

        var message = _translator.DeserializeInboundMessage(rawMessage);

        var payload = Assert.IsType<WebRconChatPayload>(message.Payload);
        Assert.Equal(-1, message.Identifier);
        Assert.Equal("Chat", message.Type);
        Assert.Null(message.Stacktrace);
        Assert.Equal(0, payload.Channel);
        Assert.Equal("Hello", payload.Message);
        Assert.Equal("1", payload.UserId);
        Assert.Equal("Dan", payload.Username);
        Assert.Equal("#5af", payload.Color);
        Assert.Equal(1773745049, payload.Time);
    }

    [Fact]
    public void DeserializeInboundMessage_FallsBackToTextPayloadForInvalidInnerChatJson()
    {
        const string rawMessage = """
            {
              "Message": "{ invalid json",
              "Identifier": -1,
              "Type": "Chat",
              "Stacktrace": null
            }
            """;

        var message = _translator.DeserializeInboundMessage(rawMessage);

        var payload = Assert.IsType<WebRconTextPayload>(message.Payload);
        Assert.Equal("{ invalid json", payload.Text);
    }

    [Fact]
    public void DeserializeInboundMessage_ThrowsForInvalidOuterJson()
    {
        Assert.Throws<JsonException>(() => _translator.DeserializeInboundMessage("{"));
    }
}
