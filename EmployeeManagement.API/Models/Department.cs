using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

// Model Phòng ban - ánh xạ tới bảng Departments trong DB
public class Department
{
    public int Id { get; set; }

    [Required]          // NOT NULL
    [MaxLength(100)]    // Tối đa 100 ký tự
    public string Name { get; set; } = string.Empty;

    // 1 phòng ban có nhiều nhân viên (navigation property)
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}