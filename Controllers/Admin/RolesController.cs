using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOnMap.API.Models.Auth;
using PayOnMap.API.Services.Interfaces;

namespace PayOnMap.API.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = "group:manage")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IGroupService groupService, ILogger<RolesController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _groupService.GetAllRolesAsync();
        return Ok(new { success = true, data = roles });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var role = await _groupService.CreateRoleAsync(dto);
        return Ok(new { success = true, data = role, message = "نقش با موفقیت ایجاد شد" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _groupService.UpdateRoleAsync(id, dto);
        if (!result)
            return NotFound(new { success = false, message = "نقش یافت نشد یا سیستمی است" });

        return Ok(new { success = true, message = "نقش با موفقیت به‌روزرسانی شد" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _groupService.DeleteRoleAsync(id);
        if (!result)
            return NotFound(new { success = false, message = "نقش یافت نشد یا سیستمی است" });

        return Ok(new { success = true, message = "نقش با موفقیت حذف شد" });
    }
}
