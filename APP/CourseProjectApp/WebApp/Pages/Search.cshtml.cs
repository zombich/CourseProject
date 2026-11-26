using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GenreId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "views_desc";

        public SearchResultDTO Results { get; set; } = new();

        public SearchModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TracksApi");
            string query = Uri.EscapeDataString(Query ?? string.Empty);
            string genre = GenreId.HasValue ? $"&genreId={GenreId.Value}" : string.Empty;
            string sort = $"&sort={Uri.EscapeDataString(Sort)}";

            var response = await client.GetAsync($"api/tracks/search?query={query}{genre}{sort}");

            if (response.IsSuccessStatusCode)
            {
                Results = await response.Content.ReadFromJsonAsync<SearchResultDTO>() ?? new();
            }
        }
    }
}
