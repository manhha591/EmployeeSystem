using AutoMapper;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;
    private readonly IMapper _mapper;

    public DepartmentService(IDepartmentRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _repo.GetAllAsync();
        return _mapper.Map<List<DepartmentDto>>(departments);
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _repo.GetByIdAsync(id);
        return department == null ? null : _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        var created = await _repo.CreateAsync(department);
        return _mapper.Map<DepartmentDto>(created);
    }

    public async Task UpdateAsync(UpdateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        await _repo.UpdateAsync(department);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }
}
