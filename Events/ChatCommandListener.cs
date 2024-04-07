using BattleBitAPI.Common;
using BattleBitApi.Api;
using BattleBitApi.Handlers;

namespace BattleBitApi.Events;

public class ChatCommandListener : Event
{
    public override async Task<bool> OnPlayerTypedMessage(BattleBitApiPlayer player, ChatChannel channel, string msg)
    {
        var returnValue = await ChatCommandHandler.Run(msg, player);
        return returnValue;
    }
}