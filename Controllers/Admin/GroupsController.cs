using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOnMap.API.Models.Auth;
using PayOnMap.API.Services.Interfaces;

namespace PayOnMap.API.Controllers.Admin;

[ApiController]
[Route("api/admin/groups")]
[Authorize(Policy = "group:manage")]
[Produces("application/json")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _groupService.GetAllGroupsAsync();
        return Ok(new { success = true, data = groups });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        if (group == null)
            return NotFound(new { success = false, message = "گروه یافت نشد" });

        return Ok(new { success = true, data = group });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
    {
        var group = await _groupService.CreateGroupAsync(dto);
        return Ok(new { success = true, data = group, message = "گروه با موفقیت ایجاد شد" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupDto dto)
    {
        var result = await _groupService.UpdateGroupAsync(id, dto);
        if (!result)
            return NotFound(new { success = false, message = "گروه یافت نشد" });

        return Ok(new { success = true, message = "گروه با موفقیت به‌روزرسانی شد" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _groupService.DeleteGroupAsync(id);
        if (!result)
            return NotFound(new { success = false, message = "گروه یافت نشد" });

        return Ok(new { success = true, message = "گروه با موفقیت حذف شد" });
    }

    [HttpGet("{groupId}/members")]
    public async Task<IActionResult> GetMembers(Guid groupId)
    {
        var members = await _groupService.GetGroupMembersAsync(groupId);
        return Ok(new { success = true, data = members });
    }

    [HttpPost("{groupId}/members")]
    public async Task<IActionResult> AssignUser(Guid groupId, [FromBody] AssignUserToGroupDto dto)
    {
        var result = await _groupService.AssignUserToGroupAsync(groupId, dto.UserId);
        if (!result)
            return BadRequest(new { success = false, message = "کاربر قبلاً در این گروه عضو است" });

        return Ok(new { success = true, message = "کاربر با موفقیت به گروه اضافه شد" });
    }

    [HttpDelete("{groupId}/members/{userId}")]
    public async Task<IActionResult> RemoveUser(Guid groupId, Guid userId)
    {
        var result = await _groupService.RemoveUserFromGroupAsync(groupId, userId);
        if (!result)
            return NotFound(new { success = false, message = "کاربر در این گروه یافت نشد" });

        return Ok(new { success = true, message = "کاربر از گروه حذف شد" });
    }

    [HttpPost("{groupId}/roles/{roleId}")]
    public async Task<IActionResult> AssignRole(Guid groupId, Guid roleId)
    {
        var result = await _groupService.AssignRoleToGroupAsync(groupId, roleId);
        if (!result)
            return BadRequest(new { success = false, message = "نقش قبلاً به این گروه اختصاص دارد" });

        return Ok(new { success = true, message = "نقش با موفقیت به گروه اختصاص یافت" });
    }

    [HttpDelete("{groupId}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRole(Guid groupId, Guid roleId)
    {
        var result = await _groupService.RemoveRoleFromGroupAsync(groupId, roleId);
        if (!result)
            return NotFound(new { success = false, message = "نقش در این گروه یافت نشد" });

        return Ok(new { success = true, message = "نقش از گروه حذف شد" });
    }

    [HttpGet("{groupId}/visible-categories")]
    public async Task<IActionResult> GetVisibleCategories(Guid groupId)
    {
        var categoryIds = await _groupService.GetGroupVisibleCategoriesAsync(groupId);
        return Ok(new { success = true, data = categoryIds });
    }

    [HttpPost("{groupId}/visible-categories/{categoryId}")]
    public async Task<IActionResult> AddVisibleCategory(Guid groupId, Guid categoryId)
    {
        var result = await _groupService.AddCategoryToGroupAsync(groupId, categoryId);
        if (!result)
            return BadRequest(new { success = false, message = "این دسته‌بندی قبلاً اضافه شده" });

        return Ok(new { success = true, message = "دسته‌بندی با موفقیت به گروه اضافه شد" });
    }

    [HttpDelete("{groupId}/visible-categories/{categoryId}")]
    public async Task<IActionResult> RemoveVisibleCategory(Guid groupId, Guid categoryId)
    {
        var result = await _groupService.RemoveCategoryFromGroupAsync(groupId, categoryId);
        if (!result)
            return NotFound(new { success = false, message = "دسته‌بندی در این گروه یافت نشد" });

        return Ok(new { success = true, message = "دسته‌بندی از گروه حذف شد" });
    }
}
