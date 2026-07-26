using AutoMapper;
using Moq;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;
using EmployeeManagement.API.Services;

namespace EmployeeManagement.API.Tests;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _repoMock;
    private readonly IMapper _mapper;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _repoMock = new Mock<IDepartmentRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _service = new DepartmentService(_repoMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDepartments()
    {
        var departments = new List<Department>
        {
            new() { Id = 1, Name = "IT" },
            new() { Id = 2, Name = "HR" },
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(departments);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("IT", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDepartment()
    {
        var department = new Department { Id = 1, Name = "IT" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(department);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("IT", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Department?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsDepartment()
    {
        var dto = new CreateDepartmentDto { Name = "Finance" };
        var department = new Department { Id = 3, Name = "Finance" };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Department>())).ReturnsAsync(department);

        var result = await _service.CreateAsync(dto);

        Assert.Equal("Finance", result.Name);
        _repoMock.Verify(r => r.CreateAsync(It.Is<Department>(d => d.Name == "Finance")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDepartment()
    {
        var dto = new UpdateDepartmentDto { Id = 1, Name = "Updated" };

        await _service.UpdateAsync(dto);

        _repoMock.Verify(r => r.UpdateAsync(It.Is<Department>(d => d.Id == 1 && d.Name == "Updated")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeletesDepartment()
    {
        await _service.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}
