using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
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
            return false; // Invalid credentials
        }

        bool result = await _Loginrepository.ValidateUserAsync(email, password);
        return result;
    }

    public async Task<string> GenerateToken(LoginModel request)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);  //Metadata for signing the token like algo used

        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
        }
        User user = await _Loginrepository.GetUserByEmailAsync(request.Email);

        Console.WriteLine($"User Role is: {user.Role}");

        var claims = new[]     // claims in Payload of JWT token
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
            signingCredentials: creds);   // signature to ensure token integrity (no one can tamper with it)

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        //  Decode token to check if Role is actually included
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(tokenString);

        Console.WriteLine(" Decoded Token Claims:");
        foreach (var claim in decodedToken.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
        }
        return tokenString;
    }

    public async Task<String> RegisterUserAsync(RegistrationViewModel request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.password))
        {
            return "Email and Password cannot be null or empty.";
        }

        // Hash the password before saving
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.password);

        User newUser = new User
        {
            Fullname = request.Name!,
            Email = request.Email,
            Passwordhash = hashedPassword,
            Role = "User" // Default "User" 
        };

        // Save the user to the repository
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

        // Extract the user ID from the token claims
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new InvalidOperationException("User ID not found in token.");
        }

        return userId;
    }

}
