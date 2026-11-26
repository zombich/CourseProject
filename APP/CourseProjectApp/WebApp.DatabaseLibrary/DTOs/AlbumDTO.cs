namespace WebApp.DatabaseLibrary.DTOs
{
    public class AlbumDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateOnly ReleaseDate { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public bool CanManageAlbum { get; set; }
        public List<ArtistDTO> Artists { get; set; } = new();
        public List<AlbumTrackDTO> Tracks { get; set; } = new();
    }
}
