using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
using quiz.Repo.Interface;

namespace quiz.Repo.Implementation
{
    public class LoginRepo : ILoginRepo
    {
        private readonly QuiZappDbContext _context;

        public LoginRepo(QuiZappDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            bool isPasswordValid = user != null && BCrypt.Net.BCrypt.Verify(password, user.Passwordhash);

            if (!isPasswordValid)
            {
                return false; 
            }
            return true;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }

        public async Task<string> RegisterUserAsync(User request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return "User already exists with this email.";
            }

            _context.Users.Add(request);
            await _context.SaveChangesAsync();

            return "User registered successfully.";
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

    }
    
    

}