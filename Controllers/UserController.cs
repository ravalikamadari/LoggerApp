using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LoggerApp.DTOs;
using LoggerApp.Models;
using LoggerApp.Repositories;
using LoggerApp.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace LoggerApp.Controllers;
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _user;
    private readonly IUserLoginRepository _userLogin;
    private readonly ITagRepository _tag;

    private IConfiguration _configuration;
    public UserController(ILogger<UserController> logger, IUserRepository user, IConfiguration configuration, ITagRepository tag, IUserLoginRepository userLogin)
    {
        _logger = logger;
        _user = user;
        _configuration = configuration;
        _tag = tag;
        _userLogin = userLogin;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] UserLoginDTO userLogin)
    {
        var user = await _user.GetByEmail(userLogin.Email);
        if (user.Status == Status.deactive)
            return BadRequest("User is deactivated");

        if (!IsValidEmailAddress(userLogin.Email))
            return BadRequest("Invalid email address");

        if (user == null)
            return NotFound("User not found");

        if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.HashedPassword))
            return Unauthorized("Invalid password");


        var token = Generate(user);
        if(token is not null)
             await _userLogin.SetLogin(user.Id, userLogin.DeviceId, userLogin.NotificationToken, Request.Headers["User-Agent"]);

        var res = new UserLoginResponseDTO
        {
            Token = token,
            Email = user.Email,
            UserId = user.Id,
            IsSuperuser = user.IsSuperUser,
            Name = user.Name,
        };
        return Ok(res);
    }
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<UserDTO>>> GetAllUsers()
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        if (isSuperuser.Trim().ToLower() != "true")
            return BadRequest("You don't have access to this endpoint");
        var allUsers = await _user.GetAllUsers();
        return Ok(allUsers.Select(x => x.asDto));
    }
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetSingleUser(int id)
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        var userId = User.Claims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value;
        if (isSuperuser.Trim().ToLower() == "true" || userId == id.ToString())
        {
            var user = (await _user.GetById(id));
            var dto = user.asDto;
            dto.Tags = (await _tag.GetAllForUser(id)).Select(x => x).ToList();
            return Ok(dto);
        }
        else
            return BadRequest("You don't have access to this endpoint");
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<User>> CreateUser([FromBody] UserCreateDTO data)
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        if (isSuperuser.Trim().ToLower() != "true")
            return Unauthorized("You are not authorized to create users");
        if (!IsValidEmailAddress(data.Email))
            return BadRequest("Invalid email address");
        var toCreateUser = new User
        {
            Name = data.Name,
            Email = data.Email.Trim().ToLower(),
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(data.Password),
            IsSuperUser = data.IsSuperUser
        };
        var createdUser = await _user.Create(toCreateUser);
        return StatusCode(StatusCodes.Status201Created, "User Created");
    }
    [HttpPut]
    [Authorize]
    public async Task<ActionResult> UpdateUser([FromBody] UserUpdateDTO data)
    {
        var isSuperuser = User.Claims.FirstOrDefault(c => c.Type == UserConstants.IsSuperuser)?.Value;
        if (isSuperuser.Trim().ToLower() != "true")
            return Unauthorized("You are not authorized to update users");
        if (await _user.UpdateUser(data))
            return NoContent();
        else
            return BadRequest("Error in update user");
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
    private string Generate(User user)
    {
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(UserConstants.Id, user.Id.ToString()),
            new Claim(UserConstants.Name, user.Name),
            new Claim(UserConstants.Email, user.Email),
            new Claim(UserConstants.IsSuperuser, user.IsSuperUser.ToString()),
        };

        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private long GetCurrentUserId()
    {
        var userClaims = User.Claims;
        return Int64.Parse(userClaims.FirstOrDefault(c => c.Type == UserConstants.Id)?.Value);
    }
}