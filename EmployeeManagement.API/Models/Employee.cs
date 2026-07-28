using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

// Model Nhân viên - ánh xạ tới bảng Employees trong DB
public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }     // Có thể null

    [Required]
    [Range(0, double.MaxValue)]            // Lương >= 0
    public decimal Salary { get; set; }

    public string? Avatar { get; set; }    // Đường dẫn file ảnh đại diện

    // Foreign key tới bảng Departments
    public int DepartmentId { get; set; }

    // Navigation property: Employee thuộc 1 Department
    public Department? Department { get; set; }

    // Ngày tạo, tự động gán UTC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}