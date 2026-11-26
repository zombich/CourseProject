using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class Genre
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
