namespace WebApp.DatabaseLibrary.DTOs
{
    public class UpdateArtistDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
