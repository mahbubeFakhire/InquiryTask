using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RedisManager;

public static class CacheHelper
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
                                                  string recordId,
                                                  T data,
                                                  TimeSpan? absoluteExpireTime = null,
                                                  TimeSpan? slidingExpireTime = null,
                                                  bool usePrefix = true,
                                                  CancellationToken cancellationToken = default)
    {
        recordId = usePrefix ? AddPrefix(recordId) : recordId;
        DistributedCacheEntryOptions options = new();

        options.AbsoluteExpirationRelativeToNow = absoluteExpireTime;
        options.SlidingExpiration = slidingExpireTime;
        string jsonData = string.Empty;

        if (typeof(T) == typeof(string))
        {
            jsonData = data.ToString();
        }
        else
        {
            jsonData = JsonSerializer.Serialize(data);
        }
        await cache.SetStringAsync(recordId, jsonData, options, cancellationToken);
    }

    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache,
                                                   string recordId, bool usePrefix = true, CancellationToken cancellationToken = default)
    {
        recordId = usePrefix ? AddPrefix(recordId) : recordId;
        var jsonData = await cache.GetStringAsync(recordId, cancellationToken);

        if (jsonData is null)
        {
            return default(T);
        }
        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(jsonData, typeof(T)); ;
        }
        return JsonSerializer.Deserialize<T>(jsonData);
    }

    public static Task RemoveRecordAsync(this IDistributedCache cache,
                                                 string recordId,
                                                 bool usePrefix = true,
                                                 CancellationToken cancellationToken = default)
    {
        recordId = usePrefix ? AddPrefix(recordId) : recordId;
        return cache.RemoveAsync(recordId, cancellationToken);
    }

    private static string AddPrefix(string recordId)
    {
        return "inquiry--" + recordId;
    }

}
