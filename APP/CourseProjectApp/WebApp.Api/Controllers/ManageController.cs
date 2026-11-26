using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Services;

namespace WebApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Модератор,Администратор,Артист")]
    public class ManageController : ControllerBase
    {
        private readonly ManageService _manageService = new();

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int userId) ? userId : null;
        }

        private bool IsModeratorOrAdmin()
            => User.IsInRole("Модератор") || User.IsInRole("Администратор");

        [HttpGet("context")]
        public async Task<ActionResult<ManageContextDTO>> GetContextAsync()
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var context = await _manageService.GetManageContextAsync(userId.Value, IsModeratorOrAdmin(), User.IsInRole("Администратор"));
            return Ok(context);
        }

        [Authorize(Roles = "Модератор,Администратор")]
        [HttpPost("artists")]
        public async Task<ActionResult<ArtistDTO>> CreateArtistAsync([FromBody] CreateArtistDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Введите имя артиста." });

            var artist = await _manageService.CreateArtistAsync(request);
            if (artist is null)
                return BadRequest(new { message = "Артист с таким именем уже существует." });

            return Ok(artist);
        }

        [Authorize(Roles = "Администратор,Артист")]
        [HttpPut("artists/{id:int}")]
        [HttpPost("artists/{id:int}/update")]
        public async Task<ActionResult<ArtistDTO>> UpdateArtistAsync(int id, [FromBody] UpdateArtistDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var (artist, error) = await _manageService.UpdateArtistAsync(id, userId.Value, User.IsInRole("Администратор"), request);
            if (artist is null)
                return BadRequest(new { message = error ?? "Не удалось обновить карточку артиста." });

            return Ok(artist);
        }

        [Authorize(Roles = "Модератор,Администратор")]
        [HttpPost("artists/grant-access")]
        public async Task<ActionResult> GrantArtistAccessAsync([FromBody] GrantArtistAccessDTO request)
        {
            bool granted = await _manageService.GrantArtistAccessAsync(request.UserId, request.ArtistId);
            if (!granted)
                return BadRequest(new { message = "Пользователь или артист не найден." });

            return Ok();
        }

        [HttpPost("albums")]
        public async Task<ActionResult<AlbumDTO>> CreateAlbumAsync([FromBody] CreateAlbumDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Введите название альбома." });

            var (album, error) = await _manageService.CreateAlbumAsync(userId.Value, IsModeratorOrAdmin(), request);
            if (album is null)
                return BadRequest(new { message = error ?? "Не удалось создать альбом." });

            return Ok(album);
        }

        [HttpPost("albums/{id:int}/cover")]
        public async Task<ActionResult> UpdateAlbumCoverAsync(int id, [FromBody] UpdateAlbumCoverDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var (ok, error) = await _manageService.UpdateAlbumCoverAsync(id, userId.Value, IsModeratorOrAdmin(), request.CoverImageUrl);
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось обновить обложку альбома." });

            return Ok();
        }

        [HttpDelete("albums/{id:int}")]
        public async Task<ActionResult> DeleteAlbumAsync(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var (ok, error) = await _manageService.DeleteAlbumAsync(id, userId.Value, IsModeratorOrAdmin());
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось удалить альбом." });

            return Ok();
        }

        [HttpPost("tracks")]
        public async Task<ActionResult<TrackListItemDTO>> CreateTrackAsync([FromBody] CreateTrackDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Lyrics))
                return BadRequest(new { message = "Введите название и текст трека." });

            var (track, error) = await _manageService.CreateTrackAsync(userId.Value, IsModeratorOrAdmin(), request);
            if (track is null)
                return BadRequest(new { message = error ?? "Не удалось создать трек." });

            return Ok(track);
        }

        [HttpPost("tracks/{id:int}/update")]
        public async Task<ActionResult> UpdateTrackAsync(int id, [FromBody] UpdateTrackDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var (ok, error) = await _manageService.UpdateTrackAsync(id, userId.Value, IsModeratorOrAdmin(), request);
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось обновить трек." });

            return Ok();
        }

        [HttpDelete("tracks/{id:int}")]
        public async Task<ActionResult> DeleteTrackAsync(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var (ok, error) = await _manageService.DeleteTrackAsync(id, userId.Value, IsModeratorOrAdmin());
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось удалить трек." });

            return Ok();
        }

        [Authorize(Roles = "Администратор")]
        [HttpPost("users/assign-role")]
        public async Task<ActionResult> AssignUserRoleAsync([FromBody] AssignRoleDTO request)
        {
            var (ok, error) = await _manageService.AssignUserRoleAsync(request.UserId, request.RoleId);
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось изменить роль." });

            return Ok();
        }

        [Authorize(Roles = "Администратор")]
        [HttpDelete("users/{id:int}")]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId is null)
                return Unauthorized();

            var (ok, error) = await _manageService.DeleteUserAsync(id, currentUserId.Value);
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось удалить аккаунт." });

            return Ok();
        }

        [HttpGet("stats")]
        public async Task<ActionResult<List<TrackStatsCsvRowDTO>>> GetStatsAsync()
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var stats = await _manageService.GetTrackStatsForExportAsync(userId.Value, IsModeratorOrAdmin());
            return Ok(stats);
        }
    }
}
