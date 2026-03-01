using System.Text;
using System.Text.Json;

namespace AuthAPI.Controllers;

    public class BasicAuthentication
    {
    private readonly HttpClient _http;

    public BasicAuthentication()
    {
        _http = new HttpClient
        {
            // When running on MAUI Windows, API address
            BaseAddress = new Uri("http://localhost:54336/")
        };
    }
    
    public async Task<UserDto?> LoginAsync(string email, string password)
        {
            var raw = $"{email}:{password}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);

            var res = await _http.GetAsync("/api/user/me");
            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized) return null;

            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
}

