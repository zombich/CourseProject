namespace WebApp.DatabaseLibrary.DTOs
{
    public class SearchResultDTO
    {
        public List<TrackListItemDTO> Tracks { get; set; } = new();
        public List<AlbumDTO> Albums { get; set; } = new();
        public List<ArtistDTO> Artists { get; set; } = new();
        public List<CriticDTO> Users { get; set; } = new();
        public List<GenreDTO> Genres { get; set; } = new();
    }
}
