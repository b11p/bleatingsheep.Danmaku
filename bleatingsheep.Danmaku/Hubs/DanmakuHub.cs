using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace bleatingsheep.Danmaku.Hubs;

public class DanmakuHub : Hub
{
    private readonly ILogger<DanmakuHub> _logger;

    public DanmakuHub(ILogger<DanmakuHub> logger)
    {
        _logger = logger;
    }

    public async Task SendMessage(string group, string user, string message)
    {
        _logger.LogInformation("{} ID {} sent: {}", user, Context.ConnectionId, message);
        if (string.IsNullOrWhiteSpace(user))
        {
            var remoteAddress = Context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress;
            user = remoteAddress?.ToString() ?? "Anonymous";
        }
        await Clients.OthersInGroup(group).SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinGroup(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();
        _logger.LogInformation("{} from {} joined group {}", Context.ConnectionId, httpConnectionFeature?.RemoteIpAddress, group);
    }

    public Task Connection(string group)
    {
        return JoinGroup(group);
    }
}