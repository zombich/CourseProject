using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public ProfileDTO? Profile { get; set; }
        public bool IsOwnProfile { get; set; }
        public int? OpenAnnotationId { get; set; }
        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public string? Bio { get; set; }
        [BindProperty] public IFormFile? AvatarFile { get; set; }
        [BindProperty] public string CommentContent { get; set; } = string.Empty;

        public ProfileModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
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

        public async Task<IActionResult> OnGetAsync(int? id = null, int? openAnnotationId = null, string? status = null)
        {
            OpenAnnotationId = openAnnotationId;
            StatusMessage = status;

            var client = CreateApiClient();

            HttpResponseMessage response;
            if (id.HasValue)
            {
                response = await client.GetAsync($"api/users/profile/{id.Value}");
            }
            else
            {
                if (User.Identity?.IsAuthenticated != true)
                    return RedirectToPage("/Login");

                response = await client.GetAsync("api/users/profile/me");
            }

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Index");
            }

            Profile = await response.Content.ReadFromJsonAsync<ProfileDTO>();
            if (Profile is null)
            {
                return RedirectToPage("/Index");
            }

            var currentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            IsOwnProfile = int.TryParse(currentIdClaim, out int currentUserId) && currentUserId == Profile.Id;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAvatarAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            if (AvatarFile is not null && AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
                if (ext is ".png" or ".jpg" or ".jpeg" or ".webp")
                {
                    var folder = Path.Combine(_env.WebRootPath, "images", "users");
                    Directory.CreateDirectory(folder);
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(folder, fileName);
                    await using var stream = new FileStream(fullPath, FileMode.Create);
                    await AvatarFile.CopyToAsync(stream);

                    var client = CreateApiClient();
                    var response = await client.PostAsJsonAsync("api/users/profile/me/update", new UpdateProfileDTO
                    {
                        Bio = null,
                        AvatarUrl = $"/images/users/{fileName}"
                    });

                    if (response.IsSuccessStatusCode)
                        return RedirectToPage("/Profile", new { status = "Аватарка обновлена." });
                }
            }

            return RedirectToPage("/Profile");
        }

        public async Task<IActionResult> OnPostUpdateBioAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/users/profile/me/update", new UpdateProfileDTO
            {
                Bio = Bio ?? string.Empty,
                AvatarUrl = null
            });

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Profile", new { status = "Описание обновлено." });

            return RedirectToPage("/Profile");
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int profileId, int annotationId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            await client.PostAsync($"api/tracks/annotations/{annotationId}/like", null);

            return RedirectToPage("/Profile", new { id = profileId, openAnnotationId = annotationId });
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int profileId, int annotationId)
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

            return RedirectToPage("/Profile", new { id = profileId, openAnnotationId = annotationId });
        }

        public async Task<IActionResult> OnPostDeleteAnnotationAsync(int profileId, int annotationId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            await client.DeleteAsync($"api/tracks/annotations/{annotationId}");

            return RedirectToPage("/Profile", new { id = profileId, status = "Аннотация удалена." });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int profileId, int annotationId, int commentId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Login");

            var client = CreateApiClient();
            await client.DeleteAsync($"api/tracks/comments/{commentId}");

            return RedirectToPage("/Profile", new { id = profileId, openAnnotationId = annotationId });
        }
    }
}
