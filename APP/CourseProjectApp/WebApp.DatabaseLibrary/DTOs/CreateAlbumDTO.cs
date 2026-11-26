namespace WebApp.DatabaseLibrary.DTOs
{
    public class CreateAlbumDTO
    {
        public string Title { get; set; } = string.Empty;
        public DateOnly ReleaseDate { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public List<int> ArtistIds { get; set; } = new();
    }
}
