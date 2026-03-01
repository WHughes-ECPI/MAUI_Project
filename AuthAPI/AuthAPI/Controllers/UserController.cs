using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using AuthAPI.Services;

namespace AuthAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _users;

    public UserController(IUserService userService)
    {
        _users = userService;
    }

    // GET: api/<UserController>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if(!Request.Headers.ContainsKey("Authorization"))
            return Unauthorized();

        var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
        if(!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            return Unauthorized();

        var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

        if(credentials.Length != 2)
            return Unauthorized();

        string email = credentials[0];
        string password = credentials[1];

        var user = await _users.ValidateUserAsync(email, password);
        if (user == null)
            return Unauthorized();

        return Ok(new
        {user.UserId, user.Email, user.Name, user.Phone});
    }

}
