namespace WebApp.DatabaseLibrary.DTOs
{
    public class ProfileDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int AnnotationsCount { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int VerifiedAnnotationsCount { get; set; }
        public List<ProfileContributionDTO> Contributions { get; set; } = new();
    }
}
