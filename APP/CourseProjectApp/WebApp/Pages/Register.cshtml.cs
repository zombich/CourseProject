using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все обязательные поля.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("TracksApi");
            var response = await client.PostAsJsonAsync("api/users/register", new RegisterRequestDTO
            {
                Username = Username,
                Email = Email,
                Password = Password
            });

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ExtractErrorMessageAsync(response, "Произошла ошибка при регистрации. Проверьте свои данные.");
                return Page();
            }

            string token = (await response.Content.ReadAsStringAsync()).Trim('"');
            await SignInWithJwtAsync(token);

            return RedirectToPage("/Index");
        }

        private async Task SignInWithJwtAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = new List<Claim> { new Claim("AccessToken", token) };
            foreach (var c in jwt.Claims)
            {
                if (c.Type == "unique_name" || c.Type == ClaimTypes.Name)
                    claims.Add(new Claim(ClaimTypes.Name, c.Value));
                else if (c.Type == "role" || c.Type == ClaimTypes.Role)
                    claims.Add(new Claim(ClaimTypes.Role, c.Value));
                else if (c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier)
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, c.Value));
                else
                    claims.Add(c);
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                });
        }

        private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response, string fallback)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                {
                    return msg.GetString() ?? fallback;
                }
            }
            catch
            {
            }
            return fallback;
        }
    }
}
