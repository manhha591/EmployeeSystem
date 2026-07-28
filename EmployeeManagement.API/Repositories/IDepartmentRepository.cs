using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Repositories;

// Interface định nghĩa các thao tác dữ liệu (data access) cho Department
public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();       // Lấy tất cả
    Task<Department?> GetByIdAsync(int id);     // Lấy theo Id
    Task<Department> CreateAsync(Department department);  // Thêm mới
    Task UpdateAsync(Department department);    // Cập nhật
    Task DeleteAsync(int id);                   // Xóa
    Task<bool> ExistsAsync(int id);             // Kiểm tra tồn tại
}
