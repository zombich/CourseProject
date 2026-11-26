using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Services;

namespace WebApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TracksController : ControllerBase
    {
        private readonly TrackService _trackService = new();

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int userId) ? userId : null;
        }

        private bool IsModeratorOrAdmin()
            => User.IsInRole("Модератор") || User.IsInRole("Администратор");

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TrackDTO>> GetTrackByIdAsync(int id, [FromQuery] bool incrementView = true)
        {
            var track = await _trackService.GetTrackByIdAsync(id, GetCurrentUserId(), IsModeratorOrAdmin(), incrementView);

            if (track is null)
                return NotFound();

            return Ok(track);
        }

        [HttpGet("random")]
        public async Task<ActionResult<int>> GetRandomTrackIdAsync()
        {
            var id = await _trackService.GetRandomTrackIdAsync();
            if (id is null)
                return NotFound();
            return Ok(id.Value);
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<TrackListItemDTO>>> GetTopTracksAsync([FromQuery] int limit = 10)
        {
            var tracks = await _trackService.GetTopTracksAsync(limit);
            return Ok(tracks);
        }

        [HttpGet("album/{id:int}")]
        public async Task<ActionResult<AlbumDTO>> GetAlbumByIdAsync(int id)
        {
            var album = await _trackService.GetAlbumByIdAsync(id, GetCurrentUserId(), IsModeratorOrAdmin());

            if (album is null)
                return NotFound();

            return Ok(album);
        }

        [HttpGet("artist/{id:int}")]
        public async Task<ActionResult<ArtistDetailsDTO>> GetArtistByIdAsync(int id)
        {
            var artist = await _trackService.GetArtistByIdAsync(id, GetCurrentUserId(), User.IsInRole("Администратор"));

            if (artist is null)
                return NotFound();

            return Ok(artist);
        }

        [HttpGet("search")]
        public async Task<ActionResult<SearchResultDTO>> SearchAsync(
            [FromQuery] string query = "",
            [FromQuery] int? genreId = null,
            [FromQuery] string sort = "views_desc")
        {
            var result = await _trackService.SearchAsync(query, genreId, sort);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:int}/annotations")]
        public async Task<ActionResult<AnnotationDTO>> CreateAnnotationAsync(int id, [FromBody] CreateAnnotationDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Текст аннотации не может быть пустым." });

            var annotation = await _trackService.CreateAnnotationAsync(id, userId.Value, request);
            if (annotation is null)
                return NotFound();

            return Ok(annotation);
        }

        [Authorize(Roles = "Критик,Модератор,Администратор,Артист")]
        [HttpPost("annotations/{annotationId:int}/verify")]
        public async Task<ActionResult> VerifyAnnotationAsync(int annotationId)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            string roleName = User.FindFirst(ClaimTypes.Role)?.Value ?? "Критик";
            var (ok, error) = await _trackService.VerifyAnnotationAsync(annotationId, userId.Value, roleName);

            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось подтвердить аннотацию." });

            return Ok();
        }

        [Authorize(Roles = "Критик,Модератор,Администратор,Артист")]
        [HttpPost("{id:int}/verify-lyrics")]
        public async Task<ActionResult> VerifyTrackLyricsAsync(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            string roleName = User.FindFirst(ClaimTypes.Role)?.Value ?? "Критик";
            var (ok, error) = await _trackService.VerifyTrackLyricsAsync(id, userId.Value, roleName);
            if (!ok)
                return BadRequest(new { message = error ?? "Не удалось подтвердить текст трека." });

            return Ok();
        }

        [Authorize]
        [HttpPost("annotations/{annotationId:int}/like")]
        public async Task<ActionResult<ToggleLikeResponseDTO>> ToggleAnnotationLikeAsync(int annotationId)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _trackService.ToggleAnnotationLikeAsync(annotationId, userId.Value);
            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [Authorize]
        [HttpPost("annotations/{annotationId:int}/comments")]
        public async Task<ActionResult<AnnotationCommentDTO>> AddCommentAsync(int annotationId, [FromBody] CreateCommentDTO request)
        {
            int? userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Текст комментария не может быть пустым." });

            var comment = await _trackService.AddCommentAsync(annotationId, userId.Value, request);
            if (comment is null)
                return NotFound();

            return Ok(comment);
        }

        [Authorize(Roles = "Модератор,Администратор")]
        [HttpDelete("annotations/{annotationId:int}")]
        public async Task<ActionResult> DeleteAnnotationAsync(int annotationId)
        {
            bool deleted = await _trackService.DeleteAnnotationAsync(annotationId);
            if (!deleted)
                return NotFound();

            return Ok();
        }

        [Authorize(Roles = "Модератор,Администратор")]
        [HttpDelete("comments/{commentId:int}")]
        public async Task<ActionResult> DeleteCommentAsync(int commentId)
        {
            bool deleted = await _trackService.DeleteCommentAsync(commentId);
            if (!deleted)
                return NotFound();

            return Ok();
        }
    }
}
