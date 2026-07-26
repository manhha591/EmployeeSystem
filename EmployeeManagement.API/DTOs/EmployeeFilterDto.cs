namespace EmployeeManagement.API.DTOs;

public class EmployeeFilterDto
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "asc";
}
