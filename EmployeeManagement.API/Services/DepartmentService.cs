using AutoMapper;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services;

// Service layer: chứa logic nghiệp vụ, gọi Repository để thao tác DB
// Được Inject IDepartmentRepository và IMapper qua constructor (DI)
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;
    private readonly IMapper _mapper;

    public DepartmentService(IDepartmentRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    // Lấy danh sách phòng ban, map từ Entity -> DTO trước khi trả về
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

    // Nhận DTO từ client, map sang Entity, lưu vào DB, trả về DTO kèm Id mới
    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        var created = await _repo.CreateAsync(department);
        return _mapper.Map<DepartmentDto>(created);
    }

    // Cập nhật: map DTO -> Entity, gửi xuống Repository
    public async Task UpdateAsync(UpdateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        await _repo.UpdateAsync(department);
    }

    // Xóa phòng ban theo Id
    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }
}
