using AuthAPI.Models;
using AuthAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Services;

public class UserService : IUserService
{
    private readonly AuthDbContext _db;
    public UserService(AuthDbContext db)
    {
        _db = db;
    }
    
    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    }
}
