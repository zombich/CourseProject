using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<TrackListItemDTO> Tracks { get; set; } = new();
        public List<CriticDTO> Critics { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TracksApi");

            var tracksResponse = await client.GetAsync("api/tracks/top?limit=10");
            if (tracksResponse.IsSuccessStatusCode)
            {
                Tracks = await tracksResponse.Content.ReadFromJsonAsync<List<TrackListItemDTO>>() ?? new();
            }

            var criticsResponse = await client.GetAsync("api/users/top-critics?limit=6");
            if (criticsResponse.IsSuccessStatusCode)
            {
                Critics = await criticsResponse.Content.ReadFromJsonAsync<List<CriticDTO>>() ?? new();
            }
        }
    }
}
