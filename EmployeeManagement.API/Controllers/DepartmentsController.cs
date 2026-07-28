using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;

namespace EmployeeManagement.API.Controllers;

// Controller xử lý các request API cho Department
// Route: api/departments
[Authorize]           // Yêu cầu xác thực JWT
[ApiController]       // Tự động validation model, binding, ...
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    // GET /api/departments - Lấy danh sách tất cả phòng ban
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        var departments = await _service.GetAllAsync();
        return Ok(departments);
    }

    // GET /api/departments/{id} - Lấy 1 phòng ban theo Id
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _service.GetByIdAsync(id);

        if (department == null)
            return NotFound();

        return Ok(department);
    }

    // POST /api/departments - Tạo mới phòng ban
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(CreateDepartmentDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/departments/{id} - Cập nhật phòng ban
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateDepartmentDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _service.UpdateAsync(dto);
        return NoContent();
    }

    // DELETE /api/departments/{id} - Xóa phòng ban
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
