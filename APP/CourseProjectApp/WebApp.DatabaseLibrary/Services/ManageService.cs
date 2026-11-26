using Microsoft.EntityFrameworkCore;
using WebApp.DatabaseLibrary.Contexts;
using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Models;

namespace WebApp.DatabaseLibrary.Services
{
    public class ManageService
    {
        private readonly AppDbContext _context = new();

        private Task<List<int>> GetManagedArtistIdsAsync(int userId) =>
            _context.UserManagedArtists.Where(uma => uma.UserId == userId).Select(uma => uma.ArtistId).ToListAsync();

        public Task<bool> CanUserManageArtistAsync(int userId, int artistId) =>
            _context.UserManagedArtists.AnyAsync(uma => uma.UserId == userId && uma.ArtistId == artistId);

        public async Task<ManageContextDTO> GetManageContextAsync(int userId, bool isModeratorOrAdmin, bool isAdmin = false)
        {
            var managedIds = isModeratorOrAdmin ? new List<int>() : await GetManagedArtistIdsAsync(userId);

            var artists = await _context.Artists
                .Where(a => isModeratorOrAdmin || managedIds.Contains(a.Id))
                .OrderBy(a => a.Name)
                .Select(a => new ArtistDTO { Id = a.Id, Name = a.Name, Bio = a.Bio, AvatarUrl = a.AvatarUrl })
                .ToListAsync();

            var albums = await _context.Albums
                .Where(al => isModeratorOrAdmin || al.Artists.Any(ar => managedIds.Contains(ar.Id)))
                .OrderByDescending(a => a.ReleaseDate)
                .Select(a => new AlbumDTO { Id = a.Id, Title = a.Title, ReleaseDate = a.ReleaseDate, CoverImageUrl = a.CoverImageUrl })
                .ToListAsync();

            var tracks = await _context.Tracks
                .Where(t => isModeratorOrAdmin || t.Artists.Any(ar => managedIds.Contains(ar.Id)) || t.Album.Artists.Any(ar => managedIds.Contains(ar.Id)))
                .OrderByDescending(t => t.Id)
                .Select(t => new TrackListItemDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    AlbumId = t.AlbumId,
                    AlbumTitle = t.Album.Title,
                    AlbumImage = t.Album.CoverImageUrl,
                    ViewsCount = t.ViewsCount,
                    PublicationYear = t.Album.ReleaseDate.Year,
                    Artists = t.Artists.Select(ar => new ArtistDTO { Id = ar.Id, Name = ar.Name }).ToList()
                })
                .ToListAsync();

            var genres = await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => new GenreDTO { Id = g.Id, Name = g.Name })
                .ToListAsync();

            var artistUsers = isModeratorOrAdmin
                ? await _context.Users.Where(u => u.Role.Name == "Артист").OrderBy(u => u.Username)
                    .Select(u => new CriticDTO { Id = u.Id, Username = u.Username, RoleName = u.Role.Name }).ToListAsync()
                : new List<CriticDTO>();

            var allUsers = isAdmin
                ? await _context.Users.OrderBy(u => u.Username)
                    .Select(u => new CriticDTO { Id = u.Id, Username = u.Username, RoleName = u.Role.Name }).ToListAsync()
                : new List<CriticDTO>();

            var roles = isAdmin
                ? await _context.Roles.OrderBy(r => r.Id).Select(r => new RoleDTO { Id = r.Id, Name = r.Name }).ToListAsync()
                : new List<RoleDTO>();

            return new ManageContextDTO { Artists = artists, Albums = albums, Tracks = tracks, Genres = genres, ArtistUsers = artistUsers, AllUsers = allUsers, Roles = roles };
        }

        public async Task<ArtistDTO?> CreateArtistAsync(CreateArtistDTO dto)
        {
            if (await _context.Artists.AnyAsync(a => a.Name.ToLower() == dto.Name.Trim().ToLower()))
                return null;

            var artist = new Artist
            {
                Name = dto.Name.Trim(),
                Bio = dto.Bio,
                AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? "/" : dto.AvatarUrl.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
            return new ArtistDTO { Id = artist.Id, Name = artist.Name, Bio = artist.Bio, AvatarUrl = artist.AvatarUrl };
        }

        public async Task<(ArtistDTO? artist, string? error)> UpdateArtistAsync(int artistId, int userId, bool isAdmin, UpdateArtistDTO dto)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artistId);
            if (artist is null)
                return (null, "Артист не найден.");

            if (!isAdmin && !await CanUserManageArtistAsync(userId, artistId))
                return (null, "У вас нет прав на редактирование этой карточки артиста.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                string newName = dto.Name.Trim();
                if (await _context.Artists.AnyAsync(a => a.Id != artistId && a.Name.ToLower() == newName.ToLower()))
                    return (null, "Артист с таким именем уже существует.");
                artist.Name = newName;
            }

            if (dto.Bio is not null) artist.Bio = dto.Bio.Trim();
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl)) artist.AvatarUrl = dto.AvatarUrl.Trim();

            await _context.SaveChangesAsync();
            return (new ArtistDTO { Id = artist.Id, Name = artist.Name, Bio = artist.Bio, AvatarUrl = artist.AvatarUrl }, null);
        }

        public async Task<(bool ok, string? error)> AssignUserRoleAsync(int userId, int roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return (false, "Пользователь не найден.");
            if (!await _context.Roles.AnyAsync(r => r.Id == roleId)) return (false, "Роль не найдена.");

            user.RoleId = roleId;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> DeleteUserAsync(int targetUserId, int currentAdminId)
        {
            if (targetUserId == currentAdminId) return (false, "Нельзя удалить собственный аккаунт.");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (user is null) return (false, "Пользователь не найден.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> GrantArtistAccessAsync(int userId, int artistId)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userId) || !await _context.Artists.AnyAsync(a => a.Id == artistId))
                return false;

            if (!await CanUserManageArtistAsync(userId, artistId))
            {
                await _context.UserManagedArtists.AddAsync(new UserManagedArtist { UserId = userId, ArtistId = artistId, GrantedAt = DateTime.UtcNow });
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<(AlbumDTO? album, string? error)> CreateAlbumAsync(int userId, bool isModeratorOrAdmin, CreateAlbumDTO dto)
        {
            if (dto.ArtistIds.Count == 0)
                return (null, "Выберите хотя бы одного артиста.");

            if (!isModeratorOrAdmin)
            {
                var managedIds = await GetManagedArtistIdsAsync(userId);
                if (!dto.ArtistIds.All(id => managedIds.Contains(id)))
                    return (null, "У вас нет прав на добавление альбома для выбранного артиста.");
            }

            var artists = await _context.Artists.Where(a => dto.ArtistIds.Contains(a.Id)).ToListAsync();
            if (artists.Count == 0)
                return (null, "Выбранные артисты не найдены.");

            var album = new Album
            {
                Title = dto.Title.Trim(),
                ReleaseDate = dto.ReleaseDate,
                CoverImageUrl = string.IsNullOrWhiteSpace(dto.CoverImageUrl) ? "/" : dto.CoverImageUrl.Trim(),
                CreatedAt = DateTime.UtcNow,
                Artists = artists
            };

            await _context.Albums.AddAsync(album);
            await _context.SaveChangesAsync();

            return (new AlbumDTO
            {
                Id = album.Id,
                Title = album.Title,
                ReleaseDate = album.ReleaseDate,
                CoverImageUrl = album.CoverImageUrl,
                Artists = artists.Select(a => new ArtistDTO { Id = a.Id, Name = a.Name, AvatarUrl = a.AvatarUrl }).ToList()
            }, null);
        }

        public async Task<(bool ok, string? error)> UpdateAlbumCoverAsync(int albumId, int userId, bool isModeratorOrAdmin, string coverUrl)
        {
            var album = await _context.Albums.Include(a => a.Artists).FirstOrDefaultAsync(a => a.Id == albumId);
            if (album is null) return (false, "Альбом не найден.");

            if (!isModeratorOrAdmin)
            {
                var managedIds = await GetManagedArtistIdsAsync(userId);
                if (!album.Artists.Any(a => managedIds.Contains(a.Id)))
                    return (false, "У вас нет прав на изменение этого альбома.");
            }

            album.CoverImageUrl = coverUrl;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> DeleteAlbumAsync(int albumId, int userId, bool isModeratorOrAdmin)
        {
            var album = await _context.Albums.Include(a => a.Artists).FirstOrDefaultAsync(a => a.Id == albumId);
            if (album is null) return (false, "Альбом не найден.");

            if (!isModeratorOrAdmin)
            {
                var managedIds = await GetManagedArtistIdsAsync(userId);
                if (!album.Artists.Any(a => managedIds.Contains(a.Id)))
                    return (false, "Вы можете удалять только свои альбомы.");
            }

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(TrackListItemDTO? track, string? error)> CreateTrackAsync(int userId, bool isModeratorOrAdmin, CreateTrackDTO dto)
        {
            var album = await _context.Albums.Include(a => a.Artists).FirstOrDefaultAsync(a => a.Id == dto.AlbumId);
            if (album is null) return (null, "Альбом не найден.");

            if (!isModeratorOrAdmin)
            {
                var managedIds = await GetManagedArtistIdsAsync(userId);
                if (!album.Artists.Any(a => managedIds.Contains(a.Id)) || (dto.ArtistIds.Count > 0 && !dto.ArtistIds.All(id => managedIds.Contains(id))))
                    return (null, "У вас нет прав на добавление трека в этот альбом или для выбранного артиста.");
            }

            var artistIds = dto.ArtistIds.Count > 0 ? dto.ArtistIds : album.Artists.Select(a => a.Id).ToList();
            var artists = await _context.Artists.Where(a => artistIds.Contains(a.Id)).ToListAsync();
            var genres = await _context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();

            var track = new Track
            {
                AlbumId = album.Id,
                Title = dto.Title.Trim(),
                Lyrics = dto.Lyrics,
                TrackNumber = dto.TrackNumber,
                DurationSeconds = dto.DurationSeconds,
                About = dto.About,
                ViewsCount = 0,
                CreatedAt = DateTime.UtcNow,
                Artists = artists,
                Genres = genres
            };

            await _context.Tracks.AddAsync(track);
            await _context.SaveChangesAsync();

            return (new TrackListItemDTO
            {
                Id = track.Id,
                Title = track.Title,
                AlbumId = album.Id,
                AlbumTitle = album.Title,
                AlbumImage = album.CoverImageUrl,
                ViewsCount = 0,
                PublicationYear = album.ReleaseDate.Year,
                Artists = artists.Select(a => new ArtistDTO { Id = a.Id, Name = a.Name, AvatarUrl = a.AvatarUrl }).ToList()
            }, null);
        }

        public async Task<(bool ok, string? error)> UpdateTrackAsync(int trackId, int userId, bool isModeratorOrAdmin, UpdateTrackDTO dto)
        {
            var track = await _context.Tracks.Include(t => t.Artists).Include(t => t.Album).ThenInclude(a => a.Artists).FirstOrDefaultAsync(t => t.Id == trackId);
            if (track is null) return (false, "Трек не найден.");

            if (!isModeratorOrAdmin)
            {
                var managedIds = await GetManagedArtistIdsAsync(userId);
                if (!track.Artists.Any(a => managedIds.Contains(a.Id)) && !track.Album.Artists.Any(a => managedIds.Contains(a.Id)))
                    return (false, "Вы можете редактировать только свои треки.");
            }

            if (dto.About is not null) track.About = dto.About.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Lyrics)) track.Lyrics = dto.Lyrics;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> DeleteTrackAsync(int trackId, int userId, bool isModeratorOrAdmin)
        {
            var track = await _context.Tracks.Include(t => t.Artists).Include(t => t.Album).ThenInclude(a => a.Artists).FirstOrDefaultAsync(t => t.Id == trackId);
            if (track is null) return (false, "Трек не найден.");

            if (!isModeratorOrAdmin)
            {
                var managedIds = await GetManagedArtistIdsAsync(userId);
                if (!track.Artists.Any(a => managedIds.Contains(a.Id)) && !track.Album.Artists.Any(a => managedIds.Contains(a.Id)))
                    return (false, "Вы можете удалять только свои треки.");
            }

            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<TrackStatsCsvRowDTO>> GetTrackStatsForExportAsync(int userId, bool isModeratorOrAdmin)
        {
            var managedIds = isModeratorOrAdmin ? new List<int>() : await GetManagedArtistIdsAsync(userId);

            return await _context.Tracks
                .Where(t => isModeratorOrAdmin || t.Artists.Any(a => managedIds.Contains(a.Id)) || t.Album.Artists.Any(a => managedIds.Contains(a.Id)))
                .OrderByDescending(t => t.ViewsCount)
                .Select(t => new TrackStatsCsvRowDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    AlbumTitle = t.Album.Title,
                    Artists = string.Join(", ", t.Artists.Select(a => a.Name)),
                    ViewsCount = t.ViewsCount,
                    AnnotationsCount = t.Annotations.Count,
                    ReleaseDate = t.Album.ReleaseDate
                })
                .ToListAsync();
        }
    }
}
