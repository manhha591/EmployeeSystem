using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeManagement.API.Models;

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
    public string? Phone { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    public string? Avatar { get; set; }

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}