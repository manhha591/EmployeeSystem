using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<PagedResult<EmployeeDto>> GetPagedAsync(EmployeeFilterDto filter);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task UpdateAsync(UpdateEmployeeDto dto);
    Task DeleteAsync(int id);
    Task<string?> UploadAvatarAsync(int id, IFormFile file);
}
