using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class Track
{
    public int Id { get; set; }

    public int AlbumId { get; set; }

    public string Title { get; set; } = null!;

    public string Lyrics { get; set; } = null!;

    public int TrackNumber { get; set; }

    public int DurationSeconds { get; set; }

    public long ViewsCount { get; set; }

    public string? About { get; set; }

    public bool IsLyricsVerified { get; set; }

    public int? LyricsVerifierId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Album Album { get; set; } = null!;

    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
