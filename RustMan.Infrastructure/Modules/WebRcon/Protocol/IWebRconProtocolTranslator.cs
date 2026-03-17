using RustMan.Core.Modules.WebRcon.Models;

namespace RustMan.Infrastructure.Modules.WebRcon.Protocol;

public interface IWebRconProtocolTranslator
{
    string SerializeCommand(WebRconCommandRequest command);

    WebRconInboundMessage DeserializeInboundMessage(string rawMessage);
}
