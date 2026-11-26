namespace WebApp.DatabaseLibrary.DTOs
{
    public class ArtistDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool CanManageArtist { get; set; }
        public List<AlbumDTO> Albums { get; set; } = new();
        public List<TrackListItemDTO> Tracks { get; set; } = new();
    }
}
