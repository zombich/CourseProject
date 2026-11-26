using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class RandomTrackModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RandomTrackModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TracksApi");
            var response = await client.GetAsync("api/tracks/random");

            if (response.IsSuccessStatusCode)
            {
                int trackId = await response.Content.ReadFromJsonAsync<int>();
                if (trackId > 0)
                {
                    return RedirectToPage("/Track", new { id = trackId });
                }
            }

            return RedirectToPage("/Index");
        }
    }
}
