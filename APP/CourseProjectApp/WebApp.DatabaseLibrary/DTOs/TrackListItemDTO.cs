namespace WebApp.DatabaseLibrary.DTOs
{
    public class TrackListItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = string.Empty;
        public string AlbumImage { get; set; } = string.Empty;
        public long ViewsCount { get; set; }
        public int PublicationYear { get; set; }
        public List<ArtistDTO> Artists { get; set; } = new();
    }
}
