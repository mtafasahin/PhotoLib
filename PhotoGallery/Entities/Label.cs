using System;

namespace PhotoGallery.Entities;

public class Label : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation Properties
    public ICollection<ImageLabel> ImageLabels { get; set; }
}

