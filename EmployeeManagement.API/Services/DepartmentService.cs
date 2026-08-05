using AutoMapper;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services;

// Service layer: chứa logic nghiệp vụ, gọi Repository để thao tác DB
// Được Inject IDepartmentRepository, IMapper và ICacheService qua constructor (DI)
public class DepartmentService : IDepartmentService
{
    // Key cache cho danh sách phòng ban
    private const string DepartmentsCacheKey = "cache:departments:all";

    private readonly IDepartmentRepository _repo;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public DepartmentService(IDepartmentRepository repo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
    }

    // Lấy danh sách phòng ban: ưu tiên đọc từ cache, không có thì đọc DB rồi lưu cache
    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<List<DepartmentDto>>(DepartmentsCacheKey);
        if (cached != null)
            return cached;

        var departments = await _repo.GetAllAsync();
        var result = _mapper.Map<List<DepartmentDto>>(departments);
        await _cache.SetAsync(DepartmentsCacheKey, result, TimeSpan.FromMinutes(5));
        return result;
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
        await _cache.RemoveAsync(DepartmentsCacheKey, DashboardService.DashboardCacheKey);
        return _mapper.Map<DepartmentDto>(created);
    }

    // Cập nhật: map DTO -> Entity, gửi xuống Repository
    public async Task UpdateAsync(UpdateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        await _repo.UpdateAsync(department);
        await _cache.RemoveAsync(DepartmentsCacheKey, DashboardService.DashboardCacheKey);
    }

    // Xóa phòng ban theo Id
    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        await _cache.RemoveAsync(DepartmentsCacheKey, DashboardService.DashboardCacheKey);
    }
}
