namespace EmployeeManagement.API.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateDepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
