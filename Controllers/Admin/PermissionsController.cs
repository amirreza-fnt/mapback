using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOnMap.API.Services.Interfaces;

namespace PayOnMap.API.Controllers.Admin;

[ApiController]
[Route("api/admin/permissions")]
[Authorize(Policy = "group:manage")]
[Produces("application/json")]
public class PermissionsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public PermissionsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _groupService.GetAllPermissionsAsync();
        return Ok(new { success = true, data = permissions });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var permissions = await _groupService.GetUserPermissionsAsync(userId);
        return Ok(new { success = true, data = permissions.ToList() });
    }
}
