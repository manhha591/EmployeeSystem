using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace EmployeeManagement.API.Services;

// Triển khai cache qua IDistributedCache (Redis hoặc MemoryCache)
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    // Đọc cache: không có key hoặc Redis lỗi → trả về default (null)
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var bytes = await _cache.GetAsync(key);
            if (bytes == null)
                return default;
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch
        {
            // Redis không khả dụng → bỏ qua cache, đọc từ DB
            return default;
        }
    }

    // Ghi cache với thời gian sống TTL; lỗi thì nuốt để không phá request
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
        }
        catch
        {
            // Nuốt lỗi: cache không phải điểm chết của ứng dụng
        }
    }

    // Xóa cache khi dữ liệu thay đổi (invalidate)
    public async Task RemoveAsync(params string[] keys)
    {
        try
        {
            foreach (var key in keys)
                await _cache.RemoveAsync(key);
        }
        catch
        {
            // Nuốt lỗi
        }
    }
}
