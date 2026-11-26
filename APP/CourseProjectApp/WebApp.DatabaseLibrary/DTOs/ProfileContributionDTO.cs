namespace WebApp.DatabaseLibrary.DTOs
{
    public class ProfileContributionDTO
    {
        public int AnnotationId { get; set; }
        public int TrackId { get; set; }
        public string TrackTitle { get; set; } = string.Empty;
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = string.Empty;
        public string AlbumImage { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int CommentsCount { get; set; }
        public List<AnnotationCommentDTO> Comments { get; set; } = new();
    }
}
