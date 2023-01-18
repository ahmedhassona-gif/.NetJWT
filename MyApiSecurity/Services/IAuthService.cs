using MyApiSecurity.Models;

namespace MyApiSecurity.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequistModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
        Task<UsersModel> GetAllUsers();
    }
}
