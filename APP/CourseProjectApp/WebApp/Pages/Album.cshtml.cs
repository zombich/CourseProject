using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class AlbumModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public AlbumDTO? Album { get; set; }
        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public IFormFile? CoverFile { get; set; }

        public AlbumModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
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

            var response = await client.GetAsync($"api/tracks/album/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Index");
            }

            Album = await response.Content.ReadFromJsonAsync<AlbumDTO>();
            if (Album is null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateCoverAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            if (CoverFile is not null && CoverFile.Length > 0)
            {
                var ext = Path.GetExtension(CoverFile.FileName).ToLowerInvariant();
                if (ext is ".png" or ".jpg" or ".jpeg" or ".webp")
                {
                    var folder = Path.Combine(_env.WebRootPath, "images", "albums");
                    Directory.CreateDirectory(folder);
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(folder, fileName);
                    await using var stream = new FileStream(fullPath, FileMode.Create);
                    await CoverFile.CopyToAsync(stream);

                    var client = CreateApiClient();
                    var response = await client.PostAsJsonAsync($"api/manage/albums/{id}/cover", new UpdateAlbumCoverDTO
                    {
                        CoverImageUrl = $"/images/albums/{fileName}"
                    });

                    if (response.IsSuccessStatusCode)
                        return RedirectToPage("/Album", new { id, status = "Обложка альбома обновлена." });

                    return RedirectToPage("/Album", new { id, error = await ExtractErrorAsync(response, "Не удалось обновить обложку.") });
                }
            }

            return RedirectToPage("/Album", new { id });
        }

        public async Task<IActionResult> OnPostDeleteAlbumAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/manage/albums/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Index");

            return RedirectToPage("/Album", new { id, error = await ExtractErrorAsync(response, "Не удалось удалить альбом.") });
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
            return fallback;
        }
    }
}
