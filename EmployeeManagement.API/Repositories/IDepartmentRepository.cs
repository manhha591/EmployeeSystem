using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
