using Microsoft.EntityFrameworkCore;
using WebApp.DatabaseLibrary.Contexts;
using WebApp.DatabaseLibrary.DTOs;

namespace WebApp.DatabaseLibrary.Services
{
    public class ProfileService
    {
        private readonly AppDbContext _context = new();

        public Task<ProfileDTO?> GetProfileByIdAsync(int id, int? currentUserId = null) =>
            _context.Users
                .Where(u => u.Id == id)
                .Select(u => new ProfileDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role.Name,
                    Bio = u.Bio,
                    AvatarUrl = u.AvatarUrl,
                    CreatedAt = u.CreatedAt,
                    AnnotationsCount = u.AnnotationAuthors.Count,
                    LikesCount = u.AnnotationAuthors.SelectMany(a => a.AnnotationLikes).Count(),
                    CommentsCount = u.AnnotationComments.Count,
                    VerifiedAnnotationsCount = u.AnnotationAuthors.Count(a => a.VerificationTypeId != null),
                    Contributions = u.AnnotationAuthors
                        .OrderByDescending(a => a.CreatedAt)
                        .Select(a => new ProfileContributionDTO
                        {
                            AnnotationId = a.Id,
                            TrackId = a.TrackId,
                            TrackTitle = a.Track.Title,
                            AlbumId = a.Track.AlbumId,
                            AlbumTitle = a.Track.Album.Title,
                            AlbumImage = a.Track.Album.CoverImageUrl,
                            PublicationYear = a.Track.Album.ReleaseDate.Year,
                            Content = a.Content,
                            CreatedAt = a.CreatedAt,
                            LikesCount = a.AnnotationLikes.Count,
                            IsLikedByCurrentUser = currentUserId.HasValue && a.AnnotationLikes.Any(l => l.UserId == currentUserId.Value),
                            CommentsCount = a.AnnotationComments.Count,
                            Comments = a.AnnotationComments
                                .OrderBy(c => c.CreatedAt)
                                .Select(c => new AnnotationCommentDTO
                                {
                                    Id = c.Id,
                                    AuthorId = c.AuthorId,
                                    AuthorUsername = c.Author.Username,
                                    AuthorRole = c.Author.Role.Name,
                                    Content = c.Content,
                                    CreatedAt = c.CreatedAt,
                                    LikesCount = c.CommentLikes.Count
                                }).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return false;

            if (dto.Bio is not null) user.Bio = dto.Bio.Trim();
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl)) user.AvatarUrl = dto.AvatarUrl.Trim();

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
