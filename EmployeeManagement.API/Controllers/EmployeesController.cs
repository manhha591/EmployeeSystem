using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;

namespace EmployeeManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    // GET: api/employees — Lấy danh sách nhân viên có phân trang, tìm kiếm, lọc theo phòng ban, sắp xếp
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetAll(
        [FromQuery] EmployeeFilterDto filter)
    {
        var result = await _service.GetPagedAsync(filter);
        return Ok(result);
    }

    // GET: api/employees/{id} — Lấy thông tin một nhân viên theo ID
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _service.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    // POST: api/employees — Tạo mới một nhân viên
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            // Trả về 201 Created kèm đường dẫn đến resource mới
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/employees/{id} — Cập nhật thông tin nhân viên
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        try
        {
            await _service.UpdateAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/employees/{id} — Xoá một nhân viên theo ID
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }

    // POST: api/employees/{id}/avatar — Upload ảnh đại diện cho nhân viên
    [HttpPost("{id}/avatar")]
    public async Task<ActionResult> UploadAvatar(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var avatarUrl = await _service.UploadAvatarAsync(id, file);
        if (avatarUrl == null)
            return NotFound();

        return Ok(new { avatarUrl });
    }
}
