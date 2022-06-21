using LoggerApp.DTOs;
using LoggerApp.Models;
using LoggerApp.Repositories;
using LoggerApp.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LoggerApp.Controllers;
[ApiController]
[Route("api/tag")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly ILogger<TagController> _logger;
    private readonly ITagRepository _tag;
    private readonly IUserRepository _user;
    private readonly ILogRepository _log;

    public TagController(ILogger<TagController> logger, ITagRepository tag, IUserRepository user, ILogRepository log)
    {
        _logger = logger;
        _tag = tag;
        _user = user;
        _log = log;
    }
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateaTag([FromBody] TagCreateDTO data)
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        if (isSuperuser.Trim().ToLower() != "true")
            return Unauthorized("You are not authorized to create Tags");
        var toCreateTag = new Tag
        {
            Name = data.Name,
            TypeId = data.TypeId,
        };
        var createdTag = await _tag.Create(toCreateTag);
        return StatusCode(StatusCodes.Status201Created, "Tag Created");
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDTO>> GetSingleTag(int id)
    {
        var tag = await _tag.GetTagById(id);
        var dto = tag.asDto;
        dto.Logs = (await _log.GetAllForTag(id)).Select(x => x.asDto).ToList();
        return Ok(dto);
    }
    [HttpGet]
    public async Task<ActionResult<List<Tag>>> GetAllTags()
    {
        var allTags = await _tag.GetAllTags();
        return Ok(allTags.Select(x => x.asDto));
    }
    [HttpGet("types")]
    public async Task<ActionResult<List<TagType>>> GetAllTagTypes()
    {
        var allTagTypes = await _tag.GetAllTagType();
        return Ok(allTagTypes);
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag([FromRoute] long id)
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        if (isSuperuser.Trim().ToLower() != "true")
            return Unauthorized("You are not authorized to delete Tags");
        var didDelete = await _tag.Delete(id);
        return NoContent();
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] TagUpdateDTO data)
    {
        var existing = await _tag.GetTagById(id);
        if (existing is null)
            return NotFound("Tag does not exist");
        if (!(await _tag.UpdateTag(id, data.TypeId)))
            return BadRequest("Could Not Update");
        return NoContent();
    }
    private bool IsValidEmailAddress(string email)
    {
        try
        {
            var emailChecked = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
    private long GetCurrentUserId()
    {
        var userClaims = User.Claims;
        return Int64.Parse(userClaims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value);
    }
}