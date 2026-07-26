using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetStatsAsync()
    {
        var totalEmployees = await _context.Employees.CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();
        var totalSalary = await _context.Employees.SumAsync(e => e.Salary);

        var employeesByDepartment = await _context.Departments
            .Select(d => new DepartmentStatDto
            {
                DepartmentName = d.Name,
                Count = d.Employees.Count,
                TotalSalary = d.Employees.Sum(e => e.Salary)
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalEmployees = totalEmployees,
            TotalDepartments = totalDepartments,
            TotalSalary = totalSalary,
            EmployeesByDepartment = employeesByDepartment
        };
    }
}
