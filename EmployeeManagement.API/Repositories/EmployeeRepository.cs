using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả nhân viên (kèm thông tin phòng ban)
    public async Task<List<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();
    }

    // Lấy danh sách nhân viên phân trang kèm tìm kiếm, lọc theo phòng ban, sắp xếp
    public async Task<PagedResult<Employee>> GetPagedAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .AsQueryable();

        // Lọc theo từ khoá tìm kiếm (tên, email, số điện thoại)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s) ||
                (e.Phone != null && e.Phone.Contains(s)));
        }

        // Lọc theo phòng ban
        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

        // Sắp xếp theo tên, lương hoặc email (tăng/giảm dần)
        query = (filter.SortBy?.ToLower()) switch
        {
            "name" => filter.SortDir == "desc"
                ? query.OrderByDescending(e => e.FullName)
                : query.OrderBy(e => e.FullName),
            "salary" => filter.SortDir == "desc"
                ? query.OrderByDescending(e => e.Salary)
                : query.OrderBy(e => e.Salary),
            "email" => filter.SortDir == "desc"
                ? query.OrderByDescending(e => e.Email)
                : query.OrderBy(e => e.Email),
            _ => query.OrderBy(e => e.Id)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Employee>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // Lấy một nhân viên theo ID (kèm thông tin phòng ban)
    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    // Thêm nhân viên mới vào DB, sau đó truy vấn lại để lấy đầy đủ thông tin (kèm phòng ban)
    public async Task<Employee> CreateAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return await _context.Employees
            .Include(e => e.Department)
            .FirstAsync(e => e.Id == employee.Id);
    }

    // Cập nhật thông tin nhân viên (đánh dấu entity đã thay đổi và lưu)
    public async Task UpdateAsync(Employee employee)
    {
        _context.Entry(employee).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    // Xoá nhân viên theo ID (tìm trước, nếu tồn tại thì xoá)
    public async Task DeleteAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }

    // Kiểm tra nhân viên có tồn tại theo ID không
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Employees.AnyAsync(e => e.Id == id);
    }

    // Kiểm tra phòng ban có tồn tại theo ID không
    public async Task<bool> DepartmentExistsAsync(int departmentId)
    {
        return await _context.Departments.AnyAsync(d => d.Id == departmentId);
    }
}
