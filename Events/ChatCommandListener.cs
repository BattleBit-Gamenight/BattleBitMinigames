using BattleBitAPI.Common;
using BattleBitMinigames.Api;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Events;

public class ChatCommandListener : Event
{
    public override async Task<bool> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
    {
        var returnValue = await ChatCommandHandler.Run(msg, player);
        return returnValue;
    }
}