using AuthAPI.Models;

namespace AuthAPI.Services;

public interface IUserService
{
    Task<User?> ValidateUserAsync(string email, string password);
}
