using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class LoginService : ILoginService
{
    private readonly ILoginRepo _Loginrepository;

    private readonly IConfiguration _configuration;

    public LoginService(ILoginRepo loginrepository, IConfiguration configuration)
    {
        _Loginrepository = loginrepository;
        _configuration = configuration;
    }
    public async Task<bool> ValidateUserAsync(string email, string password)
    {

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return false; 
        }

        bool result = await _Loginrepository.ValidateUserAsync(email, password);
        return result;
    }

    public async Task<ValidationResult> ValidateLoginAsync(LoginModel request)
    {
        if (request == null)
        {
            return ValidationResult.Failure("Request cannot be null.");
        }
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.password))
        {
            return ValidationResult.Failure("Email and Password cannot be null or empty.");
        }

        bool isValidUser = await _Loginrepository.ValidateUserAsync(request.Email, request.password);

        if (!isValidUser)
        {
            return ValidationResult.Failure("Invalid email or password.");
        }

        return ValidationResult.Success();
    }

    public async Task<string> GenerateToken(LoginModel request)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);  
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
        }
        User user = await _Loginrepository.GetUserByEmailAsync(request.Email);

        Console.WriteLine($"User Role is: {user.Role}");

        var claims = new[]    
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role?.ToString() ?? "Unknown")
    };

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: request.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);   

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(tokenString);

        Console.WriteLine(" Decoded Token Claims:");
        foreach (var claim in decodedToken.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
        }
        return tokenString;
    }



    public async Task<String> RegisterUserAsync(RegistrationDto request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.password))
        {
            return "Email and Password cannot be null or empty.";
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.password);

        User newUser = new User
        {
            Fullname = request.Name!,
            Email = request.Email,
            Passwordhash = hashedPassword,
            Role = "User" 
        };

        return await _Loginrepository.RegisterUserAsync(newUser);
    }

    public int ExtractUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new InvalidOperationException("User ID not found in token.");
        }

        return userId;
    }

    public async Task<UserProfileViewDto?> GetCurrentUserProfileAsync(string token)
    {
        int userId = ExtractUserIdFromToken(token); 

        var user = await _Loginrepository.GetUserByIdAsync(userId);

        if (user == null)
            return null;

        return new UserProfileViewDto
        {
            Id = user.Id,
            Fullname = user.Fullname,
            Email = user.Email,
            Role = user.Role
        };
    }


}
