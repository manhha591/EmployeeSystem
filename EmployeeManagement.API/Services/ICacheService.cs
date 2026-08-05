namespace EmployeeManagement.API.Services;

// Cache abstraction: giúp các Service đọc/ghi cache mà không cần biết
// đằng sau là Redis hay bộ nhớ (do Program.cs quyết định khi đăng ký)
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan ttl);
    Task RemoveAsync(params string[] keys);
}
