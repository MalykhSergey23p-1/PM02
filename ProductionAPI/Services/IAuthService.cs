using System.Threading.Tasks;
using ProductionAPI.Models;

namespace ProductionAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> Register(RegisterRequest request);
        Task<AuthResponse?> Login(LoginRequest request);
    }
}