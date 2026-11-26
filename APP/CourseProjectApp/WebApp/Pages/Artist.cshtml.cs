using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class ArtistModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public ArtistDetailsDTO? Artist { get; set; }
        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public string Name { get; set; } = string.Empty;
        [BindProperty] public string? Bio { get; set; }
        [BindProperty] public IFormFile? AvatarFile { get; set; }

        public ArtistModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient("TracksApi");
            string? token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<IActionResult> OnGetAsync(int id = 1, string? status = null, string? error = null)
        {
            StatusMessage = status;
            ErrorMessage = error;

            var client = CreateApiClient();
            var response = await client.GetAsync($"api/tracks/artist/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Index");
            }

            Artist = await response.Content.ReadFromJsonAsync<ArtistDetailsDTO>();
            if (Artist is null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAvatarAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            if (AvatarFile is not null && AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
                if (ext is ".png" or ".jpg" or ".jpeg" or ".webp")
                {
                    var folder = Path.Combine(_env.WebRootPath, "images", "artists");
                    Directory.CreateDirectory(folder);
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(folder, fileName);
                    await using var stream = new FileStream(fullPath, FileMode.Create);
                    await AvatarFile.CopyToAsync(stream);

                    var client = CreateApiClient();
                    var response = await client.PostAsJsonAsync($"api/manage/artists/{id}/update", new UpdateArtistDTO
                    {
                        Name = string.Empty,
                        Bio = null,
                        AvatarUrl = $"/images/artists/{fileName}"
                    });

                    if (response.IsSuccessStatusCode)
                        return RedirectToPage("/Artist", new { id, status = "Картинка артиста обновлена." });

                    return RedirectToPage("/Artist", new { id, error = await ExtractErrorAsync(response, "Не удалось обновить картинку.") });
                }
            }

            return RedirectToPage("/Artist", new { id });
        }

        public async Task<IActionResult> OnPostUpdateBioAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync($"api/manage/artists/{id}/update", new UpdateArtistDTO
            {
                Name = Name,
                Bio = Bio ?? string.Empty,
                AvatarUrl = null
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Artist", new { id, status = "Карточка артиста обновлена." });

            return RedirectToPage("/Artist", new { id, error = await ExtractErrorAsync(response, "Не удалось обновить карточку.") });
        }

        private static async Task<string> ExtractErrorAsync(HttpResponseMessage response, string fallback)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? fallback;
            }
            catch { }
            return $"{fallback} (HTTP {(int)response.StatusCode})";
        }
    }
}
