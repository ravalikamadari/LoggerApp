using Hangfire;
using LogAppServer.Services;
using LoggerApp.DTOs;
using LoggerApp.Models;
using LoggerApp.Repositories;
using LoggerApp.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LoggerApp.Controllers;
[ApiController]
[Route("api/log")]
[Authorize]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;
    private readonly ILogRepository _log;
    private readonly ITagRepository _tag;
    private readonly IEmailService _email;

    public LogController(ILogger<LogController> logger, ILogRepository log, ITagRepository tag, IEmailService email)
    {
        _logger = logger;
        _log = log;
        _tag = tag;
        _email = email;
    }

    [HttpGet]
    public async Task<ActionResult<List<Log>>> GetAllLogs([FromQuery] PaginationDTO pagination, [FromQuery] int? tag = null, [FromQuery] string title = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        List<Log> allLog = new List<Log>();
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        var userId = User.Claims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value;
        if (isSuperuser.Trim().ToLower() == "true")
            allLog = await _log.GetAllLogs(pagination, Int32.Parse(userId), tag, title, from, to);
        else
            allLog = await _log.GetAllLogsForUser(Int32.Parse(userId), pagination, tag, title, from, to);
        return Ok(allLog.Select(x => x.asDto));
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<LogDTO>> GetSingleLog(int id)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value;
        // var currentUserId = int.Parse(userId);
        var log = await _log.GetLogById(id);
        var dto = log.asDto;
        // dto.Tags = (await _tag.GetAllForLog(id)).Select(x => x.asDto).ToList();
        return Ok(dto);
    }
    [HttpPost]
    public async Task<ActionResult<Log>> CreateLog([FromBody] LogCreateDTO data)
    {
        var toCreateLog = new Log
        {
            Title = data.Title,
            Description = data.Description,
            StackTrace = data.StackTrace,
        };
        var createdLog = await _log.Create(toCreateLog);
        if (createdLog is not null)
        {
            await _log.SendPushNotification();
            _email.Send("ravalikamadari@gmail.com", "You got new log", $"<p>{data.Title}</p>", "ravalikamadari@gmail.com");
        }
        return StatusCode(StatusCodes.Status201Created, "Log Created");
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateLog(long id, [FromBody] LogUpdateDTO log)
    {
        var existing = await _log.GetLogById(id);
        var currentUserId = GetCurrentUserId();
        if (existing == null)
            return NotFound("Log not found");
        var toUpdateLog = existing with
        {
            Description = log.Description ?? existing.Description,
            UpdatedByUserId = currentUserId,
        };
        var didUpdate = await _log.UpdateLog(toUpdateLog, log.Tag, log.Type);
        if (!didUpdate)
            return StatusCode(StatusCodes.Status500InternalServerError, "Could not update Log");
        return Ok("Updated");
    }

    [HttpPut("seen")]
    public async Task<ActionResult> UpdateSeen([FromBody] UpdateLogSeenDTO data)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value;
        if (data.IsSeen)
            foreach (var id in data.LogIds)
            {
                var seen = (await _log.GetSeen(id, Int32.Parse(userId)));
                if (seen)
                    await _log.SetSeen(id, Int32.Parse(userId));
            }
        else
            foreach (var id in data.LogIds)
                await _log.UnsetSeen(id, Int32.Parse(userId));
        return NoContent();
    }



    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] long id)
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        if (isSuperuser.Trim().ToLower() != "true")
            return Unauthorized("You are not authorized to delete");
        var existing = await _log.GetLogById(id);
        if (existing is null)
            return NotFound("No log found with given id");
        var didDelete = _log.Delete(id);
        return NoContent();
    }
    private long GetCurrentUserId()
    {
        var userClaims = User.Claims;
        return Int64.Parse(userClaims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value);
    }

    [HttpGet("delete")]
    public void DeleteLogPermanently()
    {
        RecurringJob.AddOrUpdate(
        "myrecurringjob",
        () => _log.DeleteLogPermanently(),
        Cron.Minutely);
    }

}
