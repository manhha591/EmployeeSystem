using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Data;

// DbContext - cầu nối giữa code C# và database PostgreSQL
// Mỗi DbSet tương ứng với 1 bảng trong DB
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}