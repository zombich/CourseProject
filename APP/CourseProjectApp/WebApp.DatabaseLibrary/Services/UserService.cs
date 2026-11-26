using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp.DatabaseLibrary.Contexts;
using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Models;

namespace WebApp.DatabaseLibrary.Services
{
    public class UserService
    {
        private readonly AppDbContext _context = new();
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string CreateToken(int userId, string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GetTokenByUserAsync(User user)
        {
            string role = (await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId))?.Name ?? "Пользователь";

            return CreateToken(user.Id, user.Username, role);
        }

        private string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password + _configuration["User:Salt"]);

        private bool IsPasswordCorrect(string password, string passwordHash)
            => BCrypt.Net.BCrypt.Verify(password + _configuration["User:Salt"], passwordHash);

        public async Task<bool> IsUserExistsAsync(string username, string email)
            => await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower() || u.Email.ToLower() == email.ToLower());

        public async Task<string> RegisterUserAsync(string username, string email, string password)
        {
            User newUser = new()
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return await GetTokenByUserAsync(newUser);
        }

        public async Task<string?> LoginUserAsync(string username, string password)
        {
            User? selectedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (selectedUser is not null && IsPasswordCorrect(password, selectedUser.PasswordHash))
                return await GetTokenByUserAsync(selectedUser);

            return null;
        }

        public async Task<List<CriticDTO>> GetTopCriticsAsync(int limit = 6)
        {
            var critics = await _context.Users
                .Where(u => u.RoleId == 4 || u.AnnotationAuthors.Any())
                .Select(u => new CriticDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role.Name,
                    AnnotationsCount = u.AnnotationAuthors.Count,
                    LikesCount = u.AnnotationAuthors.SelectMany(a => a.AnnotationLikes).Count()
                })
                .OrderByDescending(c => c.AnnotationsCount)
                .ThenByDescending(c => c.LikesCount)
                .Take(limit)
                .ToListAsync();

            return critics;
        }
    }
}
