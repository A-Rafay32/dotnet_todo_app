using Microsoft.AspNetCore.Mvc;
using TodoApp.Data;
using TodoApp.DTOs;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] LoginRequestDTO dto)
    {
        if (_db.Users.Any(u => u.Username == dto.Username))
            return BadRequest("User already exists.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return Ok(new
        {
            message = "User created successfully",
            user = new UserDTO
            {
                Id = user.Id,   
                Username = user.Username
            }
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDTO login)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == login.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password" });

        var token = _jwt.GenerateToken(user);

        return Ok(new  {
            token,
            user = new UserDTO
            {
                Id = user.Id,   
                Username = user.Username
            }
        });
    }
}
