using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApp.DatabaseLibrary.Contexts;
using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Models;

namespace WebApp.DatabaseLibrary.Services
{
    public class TrackService
    {
        private readonly AppDbContext _context = new();

        private static readonly Expression<Func<Track, TrackListItemDTO>> ToTrackListItem = t => new TrackListItemDTO
        {
            Id = t.Id,
            Title = t.Title,
            AlbumId = t.AlbumId,
            AlbumTitle = t.Album.Title,
            AlbumImage = t.Album.CoverImageUrl,
            ViewsCount = t.ViewsCount,
            PublicationYear = t.Album.ReleaseDate.Year,
            Artists = t.Artists.Select(a => new ArtistDTO
            {
                Id = a.Id,
                Name = a.Name,
                AvatarUrl = a.AvatarUrl
            }).ToList()
        };

        public Task<bool> CanUserManageTrackAsync(int userId, int trackId) =>
            _context.Tracks.AnyAsync(t => t.Id == trackId &&
                (t.Artists.Any(a => a.UserManagedArtists.Any(uma => uma.UserId == userId)) ||
                 t.Album.Artists.Any(a => a.UserManagedArtists.Any(uma => uma.UserId == userId))));

        public Task<bool> CanUserManageAlbumAsync(int userId, int albumId) =>
            _context.Albums.AnyAsync(al => al.Id == albumId &&
                al.Artists.Any(a => a.UserManagedArtists.Any(uma => uma.UserId == userId)));

        public async Task<TrackDTO?> GetTrackByIdAsync(int id, int? currentUserId = null, bool isModeratorOrAdmin = false, bool incrementView = true)
        {
            if (incrementView)
            {
                await _context.Tracks
                    .Where(t => t.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.ViewsCount, t => t.ViewsCount + 1));
            }

            return await _context.Tracks
                .Where(t => t.Id == id)
                .Select(t => new TrackDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Lyrics = t.Lyrics,
                    TrackNumber = t.TrackNumber,
                    DurationSeconds = t.DurationSeconds,
                    ViewsCount = t.ViewsCount,
                    About = t.About,
                    IsLyricsVerified = t.IsLyricsVerified,
                    LyricsVerificationLabel = !t.IsLyricsVerified
                        ? null
                        : _context.Users
                            .Where(u => u.Id == t.LyricsVerifierId)
                            .Select(u => u.Role.Name == "Артист"
                                ? "Проверено автором"
                                : (u.Role.Name == "Модератор" || u.Role.Name == "Администратор")
                                    ? "Проверено модератором"
                                    : "Проверено критиком")
                            .FirstOrDefault() ?? "Проверено",
                    CanManageTrack = isModeratorOrAdmin || (currentUserId.HasValue &&
                        (t.Artists.Any(a => a.UserManagedArtists.Any(uma => uma.UserId == currentUserId.Value)) ||
                         t.Album.Artists.Any(a => a.UserManagedArtists.Any(uma => uma.UserId == currentUserId.Value)))),
                    AlbumId = t.AlbumId,
                    AlbumTitle = t.Album.Title,
                    AlbumImage = t.Album.CoverImageUrl,
                    PublicationYear = t.Album.ReleaseDate.Year,
                    Artists = t.Artists.Select(a => new ArtistDTO { Id = a.Id, Name = a.Name, Bio = a.Bio, AvatarUrl = a.AvatarUrl }).ToList(),
                    Genres = t.Genres.Select(g => g.Name).ToList(),
                    Annotations = t.Annotations
                        .OrderBy(a => a.LineStart)
                        .ThenBy(a => a.CharStart)
                        .ThenBy(a => a.CreatedAt)
                        .Select(a => new AnnotationDTO
                        {
                            Id = a.Id,
                            TrackId = a.TrackId,
                            AuthorId = a.AuthorId,
                            AuthorUsername = a.Author.Username,
                            AuthorRole = a.Author.Role.Name,
                            Content = a.Content,
                            LineStart = a.LineStart,
                            LineEnd = a.LineEnd,
                            CharStart = a.CharStart,
                            CharEnd = a.CharEnd,
                            VerificationTypeName = a.VerificationType != null ? a.VerificationType.Name : null,
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
        }

        public Task<List<TrackListItemDTO>> GetTopTracksAsync(int limit = 10) =>
            _context.Tracks
                .OrderByDescending(t => t.ViewsCount)
                .Take(limit)
                .Select(ToTrackListItem)
                .ToListAsync();

        public Task<AlbumDTO?> GetAlbumByIdAsync(int id, int? currentUserId = null, bool isModeratorOrAdmin = false) =>
            _context.Albums
                .Where(a => a.Id == id)
                .Select(a => new AlbumDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    ReleaseDate = a.ReleaseDate,
                    CoverImageUrl = a.CoverImageUrl,
                    CanManageAlbum = isModeratorOrAdmin || (currentUserId.HasValue &&
                        a.Artists.Any(ar => ar.UserManagedArtists.Any(uma => uma.UserId == currentUserId.Value))),
                    Artists = a.Artists.Select(ar => new ArtistDTO { Id = ar.Id, Name = ar.Name, Bio = ar.Bio, AvatarUrl = ar.AvatarUrl }).ToList(),
                    Tracks = a.Tracks
                        .OrderBy(t => t.TrackNumber)
                        .Select(t => new AlbumTrackDTO
                        {
                            Id = t.Id,
                            TrackNumber = t.TrackNumber,
                            Title = t.Title,
                            DurationSeconds = t.DurationSeconds,
                            ViewsCount = t.ViewsCount,
                            Artists = t.Artists.Select(ar => new ArtistDTO { Id = ar.Id, Name = ar.Name, AvatarUrl = ar.AvatarUrl }).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

        public Task<ArtistDetailsDTO?> GetArtistByIdAsync(int id, int? currentUserId = null, bool isAdmin = false) =>
            _context.Artists
                .Where(a => a.Id == id)
                .Select(a => new ArtistDetailsDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Bio = a.Bio,
                    AvatarUrl = a.AvatarUrl,
                    CanManageArtist = isAdmin || (currentUserId.HasValue &&
                        a.UserManagedArtists.Any(uma => uma.UserId == currentUserId.Value)),
                    Albums = a.Albums
                        .OrderByDescending(al => al.ReleaseDate)
                        .Select(al => new AlbumDTO { Id = al.Id, Title = al.Title, ReleaseDate = al.ReleaseDate, CoverImageUrl = al.CoverImageUrl })
                        .ToList(),
                    Tracks = a.Tracks
                        .AsQueryable()
                        .OrderByDescending(t => t.ViewsCount)
                        .Select(ToTrackListItem)
                        .ToList()
                })
                .FirstOrDefaultAsync();

        public async Task<SearchResultDTO> SearchAsync(string query, int? genreId = null, string sort = "views_desc")
        {
            var genres = await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => new GenreDTO { Id = g.Id, Name = g.Name })
                .ToListAsync();

            bool hasQuery = !string.IsNullOrWhiteSpace(query);
            if (!hasQuery && !genreId.HasValue)
                return new SearchResultDTO { Genres = genres };

            string pattern = $"%{query.Trim()}%";
            var tracksQuery = _context.Tracks.AsQueryable();

            if (hasQuery)
                tracksQuery = tracksQuery.Where(t =>
                    EF.Functions.ILike(t.Title, pattern) ||
                    EF.Functions.ILike(t.Lyrics, pattern) ||
                    t.Artists.Any(a => EF.Functions.ILike(a.Name, pattern)));

            if (genreId > 0)
                tracksQuery = tracksQuery.Where(t => t.Genres.Any(g => g.Id == genreId.Value));

            tracksQuery = sort switch
            {
                "release_desc" => tracksQuery.OrderByDescending(t => t.Album.ReleaseDate),
                "release_asc" => tracksQuery.OrderBy(t => t.Album.ReleaseDate),
                "title" => tracksQuery.OrderBy(t => t.Title),
                _ => tracksQuery.OrderByDescending(t => t.ViewsCount)
            };

            var tracks = await tracksQuery.Take(30).Select(ToTrackListItem).ToListAsync();
            if (!hasQuery)
                return new SearchResultDTO { Tracks = tracks, Genres = genres };

            var albums = await _context.Albums
                .Where(a => EF.Functions.ILike(a.Title, pattern) || a.Artists.Any(ar => EF.Functions.ILike(ar.Name, pattern)))
                .OrderByDescending(a => a.ReleaseDate)
                .Take(12)
                .Select(a => new AlbumDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    ReleaseDate = a.ReleaseDate,
                    CoverImageUrl = a.CoverImageUrl,
                    Artists = a.Artists.Select(ar => new ArtistDTO { Id = ar.Id, Name = ar.Name, AvatarUrl = ar.AvatarUrl }).ToList()
                })
                .ToListAsync();

            var artists = await _context.Artists
                .Where(a => EF.Functions.ILike(a.Name, pattern))
                .Take(12)
                .Select(a => new ArtistDTO { Id = a.Id, Name = a.Name, Bio = a.Bio, AvatarUrl = a.AvatarUrl })
                .ToListAsync();

            var users = await _context.Users
                .Where(u => EF.Functions.ILike(u.Username, pattern))
                .Take(12)
                .Select(u => new CriticDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role.Name,
                    AnnotationsCount = u.AnnotationAuthors.Count,
                    LikesCount = u.AnnotationAuthors.SelectMany(a => a.AnnotationLikes).Count()
                })
                .ToListAsync();

            return new SearchResultDTO { Tracks = tracks, Albums = albums, Artists = artists, Users = users, Genres = genres };
        }

        public async Task<AnnotationDTO?> CreateAnnotationAsync(int trackId, int userId, CreateAnnotationDTO dto)
        {
            if (!await _context.Tracks.AnyAsync(t => t.Id == trackId))
                return null;

            var annotation = new Annotation
            {
                TrackId = trackId,
                AuthorId = userId,
                Content = dto.Content,
                LineStart = dto.LineStart,
                LineEnd = dto.LineEnd,
                CharStart = dto.CharStart,
                CharEnd = dto.CharEnd,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Annotations.AddAsync(annotation);
            await _context.SaveChangesAsync();

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);

            return new AnnotationDTO
            {
                Id = annotation.Id,
                TrackId = annotation.TrackId,
                AuthorId = annotation.AuthorId,
                AuthorUsername = user?.Username ?? string.Empty,
                AuthorRole = user.Role.Name,
                Content = annotation.Content,
                LineStart = annotation.LineStart,
                LineEnd = annotation.LineEnd,
                CharStart = annotation.CharStart,
                CharEnd = annotation.CharEnd,
                CreatedAt = annotation.CreatedAt
            };
        }

        public async Task<(bool ok, string? error)> VerifyAnnotationAsync(int annotationId, int verifierId, string roleName)
        {
            var annotation = await _context.Annotations.FirstOrDefaultAsync(a => a.Id == annotationId);
            if (annotation is null)
                return (false, "Аннотация не найдена.");

            if (roleName == "Артист" && !await CanUserManageTrackAsync(verifierId, annotation.TrackId))
                return (false, "Артист может подтверждать аннотации только к своим трекам.");

            annotation.VerificationTypeId = roleName switch
            {
                "Артист" => 1,
                "Модератор" or "Администратор" => 2,
                _ => 3
            };
            annotation.VerifierId = verifierId;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> VerifyTrackLyricsAsync(int trackId, int verifierId, string roleName)
        {
            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId);
            if (track is null)
                return (false, "Трек не найден.");

            if (roleName == "Артист" && !await CanUserManageTrackAsync(verifierId, trackId))
                return (false, "Артист может подтверждать текст только своих треков.");

            track.IsLyricsVerified = true;
            track.LyricsVerifierId = verifierId;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<ToggleLikeResponseDTO?> ToggleAnnotationLikeAsync(int annotationId, int userId)
        {
            if (!await _context.Annotations.AnyAsync(a => a.Id == annotationId))
                return null;

            var existingLike = await _context.AnnotationLikes.FirstOrDefaultAsync(l => l.AnnotationId == annotationId && l.UserId == userId);
            if (existingLike is not null)
                _context.AnnotationLikes.Remove(existingLike);
            else
                await _context.AnnotationLikes.AddAsync(new AnnotationLike { AnnotationId = annotationId, UserId = userId, CreatedAt = DateTime.UtcNow });

            await _context.SaveChangesAsync();
            int likesCount = await _context.AnnotationLikes.CountAsync(l => l.AnnotationId == annotationId);
            return new ToggleLikeResponseDTO { IsLiked = existingLike is null, LikesCount = likesCount };
        }

        public async Task<AnnotationCommentDTO?> AddCommentAsync(int annotationId, int userId, CreateCommentDTO dto)
        {
            if (!await _context.Annotations.AnyAsync(a => a.Id == annotationId))
                return null;

            var comment = new AnnotationComment
            {
                AnnotationId = annotationId,
                AuthorId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AnnotationComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);

            return new AnnotationCommentDTO
            {
                Id = comment.Id,
                AuthorId = comment.AuthorId,
                AuthorUsername = user?.Username ?? string.Empty,
                AuthorRole = user.Role.Name,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteAnnotationAsync(int annotationId)
        {
            var annotation = await _context.Annotations.FirstOrDefaultAsync(a => a.Id == annotationId);
            if (annotation is null)
                return false;

            _context.Annotations.Remove(annotation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.AnnotationComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment is null)
                return false;

            _context.AnnotationComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetRandomTrackIdAsync()
        {
            var ids = await _context.Tracks.Select(t => t.Id).ToListAsync();
            return ids.Count == 0 ? null : ids[Random.Shared.Next(ids.Count)];
        }

    }
}
