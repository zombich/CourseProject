using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class VerificationType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
}
