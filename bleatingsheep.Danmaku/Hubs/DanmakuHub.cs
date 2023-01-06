using System.Text.Json;
using bleatingsheep.Danmaku.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace bleatingsheep.Danmaku.Hubs;

public class DanmakuHub : Hub
{
    private readonly ILogger<DanmakuHub> _logger;
    private readonly IDanmakuPersistence _danmakuPersistence;

    public DanmakuHub(ILogger<DanmakuHub> logger, IDanmakuPersistence danmakuPersistence)
    {
        _logger = logger;
        _danmakuPersistence = danmakuPersistence;
    }

    public async Task SendMessage(string group, string user, string message)
    {
        _logger.LogInformation("{} ID {} sent: {}", user, Context.ConnectionId, message);
        if (string.IsNullOrWhiteSpace(user))
        {
            var remoteAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress;
            user = remoteAddress?.ToString() ?? "Anonymous";
        }
        DanmakuData data;
        try
        {
            var deserializedData = JsonSerializer.Deserialize<DanmakuData>(message);
            if (deserializedData is null)
            {
                return;
            }
            data = deserializedData;
        }
        catch
        {
            _logger.LogInformation("{} ID {} sent an invalid message (deserialization error)", user, Context.ConnectionId);
            return;
        }
        await _danmakuPersistence.SaveDanmakuAsync(group, user, data).ConfigureAwait(false);
        await Clients.OthersInGroup(group).SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinGroup(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();
        _logger.LogInformation("{} from {} joined group {}", Context.ConnectionId, httpConnectionFeature?.RemoteIpAddress, group);
    }

    public async ValueTask<IEnumerable<DanmakuEntry>> GetRecentDanmaku(string group, DateTimeOffset fetchDanmakuSince = default)
    {
        var earliestAllowFetchTime = DateTimeOffset.UtcNow.AddDays(-1);
        if (fetchDanmakuSince < earliestAllowFetchTime)
        {
            fetchDanmakuSince = earliestAllowFetchTime;
        }
        var danmakuList = await _danmakuPersistence.GetDanmakuSinceAsync(group, fetchDanmakuSince).ConfigureAwait(false);
        await Clients.Caller.SendAsync("DanmakuHistory", danmakuList).ConfigureAwait(false);
        return danmakuList;
    }

    public Task Connection(string group)
    {
        return JoinGroup(group);
    }
}