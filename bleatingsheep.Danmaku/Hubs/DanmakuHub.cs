using Microsoft.AspNetCore.SignalR;

namespace bleatingsheep.Danmaku.Hubs;

public class DanmakuHub : Hub
{
    public async Task SendMessage(string group, string user, string message)
    {
        await Clients.OthersInGroup(group).SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinGroup(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    }
}