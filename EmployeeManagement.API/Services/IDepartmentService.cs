using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Services;

// Interface định nghĩa các nghiệp vụ (business logic) cho Department
public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();         // Lấy tất cả phòng ban
    Task<DepartmentDto?> GetByIdAsync(int id);       // Lấy 1 phòng ban theo Id
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);  // Tạo mới
    Task UpdateAsync(UpdateDepartmentDto dto);       // Cập nhật
    Task DeleteAsync(int id);                        // Xóa
}
