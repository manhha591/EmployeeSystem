using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;

    public DashboardService(IDashboardRepository repo)
    {
        _repo = repo;
    }

    public async Task<DashboardDto> GetStatsAsync()
    {
        return await _repo.GetStatsAsync();
    }
}
