using Microsoft.AspNetCore.Mvc;
using quiz.Domain.ViewModels;
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
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.password))
                return BadRequest("Email and password are required.");

            // Hash the user's entered password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.password);

            // Log the hashed password to the console
            Console.WriteLine($"Hashed Password: {hashedPassword}");


            bool result = await _loginService.ValidateUserAsync(request.Email, request.password);

            // return result ? Ok("Login successful") : Unauthorized("Invalid email or password");

            if (!result)
                return Unauthorized("Invalid email or password");

            string token = await _loginService.GenerateToken(request);

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
    public async Task<IActionResult> Register([FromBody] RegistrationViewModel request)
    {
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


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
}
