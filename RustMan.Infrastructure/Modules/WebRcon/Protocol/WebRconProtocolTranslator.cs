using System.Text.Json;
using System.Text.Json.Serialization;
using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.WebRcon.Protocol;

public sealed class WebRconProtocolTranslator : IWebRconProtocolTranslator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public string SerializeCommand(WebRconCommandRequest command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var outboundPacket = new OutboundPacketDto
        {
            Identifier = command.Identifier,
            Message = BuildCommandMessage(command),
            Name = command.Name ?? throw new ArgumentException("Command name is required.", nameof(command))
        };

        return JsonSerializer.Serialize(outboundPacket, SerializerOptions);
    }

    private static string BuildCommandMessage(WebRconCommandRequest command)
    {
        if (string.IsNullOrEmpty(command.CommandText))
        {
            throw new ArgumentException("Command text is required.", nameof(command));
        }

        if (command.Parameters.Count == 0)
        {
            return command.CommandText;
        }

        return command.CommandText + " " + string.Join(" ", command.Parameters);
    }

    public WebRconInboundMessage DeserializeInboundMessage(string rawMessage)
    {
        ArgumentNullException.ThrowIfNull(rawMessage);

        var inboundPacket = JsonSerializer.Deserialize<InboundPacketDto>(rawMessage, SerializerOptions)
            ?? throw new JsonException("Failed to deserialize WebRcon inbound message.");

        if (inboundPacket.Message is null)
        {
            throw new JsonException("WebRcon inbound message is missing Message.");
        }

        if (inboundPacket.Type is null)
        {
            throw new JsonException("WebRcon inbound message is missing Type.");
        }

        return new WebRconInboundMessage
        {
            Identifier = inboundPacket.Identifier,
            Type = inboundPacket.Type,
            Stacktrace = inboundPacket.Stacktrace,
            Payload = TranslatePayload(inboundPacket)
        };
    }

    private static IWebRconPayload TranslatePayload(InboundPacketDto inboundPacket)
    {
        if (string.Equals(inboundPacket.Type, "Chat", StringComparison.Ordinal))
        {
            try
            {
                var chatPayload = JsonSerializer.Deserialize<ChatPayloadDto>(inboundPacket.Message, SerializerOptions);
                if (chatPayload is not null &&
                    chatPayload.Message is not null &&
                    chatPayload.UserId is not null &&
                    chatPayload.Username is not null)
                {
                    return new WebRconChatPayload
                    {
                        Channel = chatPayload.Channel,
                        Message = chatPayload.Message,
                        UserId = chatPayload.UserId,
                        Username = chatPayload.Username,
                        Color = chatPayload.Color,
                        Time = chatPayload.Time
                    };
                }
            }
            catch (JsonException)
            {
            }
        }

        return new WebRconTextPayload
        {
            Text = inboundPacket.Message
        };
    }

    private sealed class OutboundPacketDto
    {
        public int Identifier { get; init; }

        public required string Message { get; init; }

        public required string Name { get; init; }
    }

    private sealed class InboundPacketDto
    {
        public required string Message { get; init; }

        public int Identifier { get; init; }

        public required string Type { get; init; }

        public string? Stacktrace { get; init; }
    }

    private sealed class ChatPayloadDto
    {
        public int Channel { get; init; }

        public string? Message { get; init; }

        public string? UserId { get; init; }

        public string? Username { get; init; }

        public string? Color { get; init; }

        public long Time { get; init; }
    }
}
