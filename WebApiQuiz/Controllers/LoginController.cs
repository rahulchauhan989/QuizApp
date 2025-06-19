using Azure;
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
    public async Task<ActionResult<ResponseDto>> Login([FromBody] LoginModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _loginService.ValidateLoginAsync(request);
            if (!validationResult.IsValid)
            {
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);
            }

            string token = await _loginService.GenerateToken(request);
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return new ResponseDto(true, "Login successful", new { Token = token });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Login method: {ex.Message}");
            return new ResponseDto(false, "Internal server error", null, 500);
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ResponseDto>> Register([FromBody] RegistrationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            if (request == null)
                return new ResponseDto(false, "Invalid registration data", null, 400);

            string result = await _loginService.RegisterUserAsync(request);

            if (result == "User already exists")
            {
                return new ResponseDto(false, result, null, 400);
            }
            return new ResponseDto(true, result, null, 201);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Register method: {ex.Message}");
            return new ResponseDto(false, "Internal server error", null, 500);
        }
    }

    [HttpGet("User")]
    [Authorize]
    public async Task<ActionResult<ResponseDto>> GetCurrentUser()
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
                return new ResponseDto(false, "User not found", null, 404);

            return new ResponseDto(true, "User profile retrieved successfully", userProfile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GetCurrentUser: " + ex.Message);
            return new ResponseDto(false, "Internal server error", null, 500);
        }
    }
}
