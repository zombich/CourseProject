using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class TrackModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public TrackDTO? Track { get; set; }
        public AnnotationDTO? SelectedAnnotation { get; set; }
        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public string AnnotationContent { get; set; } = string.Empty;
        [BindProperty] public IFormFile? AnnotationImageFile { get; set; }
        [BindProperty] public int LineStart { get; set; } = 0;
        [BindProperty] public int LineEnd { get; set; } = 0;
        [BindProperty] public int CharStart { get; set; } = 0;
        [BindProperty] public int CharEnd { get; set; } = 0;
        [BindProperty] public string CommentContent { get; set; } = string.Empty;
        [BindProperty] public string? About { get; set; }
        [BindProperty] public string? Lyrics { get; set; }

        public TrackModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
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

        public async Task<ActionResult> OnGetAsync(int id = 1, int? annotationId = null, bool incrementView = true, string? status = null, string? error = null)
        {
            StatusMessage = status;
            ErrorMessage = error;

            var client = CreateApiClient();
            var response = await client.GetAsync($"api/tracks/{id}?incrementView={incrementView}");

            if (!response.IsSuccessStatusCode)
                return RedirectToPage("/Index");

            Track = await response.Content.ReadFromJsonAsync<TrackDTO>();
            if (Track is null)
                return RedirectToPage("/Index");

            SelectedAnnotation = annotationId.HasValue
                ? Track.Annotations.FirstOrDefault(a => a.Id == annotationId.Value) ?? Track.Annotations.FirstOrDefault()
                : Track.Annotations.FirstOrDefault();

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAnnotationImageAsync(IFormFile? file)
        {
            if (User.Identity?.IsAuthenticated != true ||
                !(User.IsInRole("Критик") || User.IsInRole("Модератор") || User.IsInRole("Администратор")))
            {
                return Unauthorized();
            }

            if (file is null || file.Length == 0)
                return BadRequest(new { error = "Файл не выбран." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp"))
                return BadRequest(new { error = "Недопустимый формат изображения." });

            var folder = Path.Combine(_env.WebRootPath, "images", "annotations");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return new JsonResult(new { url = $"/images/annotations/{fileName}" });
        }

        public async Task<IActionResult> OnPostAddAnnotationAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            string content = AnnotationContent?.Trim() ?? string.Empty;

            if (AnnotationImageFile is not null && AnnotationImageFile.Length > 0 &&
                (User.IsInRole("Критик") || User.IsInRole("Модератор") || User.IsInRole("Администратор")))
            {
                var ext = Path.GetExtension(AnnotationImageFile.FileName).ToLowerInvariant();
                if (ext is ".png" or ".jpg" or ".jpeg" or ".webp")
                {
                    var folder = Path.Combine(_env.WebRootPath, "images", "annotations");
                    Directory.CreateDirectory(folder);
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(folder, fileName);
                    await using var stream = new FileStream(fullPath, FileMode.Create);
                    await AnnotationImageFile.CopyToAsync(stream);

                    string imgMd = $"![Изображение](/images/annotations/{fileName})";
                    content = string.IsNullOrWhiteSpace(content) ? imgMd : $"{content}\n{imgMd}";
                }
            }

            if (!string.IsNullOrWhiteSpace(content) && LineStart > 0 && LineEnd > 0)
            {
                var client = CreateApiClient();
                var response = await client.PostAsJsonAsync($"api/tracks/{id}/annotations", new CreateAnnotationDTO
                {
                    Content = content,
                    LineStart = Math.Min(LineStart, LineEnd),
                    LineEnd = Math.Max(LineStart, LineEnd),
                    CharStart = CharStart,
                    CharEnd = CharEnd
                });

                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<AnnotationDTO>();
                    return RedirectToPage("/Track", new { id, annotationId = created?.Id, incrementView = false });
                }
            }

            return RedirectToPage("/Track", new { id, incrementView = false });
        }

        public async Task<IActionResult> OnPostVerifyAnnotationAsync(int id, int annotationId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.PostAsync($"api/tracks/annotations/{annotationId}/verify", null);
            if (!response.IsSuccessStatusCode)
            {
                string err = await ExtractErrorAsync(response, "Не удалось подтвердить аннотацию.");
                return RedirectToPage("/Track", new { id, annotationId, incrementView = false, error = err });
            }

            return RedirectToPage("/Track", new { id, annotationId, incrementView = false, status = "Аннотация подтверждена." });
        }

        public async Task<IActionResult> OnPostVerifyLyricsAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.PostAsync($"api/tracks/{id}/verify-lyrics", null);
            if (!response.IsSuccessStatusCode)
            {
                string err = await ExtractErrorAsync(response, "Не удалось подтвердить текст трека.");
                return RedirectToPage("/Track", new { id, incrementView = false, error = err });
            }

            return RedirectToPage("/Track", new { id, incrementView = false, status = "Текст трека подтверждён." });
        }

        public async Task<IActionResult> OnPostUpdateAboutAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync($"api/manage/tracks/{id}/update", new UpdateTrackDTO
            {
                About = About ?? string.Empty
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Track", new { id, incrementView = false, status = "Описание композиции обновлено." });

            string err = await ExtractErrorAsync(response, "Не удалось обновить описание.");
            return RedirectToPage("/Track", new { id, incrementView = false, error = err });
        }

        public async Task<IActionResult> OnPostUpdateLyricsAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync($"api/manage/tracks/{id}/update", new UpdateTrackDTO
            {
                Lyrics = Lyrics ?? string.Empty
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Track", new { id, incrementView = false, status = "Текст композиции обновлён." });

            string err = await ExtractErrorAsync(response, "Не удалось обновить текст.");
            return RedirectToPage("/Track", new { id, incrementView = false, error = err });
        }

        public async Task<IActionResult> OnPostDeleteTrackAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/manage/tracks/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Index");

            string err = await ExtractErrorAsync(response, "Не удалось удалить композицию.");
            return RedirectToPage("/Track", new { id, incrementView = false, error = err });
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int id, int annotationId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            await client.PostAsync($"api/tracks/annotations/{annotationId}/like", null);

            return RedirectToPage("/Track", new { id, annotationId, incrementView = false });
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int id, int annotationId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            if (!string.IsNullOrWhiteSpace(CommentContent))
            {
                var client = CreateApiClient();
                await client.PostAsJsonAsync($"api/tracks/annotations/{annotationId}/comments", new CreateCommentDTO
                {
                    Content = CommentContent
                });
            }

            return RedirectToPage("/Track", new { id, annotationId, incrementView = false });
        }

        public async Task<IActionResult> OnPostDeleteAnnotationAsync(int id, int annotationId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            await client.DeleteAsync($"api/tracks/annotations/{annotationId}");

            return RedirectToPage("/Track", new { id, incrementView = false, status = "Аннотация удалена." });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int id, int annotationId, int commentId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            await client.DeleteAsync($"api/tracks/comments/{commentId}");

            return RedirectToPage("/Track", new { id, annotationId, incrementView = false });
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
