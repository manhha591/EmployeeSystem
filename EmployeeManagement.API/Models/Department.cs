using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public class Department
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}