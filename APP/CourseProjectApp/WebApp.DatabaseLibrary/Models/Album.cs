using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class Album
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public DateOnly ReleaseDate { get; set; }

    public string CoverImageUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();
}
