using BulletinBoardAPI.Models.Requests;
using BulletinBoardAPI.Models.Responses;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoardAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public ActionResult<AuthResponse> Register([FromBody] RegisterRequest request)
    {
        var user = _userService.Register(request.Email, request.FirstName, request.LastName, request.Password);
        if (user == null)
        {
            return BadRequest("Email already registered.");
        }

        return Ok(new AuthResponse
        {
            Token = _tokenService.GenerateToken(user),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
    {
        var user = _userService.Authenticate(request.Email, request.Password);
        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new AuthResponse
        {
            Token = _tokenService.GenerateToken(user),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }
}
