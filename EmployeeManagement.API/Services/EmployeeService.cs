using AutoMapper;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public EmployeeService(IEmployeeRepository repo, IMapper mapper, IWebHostEnvironment env)
    {
        _repo = repo;
        _mapper = mapper;
        _env = env;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        var employees = await _repo.GetAllAsync();
        return _mapper.Map<List<EmployeeDto>>(employees);
    }

    public async Task<PagedResult<EmployeeDto>> GetPagedAsync(EmployeeFilterDto filter)
    {
        var paged = await _repo.GetPagedAsync(filter);
        return new PagedResult<EmployeeDto>
        {
            Items = _mapper.Map<List<EmployeeDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _repo.GetByIdAsync(id);
        return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        var departmentExists = await _repo.DepartmentExistsAsync(dto.DepartmentId);
        if (!departmentExists)
            throw new KeyNotFoundException("Department not found");

        var employee = _mapper.Map<Employee>(dto);
        var created = await _repo.CreateAsync(employee);
        return _mapper.Map<EmployeeDto>(created);
    }

    public async Task UpdateAsync(UpdateEmployeeDto dto)
    {
        var departmentExists = await _repo.DepartmentExistsAsync(dto.DepartmentId);
        if (!departmentExists)
            throw new KeyNotFoundException("Department not found");

        var employee = _mapper.Map<Employee>(dto);
        await _repo.UpdateAsync(employee);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<string?> UploadAvatarAsync(int id, IFormFile file)
    {
        var employee = await _repo.GetByIdAsync(id);
        if (employee == null)
            return null;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{id}_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        employee.Avatar = $"/uploads/avatars/{fileName}";
        await _repo.UpdateAsync(employee);

        return employee.Avatar;
    }
}
