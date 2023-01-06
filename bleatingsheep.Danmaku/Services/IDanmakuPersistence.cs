namespace bleatingsheep.Danmaku.Services;

public interface IDanmakuPersistence
{
    ValueTask<IEnumerable<DanmakuEntry>> GetDanmakuSinceAsync(string group, DateTimeOffset start);
    ValueTask SaveDanmakuAsync(string group, string user, DanmakuData data);
}