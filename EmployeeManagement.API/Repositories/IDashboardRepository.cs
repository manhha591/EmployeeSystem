using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Repositories;

public interface IDashboardRepository
{
    Task<DashboardDto> GetStatsAsync();
}
