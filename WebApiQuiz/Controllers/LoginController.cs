using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.Dto;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

public class LoginController : ControllerBase
{

    private readonly ILoginService _loginService;

    public LoginController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _loginService.ValidateLoginAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            string token = await _loginService.GenerateToken(request);
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return Ok(new
            {
                Token = token,
                Message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Login method: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null.");

            string result = await _loginService.RegisterUserAsync(request);

            return result switch
            {
                "User already exists with this email." => Conflict(result),
                _ => Ok(new { Message = "Registration successful" })
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Register method: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("User")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            string? token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                token = Request.Cookies["jwtToken"];
            }

            if (string.IsNullOrEmpty(token))
                return Unauthorized("Token not found.");

            var userProfile = await _loginService.GetCurrentUserProfileAsync(token);

            if (userProfile == null)
                return NotFound("User not found.");

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GetCurrentUser: " + ex.Message);
            return StatusCode(500, "Internal server error.");
        }
    }
}
