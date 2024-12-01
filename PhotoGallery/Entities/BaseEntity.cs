using System;

namespace PhotoGallery.Entities;

public class BaseEntity
{
    public bool IsDeleted { get; set; } = false;
}
