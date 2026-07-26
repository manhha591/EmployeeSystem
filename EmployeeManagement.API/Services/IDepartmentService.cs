using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task UpdateAsync(UpdateDepartmentDto dto);
    Task DeleteAsync(int id);
}
