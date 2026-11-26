using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    [Authorize(Roles = "Модератор,Администратор,Артист")]
    public class ManageModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public ManageContextDTO Context { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public string ArtistName { get; set; } = string.Empty;
        [BindProperty] public string? ArtistBio { get; set; }
        [BindProperty] public IFormFile? ArtistAvatarFile { get; set; }

        [BindProperty] public int GrantUserId { get; set; }
        [BindProperty] public int GrantArtistId { get; set; }
        [BindProperty] public int AssignUserId { get; set; }
        [BindProperty] public int AssignRoleId { get; set; }

        [BindProperty] public string AlbumTitle { get; set; } = string.Empty;
        [BindProperty] public DateOnly AlbumReleaseDate { get; set; }
        [BindProperty] public IFormFile? AlbumCoverFile { get; set; }
        [BindProperty] public List<int> AlbumArtistIds { get; set; } = new();

        [BindProperty] public int TrackAlbumId { get; set; }
        [BindProperty] public List<int> TrackArtistIds { get; set; } = new();
        [BindProperty] public int TrackGenreId { get; set; }
        [BindProperty] public string TrackTitle { get; set; } = string.Empty;
        [BindProperty] public int TrackNumber { get; set; } = 1;
        [BindProperty] public int TrackDurationSeconds { get; set; } = 180;
        [BindProperty] public string? TrackAbout { get; set; }
        [BindProperty] public string TrackLyrics { get; set; } = string.Empty;

        public ManageModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
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

        private async Task<string?> SaveImageAsync(IFormFile? file, string subfolder)
        {
            if (file is null || file.Length == 0)
                return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp"))
                return null;

            var folder = Path.Combine(_env.WebRootPath, "images", subfolder);
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{subfolder}/{fileName}";
        }

        private async Task LoadContextAsync()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/manage/context");
            if (response.IsSuccessStatusCode)
            {
                Context = await response.Content.ReadFromJsonAsync<ManageContextDTO>() ?? new();
            }
        }

        public async Task OnGetAsync(string? status = null)
        {
            await LoadContextAsync();
            if (!string.IsNullOrEmpty(status))
            {
                SuccessMessage = status;
            }
        }

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/manage/stats");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("/Manage");

            var rows = await response.Content.ReadFromJsonAsync<List<TrackStatsCsvRowDTO>>() ?? new();
            var sb = new StringBuilder();
            sb.AppendLine("Id;Название;Альбом;Артисты;Просмотры;Аннотации;Дата релиза");
            foreach (var r in rows)
            {
                sb.AppendLine($"{r.Id};\"{r.Title.Replace("\"", "\"\"")}\";\"{r.AlbumTitle.Replace("\"", "\"\"")}\";\"{r.Artists.Replace("\"", "\"\"")}\";{r.ViewsCount};{r.AnnotationsCount};{r.ReleaseDate:yyyy-MM-dd}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"track_stats_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> OnPostCreateArtistAsync()
        {
            string? avatarUrl = await SaveImageAsync(ArtistAvatarFile, "artists");

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/manage/artists", new CreateArtistDTO
            {
                Name = ArtistName,
                Bio = ArtistBio,
                AvatarUrl = avatarUrl
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = $"Артист «{ArtistName}» успешно добавлен." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось создать артиста.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostGrantAccessAsync()
        {
            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/manage/artists/grant-access", new GrantArtistAccessDTO
            {
                UserId = GrantUserId,
                ArtistId = GrantArtistId
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = "Доступ к карточке артиста выдан." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось выдать доступ.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAlbumAsync()
        {
            string? coverUrl = await SaveImageAsync(AlbumCoverFile, "albums");

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/manage/albums", new CreateAlbumDTO
            {
                Title = AlbumTitle,
                ReleaseDate = AlbumReleaseDate,
                CoverImageUrl = coverUrl ?? string.Empty,
                ArtistIds = AlbumArtistIds
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = $"Альбом «{AlbumTitle}» успешно добавлен." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось создать альбом.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateTrackAsync()
        {
            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/manage/tracks", new CreateTrackDTO
            {
                AlbumId = TrackAlbumId,
                Title = TrackTitle,
                Lyrics = TrackLyrics,
                TrackNumber = TrackNumber,
                DurationSeconds = TrackDurationSeconds,
                About = TrackAbout,
                ArtistIds = TrackArtistIds,
                GenreIds = TrackGenreId > 0 ? new List<int> { TrackGenreId } : new List<int>()
            });

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<TrackListItemDTO>();
                if (created is not null)
                    return RedirectToPage("/Track", new { id = created.Id });

                return RedirectToPage("/Manage", new { status = $"Композиция «{TrackTitle}» успешно добавлена." });
            }

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось добавить композицию.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAssignRoleAsync()
        {
            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/manage/users/assign-role", new AssignRoleDTO
            {
                UserId = AssignUserId,
                RoleId = AssignRoleId
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = "Роль пользователя обновлена." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось выдать роль.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/manage/users/{userId}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = "Аккаунт пользователя удалён." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось удалить аккаунт.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAlbumAsync(int albumId)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/manage/albums/{albumId}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = "Альбом удалён." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось удалить альбом.");
            await LoadContextAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTrackAsync(int trackId)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/manage/tracks/{trackId}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Manage", new { status = "Композиция удалена." });

            ErrorMessage = await ExtractErrorAsync(response, "Не удалось удалить композицию.");
            await LoadContextAsync();
            return Page();
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
            catch
            {
            }
            return $"{fallback} (HTTP {(int)response.StatusCode})";
        }
    }
}
