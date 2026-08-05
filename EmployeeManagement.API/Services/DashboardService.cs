using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services;

public class DashboardService : IDashboardService
{
    // Key cache cho thống kê dashboard (public để Service khác invalidate khi dữ liệu thay đổi)
    public const string DashboardCacheKey = "cache:dashboard:stats";

    private readonly IDashboardRepository _repo;
    private readonly ICacheService _cache;

    public DashboardService(IDashboardRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    // Thống kê: đọc từ cache nếu có, không có thì tính toán từ DB rồi lưu cache 60 giây
    public async Task<DashboardDto> GetStatsAsync()
    {
        var cached = await _cache.GetAsync<DashboardDto>(DashboardCacheKey);
        if (cached != null)
            return cached;

        var stats = await _repo.GetStatsAsync();
        await _cache.SetAsync(DashboardCacheKey, stats, TimeSpan.FromSeconds(60));
        return stats;
    }
}
