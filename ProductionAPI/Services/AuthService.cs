using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProductionAPI.Models;

namespace ProductionAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        // Временное хранилище пользователей (потом заменим на БД)
        private static List<User> _users = new List<User>()
        {
            new User { Id = 1, Username = "admin", PasswordHash = HashPassword("admin123"),
                      FullName = "Администратор", Role = "Admin" }
        };

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Регистрация нового пользователя
        public async Task<AuthResponse?> Register(RegisterRequest request)
        {
            // Проверка: не существует ли уже такой пользователь
            if (_users.Any(u => u.Username == request.Username))
                return null; // Пользователь уже есть

            var user = new User
            {
                Id = _users.Max(u => u.Id) + 1,
                Username = request.Username,
                PasswordHash = HashPassword(request.Password), // Храним пароль в зашифрованном виде
                FullName = request.FullName,
                Role = "Operator" // Новая роль - оператор
            };

            _users.Add(user);
            return GenerateToken(user); // Выдаем токен при регистрации
        }

        // Вход в систему
        public async Task<AuthResponse?> Login(LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Username == request.Username
                && VerifyPassword(request.Password, u.PasswordHash));

            if (user == null) return null; // Неверные данные

            return GenerateToken(user);
        }

        // Хеширование пароля (не храним пароли открытыми!)
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Проверка пароля
        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        // Генерация JWT токена
        private AuthResponse GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            // Claims - информация, которую мы кладем в токен
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("FullName", user.FullName ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                Username = user.Username,
                Role = user.Role ?? "User",
                ExpiresAt = tokenDescriptor.Expires ?? DateTime.UtcNow.AddMinutes(60)
            };
        }
    }
}