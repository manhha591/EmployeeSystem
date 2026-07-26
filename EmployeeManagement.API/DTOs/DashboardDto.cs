namespace EmployeeManagement.API.DTOs;

public class DashboardDto
{
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public decimal TotalSalary { get; set; }
    public List<DepartmentStatDto> EmployeesByDepartment { get; set; } = new();
}

public class DepartmentStatDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalSalary { get; set; }
}
