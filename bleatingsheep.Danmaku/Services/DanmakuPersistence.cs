using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace bleatingsheep.Danmaku.Services;

internal class DanmakuPersistence : IDanmakuPersistence
{
    private readonly ILogger<DanmakuPersistence> _logger;
    private readonly IMemoryCache _memoryCache;

    public DanmakuPersistence(ILogger<DanmakuPersistence> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public ValueTask SaveDanmakuAsync(string group, string user, DanmakuData data)
    {
        var danmakuList = _memoryCache.GetOrCreate(group, e =>
        {
            e.SlidingExpiration = new TimeSpan(12, 0, 0);
            return new ConcurrentBag<DanmakuEntry>();
        });
        Debug.Assert(danmakuList != null);
        danmakuList.Add(new()
        {
            Data = data,
            TimeStamp = DateTime.UtcNow,
            User = user,
        });
        return ValueTask.CompletedTask;
    }

    public ValueTask<IEnumerable<DanmakuEntry>> GetDanmakuSinceAsync(string group, DateTimeOffset start)
    {
        _memoryCache.TryGetValue<ConcurrentBag<DanmakuEntry>>(group, out var danmakuList);
        if (danmakuList is null)
        {
            return ValueTask.FromResult(Enumerable.Empty<DanmakuEntry>());
        }
        return ValueTask.FromResult<IEnumerable<DanmakuEntry>>(danmakuList.Where(d => d.TimeStamp >= start).OrderBy(d => d.TimeStamp).ToList());
    }
}

public record class DanmakuData(
    [property: JsonPropertyName("text")] string Text,
    double Time = default)
{
    /// <summary>
    /// 当前连续播放时间，非发送弹幕的时间戳。
    /// </summary>
    [JsonPropertyName("time")]
    public double Time { get; init; } = Time;
}

public record class DanmakuEntry
{
    [JsonPropertyName("data")]
    public required DanmakuData Data { get; init; }
    [JsonPropertyName("time_stamp")]
    public required DateTimeOffset TimeStamp { get; init; }
    [JsonPropertyName("user")]
    public required string User { get; init; }
}